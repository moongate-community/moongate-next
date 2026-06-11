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

      <CardContent className="grid gap-4 p-4 xl:grid-cols-[minmax(0,1fr)_280px]">
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
              {template.nodes.map((node) => (
                <tr key={node.id} className="border-b border-border">
                  <td className="border-b border-border px-2 py-2">
                    <div className="flex min-w-[220px] items-center gap-2" style={{ paddingLeft: node.depth * 14 }}>
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
                        {node.itemTemplateId && (
                          <span className="truncate font-mono text-[11px] text-fg-subtle">{node.itemTemplateId}</span>
                        )}
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
                  <td className="border-b border-border px-2 py-2 font-mono text-xs text-fg-muted">{node.weight}</td>
                  <td className="border-b border-border px-2 py-2 font-mono text-xs text-fg-muted">
                    {amountText(node)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <LootContainerPreview items={template.previewItems} />
      </CardContent>
    </Card>
  );
}
