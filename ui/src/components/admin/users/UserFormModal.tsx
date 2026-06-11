import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
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

export function UserFormModal({ mode, user, busy, error, onCreate, onUpdate, onCancel }: UserFormModalProps) {
  const [username, setUsername] = useState(user?.username ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [password, setPassword] = useState("");
  const [level, setLevel] = useState<AdminUserLevel>(user?.level ?? "Player");
  const [isActive, setIsActive] = useState(user?.isActive ?? true);

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode === "create") {
      onCreate({ username, email, password, level, isActive });
    } else {
      onUpdate({ email, level });
    }
  }

  return (
    <Dialog open onOpenChange={(open) => { if (!open && !busy) onCancel(); }}>
      <DialogContent className="max-w-md bg-surface" showCloseButton={!busy}>
        <DialogHeader>
          <DialogTitle>{mode === "create" ? "New user" : `Edit ${user?.username}`}</DialogTitle>
          <DialogDescription className="sr-only">
            {mode === "create" ? "Create user account" : "Edit user account"}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={submit} className="grid gap-4">
          <div className="grid gap-3">
            {mode === "create" && (
              <div className="grid gap-1.5">
                <Label htmlFor="user-form-username" className="text-[13px] font-semibold text-fg-muted">
                  Username
                </Label>
                <Input
                  id="user-form-username"
                  value={username}
                  onChange={(event) => setUsername(event.target.value)}
                  className="bg-surface text-sm text-fg"
                />
              </div>
            )}

            <div className="grid gap-1.5">
              <Label htmlFor="user-form-email" className="text-[13px] font-semibold text-fg-muted">
                Email
              </Label>
              <Input
                id="user-form-email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="bg-surface text-sm text-fg"
              />
            </div>

            {mode === "create" && (
              <div className="grid gap-1.5">
                <Label htmlFor="user-form-password" className="text-[13px] font-semibold text-fg-muted">
                  Password
                </Label>
                <Input
                  id="user-form-password"
                  type="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  className="bg-surface text-sm text-fg"
                />
              </div>
            )}

            <div className="grid gap-1.5">
              <Label htmlFor="user-form-level" className="text-[13px] font-semibold text-fg-muted">
                Level
              </Label>
              <Select value={level} onValueChange={(value) => setLevel(value as AdminUserLevel)}>
                <SelectTrigger id="user-form-level" className="w-full bg-surface">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LEVELS.map((value) => (
                    <SelectItem key={value} value={value}>{value}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {mode === "create" && (
              <Label className="flex items-center gap-2 text-[13px] font-semibold text-fg-muted">
                <Checkbox checked={isActive} onCheckedChange={(checked) => setIsActive(checked === true)} />
                Active
              </Label>
            )}
          </div>

          {error && <p className="m-0 text-[13px] font-semibold text-danger">{error}</p>}

          <DialogFooter>
            <Button
              type="button"
              onClick={onCancel}
              disabled={busy}
              variant="outline"
              className="border-border bg-surface text-[13px] font-semibold text-fg hover:bg-muted"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={busy}
              className="text-[13px] font-semibold"
            >
              {mode === "create" ? "Create user" : "Save changes"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
