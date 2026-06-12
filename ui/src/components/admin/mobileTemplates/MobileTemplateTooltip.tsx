import type { MobileTemplateSummary } from "../../../types/mobileTemplates";
import { notorietyColor } from "../../../lib/notorietyColors";

/** Wowhead-style hover card for a mobile template, mirroring the item tooltip. */
export function MobileTemplateTooltip({ template }: { template: MobileTemplateSummary }) {
  return (
    <div className="min-w-[250px] max-w-[310px] rounded-sm border border-[#59627a] bg-[#07091a] p-3 text-[12px] leading-snug text-[#f8fafc] shadow-[0_0_0_1px_rgba(0,0,0,0.75),0_14px_34px_rgba(0,0,0,0.65)]">
      <div className="mb-2 flex items-start gap-2">
        {template.imageUrl ? (
          <div className="grid h-10 w-10 shrink-0 place-items-center rounded-sm border border-[#4c5368] bg-[#151829]">
            <img src={template.imageUrl} alt="" className="h-8 w-8 object-contain [image-rendering:pixelated]" />
          </div>
        ) : null}
        <div className="min-w-0">
          <div className="truncate text-sm font-semibold" style={{ color: notorietyColor(template.notoriety) }}>
            {template.name}
          </div>
          {template.title ? <div className="mt-0.5 truncate text-[#ffd100]">{template.title}</div> : null}
        </div>
      </div>

      <div className="space-y-1 border-t border-[#273047] pt-2">
        <TooltipRow label="Template" value={template.id} mono />
        <TooltipRow label="Body" value={template.bodyHex} mono />
        <TooltipRow label="Gender" value={template.gender} />
        <TooltipRow label="Notoriety" value={template.notoriety} />
        <TooltipRow label="Brain" value={template.brain} />
        <TooltipRow label="Karma / Fame" value={`${template.karma} / ${template.fame}`} mono />
        {template.factionId ? <TooltipRow label="Faction" value={template.factionId} mono /> : null}
        <TooltipRow label="Equipment" value={String(template.equipmentCount)} mono />
        <TooltipRow label="Loot tables" value={String(template.lootTablesCount)} mono />
        {template.tags.length > 0 ? <TooltipRow label="Tags" value={template.tags.join(", ")} /> : null}
      </div>
    </div>
  );
}

function TooltipRow({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex justify-between gap-4">
      <span className="text-[#9ca3af]">{label}</span>
      <span className={`min-w-0 truncate text-[#ffffff] ${mono ? "font-mono" : ""}`}>{value}</span>
    </div>
  );
}
