import type { ReactNode } from "react";
import { Box } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { ItemTemplateDetail } from "../../../types/itemTemplates";
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

type PropertyItem = {
  term: string;
  value: ReactNode;
  mono?: boolean;
};

export function ItemTemplateDetailPanel({ template, loading, error }: ItemTemplateDetailPanelProps) {
  if (loading) {
    return (
      <Card className="rounded-md border-border bg-surface py-0 shadow-none">
        <CardContent className="grid gap-3 p-4">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-32 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="rounded-md border-danger/20 bg-danger/10 py-0 text-sm font-medium text-danger shadow-none">
        <CardContent className="p-4">
          {error}
        </CardContent>
      </Card>
    );
  }

  if (!template) {
    return (
      <Card className="rounded-md border-dashed border-border bg-bg py-0 text-sm text-fg-muted shadow-none">
        <CardContent className="p-4">
          <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted">
            <Box size={17} aria-hidden />
          </div>
          <p className="m-0 font-medium text-fg">Select an item template</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="gap-0 rounded-md border-border bg-surface py-0 shadow-none">
      <CardHeader className="grid gap-3 border-b border-border p-4 text-center">
        <div className="mx-auto">
          <ItemImageCell src={template.imageUrl} alt={template.name || template.id} size="hero" />
        </div>
        <div className="grid min-w-0 gap-2">
          <div className="min-w-0">
            <h3 className="m-0 truncate text-base font-semibold text-fg">{template.name || template.id}</h3>
            <p className="m-0 mt-1 truncate font-mono text-xs text-fg-muted">
              {template.id} · {template.itemIdHex}
            </p>
          </div>
          <div className="flex flex-wrap justify-center gap-1.5">
            <RarityBadge rarity={template.rarity} mode="detail" />
            <Badge variant="outline" className="rounded-md border-border bg-bg px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
              {template.visibility}
            </Badge>
            {template.layer && (
              <Badge variant="outline" className="rounded-md border-border bg-bg px-1.5 py-0.5 font-mono text-[11px] font-medium text-fg-muted">
                {template.layer}
              </Badge>
            )}
            {template.isAbstract && (
              <Badge variant="outline" className="rounded-md border-border bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                Abstract
              </Badge>
            )}
          </div>
          {template.comment && <p className="m-0 text-left text-[13px] leading-relaxed text-fg-muted">{template.comment}</p>}
        </div>
      </CardHeader>

      <CardContent className="grid gap-4 p-4">
        <InspectorSection title="Identity">
          <PropertyRows
            items={[
              { term: "Base item", value: template.baseItem ?? "-", mono: true },
              { term: "Script", value: template.scriptId || "-", mono: true },
              { term: "Visibility", value: template.visibility },
              { term: "Abstract", value: template.isAbstract ? "Yes" : "No" }
            ]}
          />
        </InspectorSection>

        <InspectorSection title="Gameplay">
          <PropertyRows
            items={[
              { term: "Layer", value: template.layer ?? "-" },
              { term: "Amount", value: String(template.amount), mono: true },
              { term: "Weight", value: String(template.weight), mono: true },
              { term: "Movable", value: template.isMovable ? "Yes" : "No" },
              { term: "Stackable", value: template.isStackable ? "Yes" : "No" },
              { term: "Gump", value: template.gumpId?.toString() ?? "-", mono: true }
            ]}
          />
        </InspectorSection>

        <InspectorSection title="Economy">
          <ItemValueDisplay value={template.value} mode="detail" />
        </InspectorSection>

        <InspectorSection title="Visual">
          <div className="grid gap-3">
            <HueSwatch hue={template.hue} mode="detail" />
            <GraphicVariantsPanel variants={template.graphicVariants} />
          </div>
        </InspectorSection>

        <InspectorSection title="Tags">
          <div className="flex min-h-6 flex-wrap gap-1.5">
            {template.tags.length === 0 ? (
              <span className="text-xs text-fg-muted">No tags</span>
            ) : (
              template.tags.map((tag) => (
                <Badge key={tag} variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  {tag}
                </Badge>
              ))
            )}
          </div>
        </InspectorSection>

        <InspectorSection title="Params">
          {template.params.length === 0 ? (
            <span className="text-xs text-fg-muted">No params</span>
          ) : (
            <div className="grid divide-y divide-border">
              {template.params.map((param) => (
                <div key={param.key} className="grid gap-1 py-2 first:pt-0 last:pb-0">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{param.key}</span>
                    <span className="text-[11px] font-medium text-fg-muted">{param.type}</span>
                  </div>
                  <p className="m-0 mt-1 break-all font-mono text-xs text-fg-muted">{param.value}</p>
                </div>
              ))}
            </div>
          )}
        </InspectorSection>
      </CardContent>
    </Card>
  );
}

function InspectorSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="grid gap-2">
      <h4 className="m-0 text-[11px] font-semibold uppercase text-fg-subtle">{title}</h4>
      <div className="rounded-md border border-border bg-bg px-3 py-2">{children}</div>
    </section>
  );
}

function PropertyRows({ items }: { items: PropertyItem[] }) {
  return (
    <dl className="m-0 grid divide-y divide-border">
      {items.map((item) => (
        <div key={item.term} className="grid grid-cols-[88px_minmax(0,1fr)] gap-3 py-2 first:pt-0 last:pb-0">
          <dt className="text-[11px] font-medium leading-snug text-fg-subtle">{item.term}</dt>
          <dd className={`m-0 min-w-0 break-words text-[12px] font-medium leading-snug text-fg ${item.mono ? "font-mono" : ""}`}>
            {item.value}
          </dd>
        </div>
      ))}
    </dl>
  );
}
