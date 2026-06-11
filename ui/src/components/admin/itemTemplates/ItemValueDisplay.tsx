import { Coins } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { ItemTemplateValueSummary } from "../../../types/itemTemplates";

type ItemValueDisplayProps = {
  value: ItemTemplateValueSummary | null;
  mode?: "compact" | "detail";
};

const goldFormatter = new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 });
const multiplierFormatter = new Intl.NumberFormat("en-US", {
  maximumFractionDigits: 2,
  minimumFractionDigits: 1
});

export function ItemValueDisplay({ value, mode = "compact" }: ItemValueDisplayProps) {
  if (!value) {
    return <span className="text-xs text-fg-subtle">-</span>;
  }

  if (mode === "detail") {
    return (
      <div className="grid gap-2">
        <div className="flex items-center justify-between gap-2">
          <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-fg">
            <Coins size={14} aria-hidden className="text-warning" />
            Gold value
          </span>
          <Badge variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 font-mono text-[11px] font-semibold text-fg-muted">
            x{multiplierFormatter.format(value.rarityMultiplier)}
          </Badge>
        </div>
        <div className="grid grid-cols-2 gap-2 text-xs">
          <ValueCell label="Buy" base={value.buy} effective={value.effectiveBuy} />
          <ValueCell label="Sell" base={value.sell} effective={value.effectiveSell} />
        </div>
      </div>
    );
  }

  return (
    <Badge
      variant="outline"
      className="gap-1.5 rounded-md border-transparent bg-muted px-1.5 py-0.5 font-mono text-[11px] font-semibold text-fg"
      title={`Buy ${formatGold(value.effectiveBuy)} · Sell ${formatGold(value.effectiveSell)} · x${multiplierFormatter.format(value.rarityMultiplier)}`}
    >
      <Coins size={12} aria-hidden className="text-warning" />
      {formatGold(value.effectiveBuy)}
    </Badge>
  );
}

function ValueCell({ label, base, effective }: { label: string; base: number; effective: number }) {
  return (
    <div className="rounded-md border border-border bg-surface px-2 py-2">
      <div className="text-[11px] font-medium text-fg-subtle">{label}</div>
      <div className="mt-1 font-mono text-sm font-semibold text-fg">{formatGold(effective)}</div>
      <div className="mt-0.5 font-mono text-[11px] text-fg-muted">base {formatGold(base)}</div>
    </div>
  );
}

function formatGold(value: number): string {
  return goldFormatter.format(value);
}
