import { useCallback, useEffect, useState } from "react";
import { Plus, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
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
        <Button
          type="button"
          onClick={() => { setDialogError(null); setDialog({ kind: "create" }); }}
          size="sm"
          className="min-h-[34px] gap-1.5 text-[13px] font-semibold"
        >
          <Plus size={16} aria-hidden />
          New user
        </Button>
      }
    >
      <div className="grid gap-4">
        <div className="relative">
          <Search size={16} aria-hidden className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-fg-subtle" />
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search username or email…"
            aria-label="Search users"
            className="h-10 bg-surface pl-9 pr-3 text-sm text-fg"
          />
        </div>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-semibold text-danger">{error}</p>}

        {loading ? (
          <div className="grid gap-2 rounded-md bg-muted p-4">
            <Skeleton className="h-4 w-40" />
            <Skeleton className="h-24 w-full" />
          </div>
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
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
              className="min-h-[32px] border-border bg-surface px-2.5 font-semibold text-fg hover:bg-muted"
            >
              Prev
            </Button>
            <span className="font-mono">Page {page} of {totalPages}</span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
              className="min-h-[32px] border-border bg-surface px-2.5 font-semibold text-fg hover:bg-muted"
            >
              Next
            </Button>
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
