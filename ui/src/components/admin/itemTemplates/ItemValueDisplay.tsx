import { Coins } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
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
      <Card className="gap-2 rounded-md border-border bg-bg p-3 py-3 shadow-none">
        <div className="mb-2 flex items-center justify-between gap-2">
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
      </Card>
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
    <Card className="gap-0 rounded-md border-border bg-surface p-2 py-2 shadow-none">
      <div className="text-[11px] font-medium text-fg-subtle">{label}</div>
      <div className="mt-1 font-mono text-sm font-semibold text-fg">{formatGold(effective)}</div>
      <div className="mt-0.5 font-mono text-[11px] text-fg-muted">base {formatGold(base)}</div>
    </Card>
  );
}

function formatGold(value: number): string {
  return goldFormatter.format(value);
}
