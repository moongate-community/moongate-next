import type { ReactNode } from "react";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";

type ConfirmDialogProps = {
  title: string;
  message: ReactNode;
  confirmLabel: string;
  destructive?: boolean;
  busy?: boolean;
  error?: string | null;
  onConfirm: () => void;
  onCancel: () => void;
};

export function ConfirmDialog({ title, message, confirmLabel, destructive, busy, error, onConfirm, onCancel }: ConfirmDialogProps) {
  return (
    <AlertDialog open onOpenChange={(open) => { if (!open && !busy) onCancel(); }}>
      <AlertDialogContent className="bg-surface">
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription asChild>
            <div className="text-sm leading-relaxed text-fg-muted">{message}</div>
          </AlertDialogDescription>
        </AlertDialogHeader>
        {error && <p className="m-0 text-[13px] font-semibold text-danger">{error}</p>}
        <AlertDialogFooter>
          <AlertDialogCancel
            onClick={onCancel}
            disabled={busy}
            className="border-border bg-surface text-[13px] font-semibold text-fg hover:bg-muted"
          >
            Cancel
          </AlertDialogCancel>
          <Button
            type="button"
            onClick={onConfirm}
            disabled={busy}
            variant={destructive ? "destructive" : "default"}
            className="text-[13px] font-semibold"
          >
            {confirmLabel}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
