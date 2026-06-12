import { useCallback } from "react";
import { listHues } from "../../../lib/adminHuesClient";
import type { HueSummary } from "../../../types/itemTemplates";
import { EntityPickerDialog } from "../common/EntityPickerDialog";
import { HueSwatch } from "../itemTemplates/HueSwatch";

type HuePickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (hue: number) => void;
  title?: string;
};

/** Reusable modal that browses UO hues as swatches and returns the chosen hue value. */
export function HuePickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  title = "Select a hue"
}: HuePickerDialogProps) {
  const loadPage = useCallback(
    (page: number, search: string) => listHues(accessToken, page, search),
    [accessToken]
  );

  return (
    <EntityPickerDialog<HueSummary>
      open={open}
      onOpenChange={onOpenChange}
      onSelect={(hue) => onSelect(hue.value)}
      loadPage={loadPage}
      getKey={(hue) => String(hue.value)}
      getLabel={(hue) => `${hue.hex} ${hue.name}`}
      renderTile={(hue) => <HueSwatch hue={hue} mode="compact" />}
      title={title}
      searchPlaceholder="Search hues… (value or name)"
      emptyText="No hues found."
    />
  );
}
