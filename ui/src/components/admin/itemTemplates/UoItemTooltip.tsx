import type { UoItemSummary } from "../../../types/uoItems";

/** Wowhead-style hover card for a raw UO item (tiledata), mirroring the template tooltips. */
export function UoItemTooltip({ item }: { item: UoItemSummary }) {
  return (
    <div className="min-w-[250px] max-w-[310px] rounded-sm border border-[#59627a] bg-[#07091a] p-3 text-[12px] leading-snug text-[#f8fafc] shadow-[0_0_0_1px_rgba(0,0,0,0.75),0_14px_34px_rgba(0,0,0,0.65)]">
      <div className="mb-2 flex items-start gap-2">
        {item.imageUrl ? (
          <div className="grid h-10 w-10 shrink-0 place-items-center rounded-sm border border-[#4c5368] bg-[#151829]">
            <img src={item.imageUrl} alt="" className="h-8 w-8 object-contain [image-rendering:pixelated]" />
          </div>
        ) : null}
        <div className="min-w-0">
          <div className="truncate text-sm font-semibold">{item.name || item.itemIdHex}</div>
          <div className="mt-0.5 font-mono text-[#ffd100]">{item.itemIdHex}</div>
        </div>
      </div>

      <div className="space-y-1 border-t border-[#273047] pt-2">
        <TooltipRow label="Weight" value={String(item.weight)} mono />
        <TooltipRow label="Height" value={String(item.height)} mono />
        <TooltipRow label="Quality" value={String(item.quality)} mono />
        <TooltipRow label="Value" value={String(item.value)} mono />
        <TooltipRow label="Animation" value={String(item.animation)} mono />
        {item.flags.length > 0 ? <TooltipRow label="Flags" value={item.flags.join(", ")} /> : null}
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
