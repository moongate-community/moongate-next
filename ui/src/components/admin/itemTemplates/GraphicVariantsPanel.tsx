import { Card } from "@/components/ui/card";
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
    <section>
      <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Graphic variants</h4>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
        {variants.map((variant) => (
          <Card key={variant.itemId} className="flex flex-row items-center gap-2 rounded-md border-border bg-bg p-2 py-2 shadow-none">
            <ItemImageCell src={variant.imageUrl} alt={variant.itemIdHex} />
            <span className="min-w-0 truncate font-mono text-xs font-medium text-fg-muted">{variant.itemIdHex}</span>
          </Card>
        ))}
      </div>
    </section>
  );
}
