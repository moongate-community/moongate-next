import { KeyRound, Lock, Pencil, Trash2, Unlock } from "lucide-react";
import type { ReactNode } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
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

export function UserTable({ users, onEdit, onToggleActive, onResetPassword, onDelete }: UserTableProps) {
  if (users.length === 0) {
    return (
      <p className="m-0 rounded-md bg-muted p-4 text-[13px] leading-relaxed text-fg-muted">No users match this search.</p>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow className="border-border text-left text-[11px] font-bold uppercase tracking-wide text-fg-subtle hover:bg-transparent">
          <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Username</TableHead>
          <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Email</TableHead>
          <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Level</TableHead>
          <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Status</TableHead>
          <TableHead className="px-3 py-2 text-right text-[11px] font-bold text-fg-subtle">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
          {users.map((user) => (
            <TableRow key={user.id} className="border-border/60">
              <TableCell className="px-3 py-2 font-semibold text-fg">{user.username}</TableCell>
              <TableCell className="px-3 py-2 font-mono text-xs text-fg-muted">{user.email}</TableCell>
              <TableCell className="px-3 py-2">
                <Badge variant="outline" className={cn("border-transparent px-2 py-0.5 text-[11px] font-semibold", levelClass[user.level] ?? levelClass.Player)}>
                  {user.level}
                </Badge>
              </TableCell>
              <TableCell className="px-3 py-2">
                <Badge variant="outline" className="gap-1.5 rounded-md border-transparent bg-transparent px-0 text-xs font-semibold">
                  <span className={`h-2 w-2 rounded-full ${user.isActive ? "bg-success" : "bg-danger"}`} aria-hidden />
                  <span className={user.isActive ? "text-success" : "text-danger"}>{user.isActive ? "Active" : "Locked"}</span>
                </Badge>
              </TableCell>
              <TableCell className="px-3 py-2">
                <div className="flex items-center justify-end gap-1">
                  <IconAction label="Edit" ariaLabel={`Edit ${user.username}`} onClick={() => onEdit(user)}>
                    <Pencil size={16} aria-hidden />
                  </IconAction>
                  <IconAction
                    label={user.isActive ? "Lock" : "Unlock"}
                    ariaLabel={user.isActive ? `Lock ${user.username}` : `Unlock ${user.username}`}
                    onClick={() => onToggleActive(user)}
                  >
                    {user.isActive ? <Lock size={16} aria-hidden /> : <Unlock size={16} aria-hidden />}
                  </IconAction>
                  <IconAction label="Reset password" ariaLabel={`Reset password for ${user.username}`} onClick={() => onResetPassword(user)}>
                    <KeyRound size={16} aria-hidden />
                  </IconAction>
                  <IconAction label="Delete" ariaLabel={`Delete ${user.username}`} destructive onClick={() => onDelete(user)}>
                    <Trash2 size={16} aria-hidden />
                  </IconAction>
                </div>
              </TableCell>
            </TableRow>
          ))}
      </TableBody>
    </Table>
  );
}

function IconAction({
  ariaLabel,
  children,
  destructive,
  label,
  onClick
}: {
  ariaLabel: string;
  children: ReactNode;
  destructive?: boolean;
  label: string;
  onClick: () => void;
}) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          onClick={onClick}
          aria-label={ariaLabel}
          className={cn("text-fg-muted hover:bg-muted hover:text-fg", destructive && "hover:text-danger")}
        >
          {children}
        </Button>
      </TooltipTrigger>
      <TooltipContent side="bottom" sideOffset={8}>{label}</TooltipContent>
    </Tooltip>
  );
}
