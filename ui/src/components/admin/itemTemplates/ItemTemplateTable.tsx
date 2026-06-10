import type { ItemTemplateSummary } from "../../../types/itemTemplates";
import { HueSwatch } from "./HueSwatch";
import { ItemImageCell } from "./ItemImageCell";
import { RarityBadge } from "./RarityBadge";

type ItemTemplateTableProps = {
  templates: ItemTemplateSummary[];
  selectedId: string | null;
  onSelect: (template: ItemTemplateSummary) => void;
};

export function ItemTemplateTable({ templates, selectedId, onSelect }: ItemTemplateTableProps) {
  if (templates.length === 0) {
    return (
      <p className="m-0 rounded-md bg-muted p-4 text-[13px] leading-relaxed text-fg-muted">
        No item templates match this search.
      </p>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left text-[11px] font-bold uppercase tracking-wide text-fg-subtle">
            <th className="px-3 py-2">Art</th>
            <th className="px-3 py-2">ID</th>
            <th className="px-3 py-2">Name</th>
            <th className="px-3 py-2">Item</th>
            <th className="px-3 py-2">Hue</th>
            <th className="px-3 py-2">Rarity</th>
            <th className="px-3 py-2">Layer</th>
            <th className="px-3 py-2">Tags</th>
            <th className="px-3 py-2">Abstract</th>
          </tr>
        </thead>
        <tbody>
          {templates.map((template) => (
            <tr
              key={template.id}
              onClick={() => onSelect(template)}
              className={`cursor-pointer border-b border-border/60 transition-colors duration-150 hover:bg-muted/70 ${selectedId === template.id ? "bg-muted" : ""}`}
            >
              <td className="px-3 py-2">
                <ItemImageCell src={template.imageUrl} alt={template.name || template.id} />
              </td>
              <td className="px-3 py-2 font-mono text-xs font-semibold text-fg">{template.id}</td>
              <td className="px-3 py-2 font-semibold text-fg">{template.name}</td>
              <td className="px-3 py-2 font-mono text-xs text-fg-muted">{template.itemIdHex}</td>
              <td className="px-3 py-2">
                <HueSwatch hue={template.hue} />
              </td>
              <td className="px-3 py-2">
                <RarityBadge rarity={template.rarity} />
              </td>
              <td className="px-3 py-2 text-xs text-fg-muted">{template.layer ?? "-"}</td>
              <td className="px-3 py-2">
                <div className="flex max-w-[220px] flex-wrap gap-1">
                  {template.tags.map((tag) => (
                    <span key={tag} className="rounded-full bg-muted px-2 py-0.5 text-[11px] font-semibold text-fg-muted">
                      {tag}
                    </span>
                  ))}
                </div>
              </td>
              <td className="px-3 py-2 text-xs font-semibold text-fg-muted">{template.isAbstract ? "Yes" : "No"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
