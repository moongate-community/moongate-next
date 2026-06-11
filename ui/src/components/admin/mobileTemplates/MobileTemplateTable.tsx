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
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border bg-surface-raised text-left text-[11px] font-medium text-fg-subtle">
            <th className="px-2.5 py-2">Art</th>
            <th className="px-2.5 py-2">Name</th>
            <th className="px-2.5 py-2">Body</th>
            <th className="px-2.5 py-2">Gender</th>
            <th className="px-2.5 py-2">Notoriety</th>
            <th className="px-2.5 py-2">Karma / Fame</th>
            <th className="px-2.5 py-2">Tags</th>
            <th className="px-2.5 py-2">Abstract</th>
          </tr>
        </thead>
        <tbody>
          {templates.map((template) => (
            <tr
              key={template.id}
              onClick={() => onSelect(template.id)}
              className={`cursor-pointer border-b border-border/70 transition-colors duration-150 last:border-b-0 hover:bg-muted/70 ${selectedId === template.id ? "bg-muted" : ""}`}
            >
              <td className="px-2.5 py-1.5">
                <BodyImageCell imageUrl={template.imageUrl} body={template.body} bodyHex={template.bodyHex} />
              </td>
              <td className="px-2.5 py-1.5">
                <span className="font-medium text-fg">{template.name || template.id}</span>
                {template.title ? (
                  <p className="m-0 mt-0.5 text-[11px] text-fg-muted">{template.title}</p>
                ) : (
                  <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.id}</p>
                )}
              </td>
              <td className="px-2.5 py-1.5">
                <span className="font-mono text-xs font-medium text-fg">{template.body}</span>
                <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.bodyHex}</p>
              </td>
              <td className="px-2.5 py-1.5 text-xs text-fg-muted">{template.gender}</td>
              <td className="px-2.5 py-1.5">
                <NotorietyBadge notoriety={template.notoriety} />
              </td>
              <td className="px-2.5 py-1.5">
                <span className="font-mono text-xs font-medium text-fg">{template.karma}</span>
                <p className="m-0 mt-0.5 font-mono text-[11px] text-fg-muted">{template.fame}</p>
              </td>
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
