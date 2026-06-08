import { KeyRound } from "lucide-react";
import type { AuthUser } from "../../types/auth";

type AdminSecurityPanelProps = {
  accessTokenExpiresAt: string;
  verifiedUser: AuthUser | null;
  user: AuthUser;
};

export function AdminSecurityPanel({ accessTokenExpiresAt, verifiedUser, user }: AdminSecurityPanelProps) {
  const account = verifiedUser ?? user;

  return (
    <article className="admin-panel">
      <header>
        <KeyRound size={20} aria-hidden />
        <h3>Security</h3>
      </header>
      <dl className="admin-definition-list">
        <div>
          <dt>User</dt>
          <dd>{account.username}</dd>
        </div>
        <div>
          <dt>Access level</dt>
          <dd>{account.level}</dd>
        </div>
        <div>
          <dt>Account state</dt>
          <dd>{account.isActive ? "Active" : "Disabled"}</dd>
        </div>
        <div>
          <dt>Access token expires</dt>
          <dd>{new Date(accessTokenExpiresAt).toLocaleString()}</dd>
        </div>
      </dl>
    </article>
  );
}
