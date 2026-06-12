import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { LootTemplateNodeSummary } from "../../../types/lootTemplates";
import { rarityColor } from "../../../lib/rarityColors";

type LootContainerPreviewProps = {
  items: LootTemplateNodeSummary[];
};

function amountText(item: LootTemplateNodeSummary): string {
  return item.amountMin === item.amountMax ? String(item.amountMin) : `${item.amountMin}-${item.amountMax}`;
}

function chanceText(chance: number): string {
  return `${Math.round(chance * 1000) / 10}%`;
}

function LootItemTooltip({ item }: { item: LootTemplateNodeSummary }) {
  return (
    <div className="min-w-[250px] max-w-[310px] rounded-sm border border-[#59627a] bg-[#07091a] p-3 text-[12px] leading-snug text-[#f8fafc] shadow-[0_0_0_1px_rgba(0,0,0,0.75),0_14px_34px_rgba(0,0,0,0.65)]">
      <div className="mb-2 flex items-start gap-2">
        {item.imageUrl ? (
          <div className="grid h-10 w-10 shrink-0 place-items-center rounded-sm border border-[#4c5368] bg-[#151829]">
            <img
              src={item.imageUrl}
              alt=""
              className="h-8 w-8 object-contain [image-rendering:pixelated]"
            />
          </div>
        ) : null}
        <div className="min-w-0">
          <div className="truncate text-sm font-semibold" style={{ color: rarityColor(item.rarity) }}>
            {item.label}
          </div>
          {item.rarity ? <div className="mt-0.5 text-[#ffd100]">{item.rarity}</div> : null}
        </div>
      </div>

      <div className="space-y-1 border-t border-[#273047] pt-2">
        {item.itemTemplateId ? (
          <div className="flex justify-between gap-4">
            <span className="text-[#9ca3af]">Template</span>
            <span className="min-w-0 truncate font-mono text-[#ffffff]">{item.itemTemplateId}</span>
          </div>
        ) : null}
        {item.itemIdHex ? (
          <div className="flex justify-between gap-4">
            <span className="text-[#9ca3af]">Item ID</span>
            <span className="font-mono text-[#ffffff]">{item.itemIdHex}</span>
          </div>
        ) : null}
        <div className="flex justify-between gap-4">
          <span className="text-[#9ca3af]">Chance</span>
          <span className="font-mono text-[#ffffff]">{chanceText(item.chance)}</span>
        </div>
        <div className="flex justify-between gap-4">
          <span className="text-[#9ca3af]">Amount</span>
          <span className="font-mono text-[#ffffff]">{amountText(item)}</span>
        </div>
        <div className="flex justify-between gap-4">
          <span className="text-[#9ca3af]">Stackable</span>
          <span className={item.stackable ? "text-[#1eff00]" : "text-[#ffffff]"}>
            {item.stackable ? "Yes" : "No"}
          </span>
        </div>
      </div>
    </div>
  );
}

export function LootContainerPreview({ items }: LootContainerPreviewProps) {
  const cells = items.slice(0, 24);

  return (
    <div className="mx-auto w-full max-w-[460px] rounded-md border border-[#6f5530] bg-[#21180f] p-3 shadow-inner">
      <div className="mb-2 text-center text-[11px] font-medium uppercase text-[#d6b576]">
        Container preview
      </div>
      <div className="grid grid-cols-6 gap-1.5">
        {Array.from({ length: 24 }).map((_, index) => {
          const item = cells[index];

          return (
            <div key={index} className="grid aspect-square place-items-center rounded-sm bg-[#352514]">
              {item?.imageUrl ? (
                <Tooltip>
                  <TooltipTrigger asChild>
                    <button
                      type="button"
                      aria-label={item.label}
                      className="grid h-full w-full cursor-help place-items-center rounded-sm border border-transparent transition-colors hover:border-[#d6b576]/60 hover:bg-[#4a351c] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d6b576]"
                    >
                      <img
                        src={item.imageUrl}
                        alt=""
                        className="h-8 w-8 object-contain [image-rendering:pixelated]"
                      />
                    </button>
                  </TooltipTrigger>
                  <TooltipContent
                    side="right"
                    sideOffset={10}
                    className="border-0 bg-transparent p-0 text-left text-[#f8fafc] shadow-none [&>svg]:bg-[#07091a] [&>svg]:fill-[#07091a]"
                  >
                    <LootItemTooltip item={item} />
                  </TooltipContent>
                </Tooltip>
              ) : null}
            </div>
          );
        })}
      </div>
    </div>
  );
}
