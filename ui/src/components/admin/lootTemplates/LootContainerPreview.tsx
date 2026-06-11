import type { LootTemplateNodeSummary } from "../../../types/lootTemplates";

type LootContainerPreviewProps = {
  items: LootTemplateNodeSummary[];
};

export function LootContainerPreview({ items }: LootContainerPreviewProps) {
  const cells = items.slice(0, 24);

  return (
    <div className="rounded-md border border-[#6f5530] bg-[#21180f] p-3 shadow-inner">
      <div className="mb-2 text-center text-[11px] font-medium uppercase text-[#d6b576]">
        Container preview
      </div>
      <div className="grid grid-cols-6 gap-1.5">
        {Array.from({ length: 24 }).map((_, index) => {
          const item = cells[index];

          return (
            <div key={index} className="grid aspect-square place-items-center rounded-sm bg-[#352514]">
              {item?.imageUrl ? (
                <img
                  src={item.imageUrl}
                  alt={item.label}
                  className="h-8 w-8 object-contain [image-rendering:pixelated]"
                />
              ) : null}
            </div>
          );
        })}
      </div>
    </div>
  );
}
