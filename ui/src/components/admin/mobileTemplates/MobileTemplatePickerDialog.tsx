import { useCallback } from "react";
import { listMobileTemplates } from "../../../lib/adminMobileTemplatesClient";
import { EntityPickerDialog } from "../common/EntityPickerDialog";
import { MobileTemplateTooltip } from "./MobileTemplateTooltip";

type MobileTemplatePickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (id: string) => void;
  includeAbstract?: boolean;
  title?: string;
};

/** Reusable modal that browses mobile templates and returns the chosen template id. */
export function MobileTemplatePickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  includeAbstract = false,
  title = "Select a mobile"
}: MobileTemplatePickerDialogProps) {
  const loadPage = useCallback(
    (page: number, search: string) =>
      listMobileTemplates(accessToken, {
        page,
        pageSize: 60,
        search,
        tag: "",
        notoriety: "",
        abstract: includeAbstract ? "all" : "false"
      }),
    [accessToken, includeAbstract]
  );

  return (
    <EntityPickerDialog
      open={open}
      onOpenChange={onOpenChange}
      onSelect={(template) => onSelect(template.id)}
      loadPage={loadPage}
      getKey={(template) => template.id}
      getImageUrl={(template) => template.imageUrl}
      getLabel={(template) => template.name}
      renderTooltip={(template) => <MobileTemplateTooltip template={template} />}
      title={title}
      searchPlaceholder="Search mobiles…"
      emptyText="No mobiles found."
    />
  );
}
