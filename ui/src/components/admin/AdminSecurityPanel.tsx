import { KeyRound } from "lucide-react";
import type { AuthUser } from "../../types/auth";
import { DefinitionList } from "./DefinitionList";
import { Panel } from "./Panel";

type AdminSecurityPanelProps = {
  accessTokenExpiresAt: string;
  verifiedUser: AuthUser | null;
  user: AuthUser;
};

export function AdminSecurityPanel({ accessTokenExpiresAt, verifiedUser, user }: AdminSecurityPanelProps) {
  const account = verifiedUser ?? user;

  return (
    <Panel title="Security" icon={KeyRound}>
      <DefinitionList
        items={[
          { term: "User", value: account.username },
          { term: "Access level", value: account.level },
          { term: "Account state", value: account.isActive ? "Active" : "Disabled" },
          { term: "Access token expires", value: new Date(accessTokenExpiresAt).toLocaleString(), mono: true }
        ]}
      />
    </Panel>
  );
}
