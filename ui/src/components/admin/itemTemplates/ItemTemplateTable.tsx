import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";
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
      <Table>
        <TableHeader>
          <TableRow className="border-border bg-surface-raised text-left text-[11px] font-medium text-fg-subtle hover:bg-surface-raised">
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Art</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">ID</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Name</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Item</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Hue</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Rarity</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Value</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Layer</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Tags</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Abstract</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {templates.map((template) => (
            <TableRow
              key={template.id}
              onClick={() => onSelect(template)}
              className={cn(
                "cursor-pointer border-border/70 transition-colors duration-150 last:border-b-0 hover:bg-muted/70",
                selectedId === template.id && "bg-muted"
              )}
            >
              <TableCell className="px-2.5 py-1.5">
                <ItemImageCell src={template.imageUrl} alt={template.name || template.id} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5 font-mono text-xs font-medium text-fg">{template.id}</TableCell>
              <TableCell className="px-2.5 py-1.5 font-medium text-fg">{template.name}</TableCell>
              <TableCell className="px-2.5 py-1.5 font-mono text-xs text-fg-muted">{template.itemIdHex}</TableCell>
              <TableCell className="px-2.5 py-1.5">
                <HueSwatch hue={template.hue} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <RarityBadge rarity={template.rarity} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <ItemValueDisplay value={template.value} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5 text-xs text-fg-muted">{template.layer ?? "-"}</TableCell>
              <TableCell className="px-2.5 py-1.5">
                <div className="flex max-w-[220px] flex-wrap gap-1">
                  {template.tags.map((tag) => (
                    <Badge key={tag} variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                      {tag}
                    </Badge>
                  ))}
                </div>
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <Badge variant="outline" className="rounded-md border-border bg-bg text-xs font-medium text-fg-muted">
                  {template.isAbstract ? "Yes" : "No"}
                </Badge>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
