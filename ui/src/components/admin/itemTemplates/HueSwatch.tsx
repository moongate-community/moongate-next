import { Badge } from "@/components/ui/badge";
import type { HueSummary } from "../../../types/itemTemplates";

type HueSwatchProps = {
  hue: HueSummary;
  mode?: "compact" | "detail";
};

export function HueSwatch({ hue, mode = "compact" }: HueSwatchProps) {
  const label = hue.isNone ? "None" : hue.isKnown ? hue.hex : "Unknown";

  if (mode === "compact") {
    return (
      <span className="inline-flex min-w-[80px] items-center gap-1.5 text-xs text-fg-muted" title={`${hue.hex} ${hue.name}`}>
        <span className="h-3 w-8 overflow-hidden rounded-sm border border-border bg-muted" aria-hidden>
          {hue.colors.length > 0 && (
            <span
              className="block h-full w-full"
              style={{ background: `linear-gradient(90deg, ${hue.colors.map((color) => color.hex).join(", ")})` }}
            />
          )}
        </span>
        <span className="font-mono text-[11px]">{label}</span>
      </span>
    );
  }

  return (
    <div className="grid gap-2">
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <span className="font-mono text-[13px] font-medium text-fg">{hue.hex}</span>
        <span className="text-fg-muted">{hue.name}</span>
        {hue.isNone && (
          <Badge variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">None</Badge>
        )}
        {!hue.isKnown && (
          <Badge variant="outline" className="rounded-md border-transparent bg-warning/10 px-1.5 py-0.5 text-[11px] font-medium text-warning">
            Unknown hue
          </Badge>
        )}
      </div>
      {hue.colors.length > 0 && (
        <div
          className="grid overflow-hidden rounded-md border border-border"
          style={{ gridTemplateColumns: "repeat(32, minmax(0, 1fr))" }}
        >
          {hue.colors.map((color) => (
            <span
              key={color.index}
              className="h-4"
              title={`${color.index}: ${color.hex}`}
              style={{ backgroundColor: color.hex }}
            />
          ))}
        </div>
      )}
    </div>
  );
}
