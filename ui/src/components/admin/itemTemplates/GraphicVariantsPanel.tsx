import type { ItemTemplateGraphicVariantSummary } from "../../../types/itemTemplates";
import { ItemImageCell } from "./ItemImageCell";

type GraphicVariantsPanelProps = {
  variants: ItemTemplateGraphicVariantSummary[];
};

export function GraphicVariantsPanel({ variants }: GraphicVariantsPanelProps) {
  if (variants.length === 0) {
    return null;
  }

  return (
    <div className="grid gap-2">
      <div className="flex gap-2 overflow-x-auto pb-1">
        {variants.map((variant) => (
          <div key={variant.itemId} className="grid w-16 shrink-0 justify-items-center gap-1 rounded-md border border-border bg-bg p-1.5">
            <ItemImageCell src={variant.imageUrl} alt={variant.itemIdHex} />
            <span className="max-w-full truncate font-mono text-[10px] font-medium text-fg-muted">{variant.itemIdHex}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
