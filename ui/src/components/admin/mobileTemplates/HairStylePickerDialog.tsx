import { useCallback } from "react";
import { listHairStyles, type HairStyleSummary } from "../../../lib/adminHairStylesClient";
import { EntityPickerDialog } from "../common/EntityPickerDialog";

type HairStylePickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  facial: boolean;
  onSelect: (style: number) => void;
  title?: string;
};

/** Reusable modal that browses hair styles rendered over a reference body and returns the chosen style id. */
export function HairStylePickerDialog({
  open,
  onOpenChange,
  accessToken,
  facial,
  onSelect,
  title
}: HairStylePickerDialogProps) {
  const loadPage = useCallback(
    (page: number, search: string) => listHairStyles(accessToken, page, search, facial),
    [accessToken, facial]
  );

  return (
    <EntityPickerDialog<HairStyleSummary>
      open={open}
      onOpenChange={onOpenChange}
      onSelect={(hair) => onSelect(hair.style)}
      loadPage={loadPage}
      getKey={(hair) => String(hair.style)}
      getImageUrl={(hair) => hair.imageUrl}
      getLabel={(hair) => hair.name}
      title={title ?? (facial ? "Select facial hair" : "Select hair style")}
      searchPlaceholder="Search styles…"
      emptyText="No styles found."
    />
  );
}
