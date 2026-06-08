import type { ReactNode } from "react";

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
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4" role="dialog" aria-modal="true">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-5 shadow-raised">
        <h3 className="m-0 text-base font-bold text-fg">{title}</h3>
        <div className="mt-2 text-sm leading-relaxed text-fg-muted">{message}</div>
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
            onClick={onConfirm}
            disabled={busy}
            className={[
              "inline-flex min-h-[38px] items-center rounded-md px-3 text-[13px] font-semibold transition-opacity duration-150 disabled:opacity-60",
              destructive
                ? "bg-danger text-white hover:opacity-90"
                : "bg-accent text-accent-fg hover:opacity-90"
            ].join(" ")}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
