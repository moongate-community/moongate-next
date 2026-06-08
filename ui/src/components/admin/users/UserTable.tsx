import { KeyRound, Lock, Pencil, Trash2, Unlock } from "lucide-react";
import type { AdminUser } from "../../../types/users";

type UserTableProps = {
  users: AdminUser[];
  onEdit: (user: AdminUser) => void;
  onToggleActive: (user: AdminUser) => void;
  onResetPassword: (user: AdminUser) => void;
  onDelete: (user: AdminUser) => void;
};

const levelClass: Record<string, string> = {
  Administrator: "text-danger bg-danger/10",
  GameMaster: "text-warning bg-warning/10",
  Player: "text-fg-muted bg-muted"
};

const iconButtonClass =
  "inline-flex h-8 w-8 items-center justify-center rounded-md text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg";

export function UserTable({ users, onEdit, onToggleActive, onResetPassword, onDelete }: UserTableProps) {
  if (users.length === 0) {
    return (
      <p className="m-0 rounded-md bg-muted p-4 text-[13px] leading-relaxed text-fg-muted">No users match this search.</p>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left text-[11px] font-bold uppercase tracking-wide text-fg-subtle">
            <th className="px-3 py-2">Username</th>
            <th className="px-3 py-2">Email</th>
            <th className="px-3 py-2">Level</th>
            <th className="px-3 py-2">Status</th>
            <th className="px-3 py-2 text-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id} className="border-b border-border/60">
              <td className="px-3 py-2 font-semibold text-fg">{user.username}</td>
              <td className="px-3 py-2 font-mono text-xs text-fg-muted">{user.email}</td>
              <td className="px-3 py-2">
                <span className={`inline-flex rounded-full px-2 py-0.5 text-[11px] font-semibold ${levelClass[user.level] ?? levelClass.Player}`}>
                  {user.level}
                </span>
              </td>
              <td className="px-3 py-2">
                <span className="inline-flex items-center gap-1.5 text-xs font-semibold">
                  <span className={`h-2 w-2 rounded-full ${user.isActive ? "bg-success" : "bg-danger"}`} aria-hidden />
                  <span className={user.isActive ? "text-success" : "text-danger"}>{user.isActive ? "Active" : "Locked"}</span>
                </span>
              </td>
              <td className="px-3 py-2">
                <div className="flex items-center justify-end gap-1">
                  <button type="button" className={iconButtonClass} onClick={() => onEdit(user)} aria-label={`Edit ${user.username}`} title="Edit">
                    <Pencil size={16} aria-hidden />
                  </button>
                  <button type="button" className={iconButtonClass} onClick={() => onToggleActive(user)} aria-label={user.isActive ? `Lock ${user.username}` : `Unlock ${user.username}`} title={user.isActive ? "Lock" : "Unlock"}>
                    {user.isActive ? <Lock size={16} aria-hidden /> : <Unlock size={16} aria-hidden />}
                  </button>
                  <button type="button" className={iconButtonClass} onClick={() => onResetPassword(user)} aria-label={`Reset password for ${user.username}`} title="Reset password">
                    <KeyRound size={16} aria-hidden />
                  </button>
                  <button type="button" className={`${iconButtonClass} hover:text-danger`} onClick={() => onDelete(user)} aria-label={`Delete ${user.username}`} title="Delete">
                    <Trash2 size={16} aria-hidden />
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
