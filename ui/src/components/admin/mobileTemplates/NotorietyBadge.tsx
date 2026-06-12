import { Shield } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { notorietyStyle } from "../../../lib/notorietyColors";

type NotorietyBadgeProps = {
  notoriety: string;
  mode?: "compact" | "detail";
};

export function NotorietyBadge({ notoriety, mode = "compact" }: NotorietyBadgeProps) {
  const style = notorietyStyle(notoriety);
  const isDetail = mode === "detail";

  return (
    <Badge
      variant="outline"
      className={`relative inline-flex max-w-full items-center gap-1.5 overflow-hidden rounded-md border font-bold leading-none ${
        isDetail ? "px-2 py-1 text-xs" : "px-1.5 py-0.5 text-[11px]"
      }`}
      style={{ background: style.background, borderColor: style.border, color: style.text }}
      aria-label={`Notoriety ${style.label}`}
      title={`Notoriety: ${style.label}`}
    >
      <Shield size={isDetail ? 14 : 12} aria-hidden className="shrink-0" style={{ color: style.icon }} />
      <span className="truncate">{style.label}</span>
    </Badge>
  );
}
