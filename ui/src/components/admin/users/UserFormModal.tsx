import { useState } from "react";
import type { AdminUser, AdminUserLevel, CreateUserPayload, UpdateUserPayload } from "../../../types/users";

const LEVELS: AdminUserLevel[] = ["Player", "GameMaster", "Administrator"];

type UserFormModalProps = {
  mode: "create" | "edit";
  user?: AdminUser;
  busy?: boolean;
  error?: string | null;
  onCreate: (payload: CreateUserPayload) => void;
  onUpdate: (payload: UpdateUserPayload) => void;
  onCancel: () => void;
};

const fieldClass =
  "h-10 w-full rounded-md border border-border bg-surface px-3 text-sm text-fg outline-none focus:border-accent";
const labelClass = "grid gap-1.5 text-[13px] font-semibold text-fg-muted";

export function UserFormModal({ mode, user, busy, error, onCreate, onUpdate, onCancel }: UserFormModalProps) {
  const [username, setUsername] = useState(user?.username ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [password, setPassword] = useState("");
  const [level, setLevel] = useState<AdminUserLevel>(user?.level ?? "Player");
  const [isActive, setIsActive] = useState(user?.isActive ?? true);

  function submit() {
    if (mode === "create") {
      onCreate({ username, email, password, level, isActive });
    } else {
      onUpdate({ email, level });
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4" role="dialog" aria-modal="true">
      <div className="w-full max-w-md rounded-lg border border-border bg-surface p-5 shadow-raised">
        <h3 className="m-0 text-base font-bold text-fg">{mode === "create" ? "New user" : `Edit ${user?.username}`}</h3>

        <div className="mt-4 grid gap-3">
          {mode === "create" && (
            <label className={labelClass}>
              Username
              <input className={fieldClass} value={username} onChange={(e) => setUsername(e.target.value)} />
            </label>
          )}
          <label className={labelClass}>
            Email
            <input className={fieldClass} type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
          {mode === "create" && (
            <label className={labelClass}>
              Password
              <input className={fieldClass} type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </label>
          )}
          <label className={labelClass}>
            Level
            <select className={fieldClass} value={level} onChange={(e) => setLevel(e.target.value as AdminUserLevel)}>
              {LEVELS.map((value) => (
                <option key={value} value={value}>{value}</option>
              ))}
            </select>
          </label>
          {mode === "create" && (
            <label className="flex items-center gap-2 text-[13px] font-semibold text-fg-muted">
              <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
              Active
            </label>
          )}
        </div>

        {error && <p className="mt-3 text-[13px] font-semibold text-danger">{error}</p>}

        <div className="mt-5 flex justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={busy}
            className="inline-flex min-h-[38px] items-center rounded-md border border-border bg-surface px-3 text-[13px] font-semibold text-fg transition-colors duration-150 hover:bg-muted disabled:opacity-60"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={submit}
            disabled={busy}
            className="inline-flex min-h-[38px] items-center rounded-md bg-accent px-3 text-[13px] font-semibold text-accent-fg transition-opacity duration-150 hover:opacity-90 disabled:opacity-60"
          >
            {mode === "create" ? "Create user" : "Save changes"}
          </button>
        </div>
      </div>
    </div>
  );
}
