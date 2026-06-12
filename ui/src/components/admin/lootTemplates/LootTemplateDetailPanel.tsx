import { Boxes } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { LootTemplateDetail, LootTemplateNodeSummary } from "../../../types/lootTemplates";
import { LootContainerPreview } from "./LootContainerPreview";

type LootTemplateDetailPanelProps = {
  template: LootTemplateDetail | null;
  loading: boolean;
  error: string | null;
};

function amountText(node: LootTemplateNodeSummary): string {
  return node.amountMin === node.amountMax ? String(node.amountMin) : `${node.amountMin}-${node.amountMax}`;
}

function chanceText(chance: number): string {
  return `${Math.round(chance * 1000) / 10}%`;
}

function weightText(node: LootTemplateNodeSummary): string {
  return node.kind === "category_candidate" ? "-" : String(node.weight);
}

function LootNodeTable({
  nodes,
  emptyText,
  indent
}: {
  nodes: LootTemplateNodeSummary[];
  emptyText: string;
  indent: boolean;
}) {
  if (nodes.length === 0) {
    return (
      <p className="m-0 rounded-md border border-dashed border-border bg-bg p-4 text-[13px] text-fg-muted">
        {emptyText}
      </p>
    );
  }

  return (
    <div className="min-w-0 overflow-x-auto">
      <table className="w-full border-separate border-spacing-0 text-left text-[13px]">
        <thead className="text-[11px] uppercase text-fg-subtle">
          <tr>
            <th className="border-b border-border px-2 py-2 font-medium">Item</th>
            <th className="border-b border-border px-2 py-2 font-medium">Kind</th>
            <th className="border-b border-border px-2 py-2 font-medium">Chance</th>
            <th className="border-b border-border px-2 py-2 font-medium">Weight</th>
            <th className="border-b border-border px-2 py-2 font-medium">Amount</th>
          </tr>
        </thead>
        <tbody>
          {nodes.map((node) => (
            <tr key={node.id} className="border-b border-border">
              <td className="border-b border-border px-2 py-2">
                <div
                  className="flex min-w-[220px] items-center gap-2"
                  style={{ paddingLeft: indent ? node.depth * 14 : 0 }}
                >
                  {node.imageUrl ? (
                    <img
                      src={node.imageUrl}
                      alt={node.label}
                      className="h-8 w-8 shrink-0 object-contain [image-rendering:pixelated]"
                    />
                  ) : (
                    <span className="h-8 w-8 shrink-0 rounded-md bg-muted" />
                  )}
                  <span className="grid min-w-0">
                    <span className="truncate font-medium text-fg">{node.label}</span>
                    {node.itemTemplateId ? (
                      <span className="truncate font-mono text-[11px] text-fg-subtle">{node.itemTemplateId}</span>
                    ) : null}
                  </span>
                </div>
              </td>
              <td className="border-b border-border px-2 py-2">
                <Badge
                  variant="outline"
                  className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted"
                >
                  {node.kind}
                </Badge>
              </td>
              <td className="border-b border-border px-2 py-2 font-mono text-xs text-fg-muted">
                {chanceText(node.chance)}
              </td>
              <td className="border-b border-border px-2 py-2 font-mono text-xs text-fg-muted">
                {weightText(node)}
              </td>
              <td className="border-b border-border px-2 py-2 font-mono text-xs text-fg-muted">
                {amountText(node)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function LootTemplateDetailPanel({ template, loading, error }: LootTemplateDetailPanelProps) {
  if (loading) {
    return (
      <Card className="rounded-md border-border bg-surface py-0 shadow-none">
        <CardContent className="grid gap-3 p-4">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-64 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="rounded-md border-danger/20 bg-danger/10 py-0 text-sm font-medium text-danger shadow-none">
        <CardContent className="p-4">{error}</CardContent>
      </Card>
    );
  }

  if (!template) {
    return (
      <Card className="rounded-md border-dashed border-border bg-bg py-0 text-sm text-fg-muted shadow-none">
        <CardContent className="p-4">Select a loot template</CardContent>
      </Card>
    );
  }

  const rawNodes = template.nodes ?? [];
  const rawPreviewItems = template.previewItems ?? [];
  const definitionNodes = rawNodes.filter((node) => node.kind !== "category_candidate");
  const nodeItemOutcomes = rawNodes.filter((node) => node.itemTemplateId !== null);
  const legacyPotentialItems = nodeItemOutcomes.length >= rawPreviewItems.length ? nodeItemOutcomes : rawPreviewItems;
  const potentialItems = template.potentialItems ?? legacyPotentialItems;
  const previewItems = rawPreviewItems.length > 0 ? rawPreviewItems : potentialItems;

  return (
    <Card className="gap-0 rounded-md border-border bg-surface py-0 shadow-none">
      <CardHeader className="flex flex-row items-start gap-3 border-b border-border p-4">
        <div className="inline-flex h-9 w-9 items-center justify-center rounded-md bg-muted text-fg-muted">
          <Boxes size={18} aria-hidden />
        </div>
        <div className="min-w-0">
          <h3 className="m-0 text-base font-semibold text-fg">{template.id}</h3>
          <p className="m-0 mt-1 font-mono text-xs text-fg-muted">{template.rootNodeCount} root nodes</p>
        </div>
      </CardHeader>

      <CardContent className="grid gap-5 p-4">
        <LootContainerPreview items={previewItems} />

        <section className="grid gap-2">
          <div className="flex items-center justify-between gap-3">
            <h4 className="m-0 text-sm font-semibold text-fg">Loot definition</h4>
            <span className="font-mono text-[11px] text-fg-subtle">{definitionNodes.length} rows</span>
          </div>
          <LootNodeTable nodes={definitionNodes} emptyText="No loot definition rows." indent />
        </section>

        <section className="grid gap-2">
          <div className="flex items-center justify-between gap-3">
            <h4 className="m-0 text-sm font-semibold text-fg">Possible outcomes</h4>
            <span className="font-mono text-[11px] text-fg-subtle">{potentialItems.length} items</span>
          </div>
          <LootNodeTable nodes={potentialItems} emptyText="No possible item outcomes." indent={false} />
        </section>
      </CardContent>
    </Card>
  );
}
