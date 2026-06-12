import { useCallback } from "react";
import { listBodies, type BodySummary } from "../../../lib/adminBodiesClient";
import { EntityPickerDialog } from "../common/EntityPickerDialog";

type BodyPickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (body: number) => void;
  title?: string;
};

/** Reusable modal that browses classified UO bodies and returns the chosen body id. */
export function BodyPickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  title = "Select a body"
}: BodyPickerDialogProps) {
  const loadPage = useCallback(
    (page: number, search: string) => listBodies(accessToken, page, search),
    [accessToken]
  );

  return (
    <EntityPickerDialog<BodySummary>
      open={open}
      onOpenChange={onOpenChange}
      onSelect={(body) => onSelect(body.body)}
      loadPage={loadPage}
      getKey={(body) => String(body.body)}
      getImageUrl={(body) => body.imageUrl}
      getLabel={(body) => `${body.body} (${body.bodyType})`}
      title={title}
      searchPlaceholder="Search bodies… (id or 0x hex)"
      emptyText="No bodies found."
    />
  );
}
