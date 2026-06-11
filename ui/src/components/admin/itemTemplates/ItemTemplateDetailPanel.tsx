import { Box } from "lucide-react";
import type { ItemTemplateDetail } from "../../../types/itemTemplates";
import { DefinitionList } from "../DefinitionList";
import { GraphicVariantsPanel } from "./GraphicVariantsPanel";
import { HueSwatch } from "./HueSwatch";
import { ItemImageCell } from "./ItemImageCell";
import { ItemValueDisplay } from "./ItemValueDisplay";
import { RarityBadge } from "./RarityBadge";

type ItemTemplateDetailPanelProps = {
  template: ItemTemplateDetail | null;
  loading: boolean;
  error: string | null;
};

export function ItemTemplateDetailPanel({ template, loading, error }: ItemTemplateDetailPanelProps) {
  if (loading) {
    return (
      <aside className="rounded-md border border-border bg-surface p-4 text-sm font-medium text-fg-muted">
        Loading template...
      </aside>
    );
  }

  if (error) {
    return (
      <aside className="rounded-md border border-danger/20 bg-danger/10 p-4 text-sm font-medium text-danger">
        {error}
      </aside>
    );
  }

  if (!template) {
    return (
      <aside className="rounded-md border border-dashed border-border bg-bg p-4 text-sm text-fg-muted">
        <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted">
          <Box size={17} aria-hidden />
        </div>
        <p className="m-0 font-medium text-fg">Select an item template</p>
        <p className="m-0 mt-1 text-[13px] leading-relaxed">Choose a row to inspect its full read-only definition.</p>
      </aside>
    );
  }

  return (
    <aside className="rounded-md border border-border bg-surface">
      <div className="flex items-start gap-3 border-b border-border p-4">
        <ItemImageCell src={template.imageUrl} alt={template.name || template.id} size="large" />
        <div className="min-w-0">
          <h3 className="m-0 text-base font-semibold text-fg">{template.name || template.id}</h3>
          <p className="m-0 mt-1 font-mono text-xs text-fg-muted">
            {template.id} · {template.itemIdHex}
          </p>
          {template.comment && <p className="m-0 mt-2 text-[13px] leading-relaxed text-fg-muted">{template.comment}</p>}
        </div>
      </div>

      <div className="grid gap-4 p-4">
        <HueSwatch hue={template.hue} mode="detail" />
        <ItemValueDisplay value={template.value} mode="detail" />
        <GraphicVariantsPanel variants={template.graphicVariants} />
        <DefinitionList
          items={[
            { term: "Base item", value: template.baseItem ?? "-", mono: true },
            { term: "Script", value: template.scriptId || "-", mono: true },
            { term: "Visibility", value: template.visibility },
            { term: "Layer", value: template.layer ?? "-" },
            { term: "Rarity", value: <RarityBadge rarity={template.rarity} mode="detail" /> },
            { term: "Amount", value: String(template.amount), mono: true },
            { term: "Weight", value: String(template.weight), mono: true },
            { term: "Movable", value: template.isMovable ? "Yes" : "No" },
            { term: "Stackable", value: template.isStackable ? "Yes" : "No" },
            { term: "Gump", value: template.gumpId?.toString() ?? "-", mono: true },
            { term: "Abstract", value: template.isAbstract ? "Yes" : "No" }
          ]}
        />
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Tags</h4>
          <div className="flex flex-wrap gap-1.5">
            {template.tags.length === 0 ? (
              <span className="text-xs text-fg-muted">No tags</span>
            ) : (
              template.tags.map((tag) => (
                <span key={tag} className="rounded-md bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  {tag}
                </span>
              ))
            )}
          </div>
        </section>
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Params</h4>
          {template.params.length === 0 ? (
            <span className="text-xs text-fg-muted">No params</span>
          ) : (
            <div className="grid gap-2">
              {template.params.map((param) => (
                <div key={param.key} className="rounded-md border border-border bg-bg p-2">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{param.key}</span>
                    <span className="text-[11px] font-medium text-fg-muted">{param.type}</span>
                  </div>
                  <p className="m-0 mt-1 break-all font-mono text-xs text-fg-muted">{param.value}</p>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </aside>
  );
}
