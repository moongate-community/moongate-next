import { useCallback } from "react";
import { listItemTemplates } from "../../../lib/adminItemTemplatesClient";
import { EntityPickerDialog } from "../common/EntityPickerDialog";
import { ItemTemplateTooltip } from "./ItemTemplateTooltip";

type ItemTemplatePickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (id: string) => void;
  includeAbstract?: boolean;
  title?: string;
};

/** Reusable modal that browses item templates and returns the chosen template id. */
export function ItemTemplatePickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  includeAbstract = false,
  title = "Select an item"
}: ItemTemplatePickerDialogProps) {
  const loadPage = useCallback(
    (page: number, search: string) =>
      listItemTemplates(accessToken, {
        page,
        pageSize: 60,
        search,
        tag: "",
        rarity: "",
        layer: "",
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
      renderTooltip={(template) => <ItemTemplateTooltip template={template} />}
      title={title}
      searchPlaceholder="Search items…"
      emptyText="No items found."
    />
  );
}
