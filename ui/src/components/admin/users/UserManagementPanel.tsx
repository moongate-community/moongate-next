import { useCallback, useEffect, useState } from "react";
import { Plus, Search } from "lucide-react";
import {
  createUser,
  deleteUser,
  listUsers,
  resetUserPassword,
  setUserActive,
  updateUser
} from "../../../lib/adminUsersClient";
import type { AdminUser, CreateUserPayload, UpdateUserPayload } from "../../../types/users";
import { Panel } from "../Panel";
import { ConfirmDialog } from "./ConfirmDialog";
import { UserFormModal } from "./UserFormModal";
import { UserTable } from "./UserTable";

type UserManagementPanelProps = {
  accessToken: string;
};

const PAGE_SIZE = 20;

type Dialog =
  | { kind: "create" }
  | { kind: "edit"; user: AdminUser }
  | { kind: "reset"; user: AdminUser }
  | { kind: "delete"; user: AdminUser }
  | null;

export function UserManagementPanel({ accessToken }: UserManagementPanelProps) {
  const [search, setSearch] = useState("");
  const [debounced, setDebounced] = useState("");
  const [page, setPage] = useState(1);
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [dialog, setDialog] = useState<Dialog>(null);
  const [dialogBusy, setDialogBusy] = useState(false);
  const [dialogError, setDialogError] = useState<string | null>(null);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebounced(search);
      setPage(1);
    }, 300);

    return () => window.clearTimeout(timer);
  }, [search]);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await listUsers(accessToken, page, PAGE_SIZE, debounced);

      setUsers(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load users");
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken, page, debounced]);

  useEffect(() => {
    void load();
  }, [load]);

  async function runDialogAction(action: () => Promise<void>) {
    setDialogBusy(true);
    setDialogError(null);

    try {
      await action();
      setDialog(null);
      await load();
    } catch (caught) {
      setDialogError(caught instanceof Error ? caught.message : "Action failed");
    } finally {
      setDialogBusy(false);
    }
  }

  function handleCreate(payload: CreateUserPayload) {
    void runDialogAction(() => createUser(accessToken, payload).then(() => undefined));
  }

  function handleUpdate(id: string, payload: UpdateUserPayload) {
    void runDialogAction(() => updateUser(accessToken, id, payload).then(() => undefined));
  }

  function handleToggleActive(user: AdminUser) {
    void runDialogAction(() => setUserActive(accessToken, user.id, !user.isActive).then(() => undefined));
  }

  return (
    <Panel
      title="Users"
      action={
        <button
          type="button"
          onClick={() => { setDialogError(null); setDialog({ kind: "create" }); }}
          className="inline-flex min-h-[34px] items-center gap-1.5 rounded-md bg-accent px-3 text-[13px] font-semibold text-accent-fg transition-opacity duration-150 hover:opacity-90"
        >
          <Plus size={16} aria-hidden />
          New user
        </button>
      }
    >
      <div className="grid gap-4">
        <div className="relative">
          <Search size={16} aria-hidden className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-fg-subtle" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search username or email…"
            aria-label="Search users"
            className="h-10 w-full rounded-md border border-border bg-surface pl-9 pr-3 text-sm text-fg outline-none focus:border-accent"
          />
        </div>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-semibold text-danger">{error}</p>}

        {loading ? (
          <p className="m-0 rounded-md bg-muted p-4 text-[13px] font-semibold text-fg-muted">Loading users…</p>
        ) : (
          <UserTable
            users={users}
            onEdit={(user) => { setDialogError(null); setDialog({ kind: "edit", user }); }}
            onToggleActive={handleToggleActive}
            onResetPassword={(user) => { setDialogError(null); setDialog({ kind: "reset", user }); }}
            onDelete={(user) => { setDialogError(null); setDialog({ kind: "delete", user }); }}
          />
        )}

        <div className="flex items-center justify-between text-xs text-fg-muted">
          <span className="font-mono">{totalCount} users</span>
          <div className="flex items-center gap-2">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
              className="inline-flex min-h-[32px] items-center rounded-md border border-border bg-surface px-2.5 font-semibold text-fg transition-colors duration-150 hover:bg-muted disabled:opacity-50"
            >
              Prev
            </button>
            <span className="font-mono">Page {page} of {totalPages}</span>
            <button
              type="button"
              disabled={page >= totalPages}
              onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
              className="inline-flex min-h-[32px] items-center rounded-md border border-border bg-surface px-2.5 font-semibold text-fg transition-colors duration-150 hover:bg-muted disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      </div>

      {dialog?.kind === "create" && (
        <UserFormModal
          mode="create"
          busy={dialogBusy}
          error={dialogError}
          onCreate={handleCreate}
          onUpdate={() => undefined}
          onCancel={() => setDialog(null)}
        />
      )}

      {dialog?.kind === "edit" && (
        <UserFormModal
          mode="edit"
          user={dialog.user}
          busy={dialogBusy}
          error={dialogError}
          onCreate={() => undefined}
          onUpdate={(payload) => handleUpdate(dialog.user.id, payload)}
          onCancel={() => setDialog(null)}
        />
      )}

      {dialog?.kind === "reset" && (
        <ConfirmDialog
          title="Reset password"
          message={<>Reset the password for <strong>{dialog.user.username}</strong> to “changeme”? They will need it to sign in again.</>}
          confirmLabel="Reset password"
          busy={dialogBusy}
          error={dialogError}
          onConfirm={() => runDialogAction(() => resetUserPassword(accessToken, dialog.user.id, "changeme"))}
          onCancel={() => setDialog(null)}
        />
      )}

      {dialog?.kind === "delete" && (
        <ConfirmDialog
          title="Delete user"
          message={<>Permanently delete <strong>{dialog.user.username}</strong>? This cannot be undone.</>}
          confirmLabel="Delete"
          destructive
          busy={dialogBusy}
          error={dialogError}
          onConfirm={() => runDialogAction(() => deleteUser(accessToken, dialog.user.id))}
          onCancel={() => setDialog(null)}
        />
      )}
    </Panel>
  );
}
