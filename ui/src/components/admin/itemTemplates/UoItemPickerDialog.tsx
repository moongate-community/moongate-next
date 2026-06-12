import { useCallback, useState } from "react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { listUoItems } from "../../../lib/adminUoItemsClient";
import type { UoItemSummary } from "../../../types/uoItems";
import { EntityPickerDialog } from "../common/EntityPickerDialog";
import { UoItemTooltip } from "./UoItemTooltip";

type UoItemPickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (item: UoItemSummary) => void;
  title?: string;
};

const flagOptions = ["Container", "Weapon", "Armor", "Wearable", "Door", "Surface", "Background", "Wall"];

/** Reusable modal that browses raw UO items (tiledata) and returns the chosen item. */
export function UoItemPickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  title = "Select UO item"
}: UoItemPickerDialogProps) {
  const [flag, setFlag] = useState("all");

  const loadPage = useCallback(
    (page: number, search: string) =>
      listUoItems(accessToken, { page, pageSize: 40, search, flag: flag === "all" ? "" : flag }),
    [accessToken, flag]
  );

  return (
    <EntityPickerDialog
      open={open}
      onOpenChange={onOpenChange}
      onSelect={onSelect}
      loadPage={loadPage}
      getKey={(item) => item.itemIdHex}
      getImageUrl={(item) => item.imageUrl}
      getLabel={(item) => item.name || item.itemIdHex}
      renderTooltip={(item) => <UoItemTooltip item={item} />}
      filters={
        <Select value={flag} onValueChange={setFlag}>
          <SelectTrigger aria-label="Filter UO items by flag" className="h-9 w-full bg-bg text-[13px] md:w-44">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All flags</SelectItem>
            {flagOptions.map((option) => (
              <SelectItem key={option} value={option}>
                {option}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      }
      title={title}
      searchPlaceholder="Search raw item id, name or flag…"
      emptyText="No UO items match this search."
    />
  );
}
