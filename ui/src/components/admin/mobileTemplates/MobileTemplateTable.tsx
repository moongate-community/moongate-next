import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";
import type { MobileTemplateSummary } from "../../../types/mobileTemplates";
import { BodyImageCell } from "./BodyImageCell";
import { NotorietyBadge } from "./NotorietyBadge";

type MobileTemplateTableProps = {
  templates: MobileTemplateSummary[];
  selectedId: string | null;
  onSelect: (id: string) => void;
};

export function MobileTemplateTable({ templates, selectedId, onSelect }: MobileTemplateTableProps) {
  if (templates.length === 0) {
    return (
      <p className="m-0 rounded-md border border-dashed border-border bg-bg p-6 text-center text-[13px] leading-relaxed text-fg-muted">
        No mobile templates match this search.
      </p>
    );
  }

  return (
    <div className="overflow-x-auto rounded-md border border-border bg-surface">
      <Table>
        <TableHeader>
          <TableRow className="border-border bg-surface-raised text-left text-[11px] font-medium text-fg-subtle hover:bg-surface-raised">
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Art</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Name</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Body</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Gender</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Notoriety</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Karma / Fame</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Tags</TableHead>
            <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Abstract</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {templates.map((template) => (
            <TableRow
              key={template.id}
              onClick={() => onSelect(template.id)}
              className={cn(
                "cursor-pointer border-border/70 transition-colors duration-150 last:border-b-0 hover:bg-muted/70",
                selectedId === template.id && "bg-muted"
              )}
            >
              <TableCell className="px-2.5 py-1.5">
                <BodyImageCell imageUrl={template.imageUrl} body={template.body} bodyHex={template.bodyHex} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <span className="font-medium text-fg">{template.name || template.id}</span>
                {template.title ? (
                  <p className="m-0 mt-0.5 text-[11px] text-fg-muted">{template.title}</p>
                ) : (
                  <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.id}</p>
                )}
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <span className="font-mono text-xs font-medium text-fg">{template.body}</span>
                <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.bodyHex}</p>
              </TableCell>
              <TableCell className="px-2.5 py-1.5 text-xs text-fg-muted">{template.gender}</TableCell>
              <TableCell className="px-2.5 py-1.5">
                <NotorietyBadge notoriety={template.notoriety} />
              </TableCell>
              <TableCell className="px-2.5 py-1.5">
                <span className="font-mono text-xs font-medium text-fg">{template.karma}</span>
                <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.fame}</p>
              </TableCell>
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
