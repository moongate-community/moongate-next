import type { ItemTemplateSummary } from "../../../types/itemTemplates";
import { HueSwatch } from "./HueSwatch";
import { ItemImageCell } from "./ItemImageCell";
import { ItemValueDisplay } from "./ItemValueDisplay";
import { RarityBadge } from "./RarityBadge";

type ItemTemplateTableProps = {
  templates: ItemTemplateSummary[];
  selectedId: string | null;
  onSelect: (template: ItemTemplateSummary) => void;
};

export function ItemTemplateTable({ templates, selectedId, onSelect }: ItemTemplateTableProps) {
  if (templates.length === 0) {
    return (
      <p className="m-0 rounded-md border border-dashed border-border bg-bg p-6 text-center text-[13px] leading-relaxed text-fg-muted">
        No item templates match this search.
      </p>
    );
  }

  return (
    <div className="overflow-x-auto rounded-md border border-border bg-surface">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border bg-surface-raised text-left text-[11px] font-medium text-fg-subtle">
            <th className="px-2.5 py-2">Art</th>
            <th className="px-2.5 py-2">ID</th>
            <th className="px-2.5 py-2">Name</th>
            <th className="px-2.5 py-2">Item</th>
            <th className="px-2.5 py-2">Hue</th>
            <th className="px-2.5 py-2">Rarity</th>
            <th className="px-2.5 py-2">Value</th>
            <th className="px-2.5 py-2">Layer</th>
            <th className="px-2.5 py-2">Tags</th>
            <th className="px-2.5 py-2">Abstract</th>
          </tr>
        </thead>
        <tbody>
          {templates.map((template) => (
            <tr
              key={template.id}
              onClick={() => onSelect(template)}
              className={`cursor-pointer border-b border-border/70 transition-colors duration-150 last:border-b-0 hover:bg-muted/70 ${selectedId === template.id ? "bg-muted" : ""}`}
            >
              <td className="px-2.5 py-1.5">
                <ItemImageCell src={template.imageUrl} alt={template.name || template.id} />
              </td>
              <td className="px-2.5 py-1.5 font-mono text-xs font-medium text-fg">{template.id}</td>
              <td className="px-2.5 py-1.5 font-medium text-fg">{template.name}</td>
              <td className="px-2.5 py-1.5 font-mono text-xs text-fg-muted">{template.itemIdHex}</td>
              <td className="px-2.5 py-1.5">
                <HueSwatch hue={template.hue} />
              </td>
              <td className="px-2.5 py-1.5">
                <RarityBadge rarity={template.rarity} />
              </td>
              <td className="px-2.5 py-1.5">
                <ItemValueDisplay value={template.value} />
              </td>
              <td className="px-2.5 py-1.5 text-xs text-fg-muted">{template.layer ?? "-"}</td>
              <td className="px-2.5 py-1.5">
                <div className="flex max-w-[220px] flex-wrap gap-1">
                  {template.tags.map((tag) => (
                    <span key={tag} className="rounded-md bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                      {tag}
                    </span>
                  ))}
                </div>
              </td>
              <td className="px-2.5 py-1.5 text-xs font-medium text-fg-muted">{template.isAbstract ? "Yes" : "No"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
