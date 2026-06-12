import { useEffect, useMemo, useState, type FormEvent, type ReactNode } from "react";
import { Plus, Save, Search, Trash2, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { createItemTemplate, updateItemTemplate } from "../../../lib/adminItemTemplatesClient";
import {
  applyUoItemToItemTemplateFormState,
  createEmptyItemTemplateFormState,
  createItemTemplateFormStateFromDetail,
  itemTemplateGenerateOptions,
  itemTemplateLayerOptions,
  itemTemplateParamTypeOptions,
  itemTemplateRarityOptions,
  itemTemplateRefillPolicyOptions,
  itemTemplateRefillScopeOptions,
  itemTemplateVisibilityOptions,
  toItemTemplateEditRequest,
  type ItemTemplateFormState
} from "../../../lib/itemTemplateFormModel";
import type { ItemTemplateDetail, ItemTemplateSaveResult } from "../../../types/itemTemplates";
import { ItemImageCell } from "./ItemImageCell";
import { UoItemPickerDialog } from "./UoItemPickerDialog";

type ItemTemplateFormProps = {
  accessToken: string;
  mode: "create" | "edit";
  template?: ItemTemplateDetail | null;
  onCancel: () => void;
  onSaved: (result: ItemTemplateSaveResult) => void;
};

export function ItemTemplateForm({ accessToken, mode, template, onCancel, onSaved }: ItemTemplateFormProps) {
  const [state, setState] = useState<ItemTemplateFormState>(() =>
    template ? createItemTemplateFormStateFromDetail(template) : createEmptyItemTemplateFormState()
  );
  const [pickerOpen, setPickerOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const itemImageUrl = useMemo(() => imageUrlForItemId(state.itemId), [state.itemId]);

  useEffect(() => {
    setState(template ? createItemTemplateFormStateFromDetail(template) : createEmptyItemTemplateFormState());
  }, [template?.id]);

  function update<K extends keyof ItemTemplateFormState>(key: K, value: ItemTemplateFormState[K]) {
    setState((current) => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setSaving(true);
    setError(null);

    try {
      const request = toItemTemplateEditRequest(state);
      const result = mode === "create"
        ? await createItemTemplate(accessToken, request)
        : await updateItemTemplate(accessToken, template?.id ?? request.id, request);

      onSaved(result);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Failed to save item template.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={submit} className="grid gap-3">
      <div className="flex min-h-[48px] flex-wrap items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-2">
        <div className="min-w-0">
          <h3 className="m-0 truncate text-sm font-semibold tracking-tight text-fg">
            {mode === "create" ? "New Item Template" : `Edit ${template?.id ?? state.id}`}
          </h3>
          <p className="m-0 truncate font-mono text-[11px] text-fg-subtle">{mode === "create" ? "_web.yaml" : template?.id}</p>
        </div>
        <div className="flex items-center gap-1.5">
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={onCancel}
            disabled={saving}
            className="min-h-[30px] gap-1.5 px-2.5 text-[13px] font-medium text-fg-muted hover:bg-muted hover:text-fg"
          >
            <X size={14} aria-hidden />
            Cancel
          </Button>
          <Button type="submit" size="sm" disabled={saving} className="min-h-[30px] gap-1.5 px-2.5 text-[13px] font-semibold">
            <Save size={14} aria-hidden />
            {saving ? "Saving" : "Save"}
          </Button>
        </div>
      </div>

      {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-medium text-danger">{error}</p>}

      <div className="grid gap-3 xl:grid-cols-[minmax(0,1fr)_340px]">
        <div className="grid gap-3">
          <FormSection title="Identity">
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Template ID" htmlFor="item-template-id">
                <Input
                  id="item-template-id"
                  value={state.id}
                  onChange={(event) => update("id", event.target.value)}
                  disabled={mode === "edit"}
                  className="h-9 bg-bg text-[13px]"
                />
              </Field>
              <Field label="Name" htmlFor="item-template-name">
                <Input
                  id="item-template-name"
                  value={state.name}
                  onChange={(event) => update("name", event.target.value)}
                  className="h-9 bg-bg text-[13px]"
                />
              </Field>
              <Field label="Script" htmlFor="item-template-script">
                <Input
                  id="item-template-script"
                  value={state.scriptId}
                  onChange={(event) => update("scriptId", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
            <Field label="Comment" htmlFor="item-template-comment">
              <Input
                id="item-template-comment"
                value={state.comment}
                onChange={(event) => update("comment", event.target.value)}
                className="h-9 bg-bg text-[13px]"
              />
            </Field>
            <Field label="Tags" htmlFor="item-template-tags">
              <Input
                id="item-template-tags"
                value={state.tagsText}
                onChange={(event) => update("tagsText", event.target.value)}
                placeholder="weapon, loot, container"
                className="h-9 bg-bg text-[13px]"
              />
            </Field>
          </FormSection>

          <FormSection title="Gameplay">
            <div className="grid gap-3 md:grid-cols-3">
              <SelectField label="Rarity" value={state.rarity} onChange={(value) => update("rarity", value)} options={itemTemplateRarityOptions} />
              <SelectField label="Layer" value={state.layer} onChange={(value) => update("layer", value)} options={["none", ...itemTemplateLayerOptions]} />
              <SelectField label="Visibility" value={state.visibility} onChange={(value) => update("visibility", value)} options={itemTemplateVisibilityOptions} />
              <Field label="Amount" htmlFor="item-template-amount">
                <Input
                  id="item-template-amount"
                  value={state.amount}
                  onChange={(event) => update("amount", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Weight" htmlFor="item-template-weight">
                <Input
                  id="item-template-weight"
                  value={state.weight}
                  onChange={(event) => update("weight", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Gump" htmlFor="item-template-gump">
                <Input
                  id="item-template-gump"
                  value={state.gumpId}
                  onChange={(event) => update("gumpId", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
            <div className="flex flex-wrap gap-4">
              <BooleanField checked={state.isAbstract} onChange={(checked) => update("isAbstract", checked)}>
                Abstract
              </BooleanField>
              <BooleanField checked={state.isMovable} onChange={(checked) => update("isMovable", checked)}>
                Movable
              </BooleanField>
              <BooleanField checked={state.isStackable} onChange={(checked) => update("isStackable", checked)}>
                Stackable
              </BooleanField>
            </div>
          </FormSection>

          <FormSection title="Economy">
            <BooleanField checked={state.hasValue} onChange={(checked) => update("hasValue", checked)}>
              Define value
            </BooleanField>
            {state.hasValue && (
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Buy" htmlFor="item-template-value-buy">
                  <Input
                    id="item-template-value-buy"
                    value={state.valueBuy}
                    onChange={(event) => update("valueBuy", event.target.value)}
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
                <Field label="Sell" htmlFor="item-template-value-sell">
                  <Input
                    id="item-template-value-sell"
                    value={state.valueSell}
                    onChange={(event) => update("valueSell", event.target.value)}
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
              </div>
            )}
          </FormSection>

          <FormSection title="Contents">
            <BooleanField checked={state.hasContents} onChange={(checked) => update("hasContents", checked)}>
              Generate contents from loot template
            </BooleanField>
            {state.hasContents && (
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Loot template" htmlFor="item-template-loot">
                  <Input
                    id="item-template-loot"
                    value={state.lootTemplate}
                    onChange={(event) => update("lootTemplate", event.target.value)}
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
                <Field label="Refill every" htmlFor="item-template-refill">
                  <Input
                    id="item-template-refill"
                    value={state.refillEvery}
                    onChange={(event) => update("refillEvery", event.target.value)}
                    placeholder="06:00:00"
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
                <SelectField label="Generate" value={state.generate} onChange={(value) => update("generate", value)} options={itemTemplateGenerateOptions} />
                <SelectField
                  label="Refill policy"
                  value={state.refillPolicy}
                  onChange={(value) => update("refillPolicy", value)}
                  options={itemTemplateRefillPolicyOptions}
                />
                <SelectField
                  label="Refill scope"
                  value={state.refillScope}
                  onChange={(value) => update("refillScope", value)}
                  options={itemTemplateRefillScopeOptions}
                />
              </div>
            )}
          </FormSection>

          <FormSection title="Params">
            <div className="grid gap-2">
              {state.params.length === 0 ? (
                <p className="m-0 text-[13px] text-fg-muted">No params.</p>
              ) : (
                state.params.map((param, index) => (
                  <div key={index} className="grid gap-2 rounded-md border border-border bg-bg p-2 md:grid-cols-[minmax(140px,1fr)_140px_minmax(180px,2fr)_32px]">
                    <Input
                      value={param.key}
                      onChange={(event) => updateParam(index, "key", event.target.value)}
                      placeholder="key"
                      className="h-8 bg-surface font-mono text-[13px]"
                    />
                    <Select value={param.type} onValueChange={(value) => updateParam(index, "type", value)}>
                      <SelectTrigger aria-label="Param type" className="h-8 w-full bg-surface text-[13px]">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {itemTemplateParamTypeOptions.map((option) => (
                          <SelectItem key={option} value={option}>
                            {option}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <Input
                      value={param.value}
                      onChange={(event) => updateParam(index, "value", event.target.value)}
                      placeholder="value"
                      className="h-8 bg-surface font-mono text-[13px]"
                    />
                    <IconButton label="Remove param" onClick={() => removeParam(index)}>
                      <Trash2 size={14} aria-hidden />
                    </IconButton>
                  </div>
                ))
              )}
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={addParam}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Plus size={14} aria-hidden />
                Add param
              </Button>
            </div>
          </FormSection>
        </div>

        <div className="grid h-fit gap-3">
          <FormSection title="Visual">
            <div className="grid gap-3">
              <div className="flex items-center gap-3">
                <ItemImageCell src={itemImageUrl} alt={state.name || state.id || state.itemId} size="large" />
                <div className="min-w-0">
                  <p className="m-0 truncate font-mono text-xs font-semibold text-fg">{state.itemId}</p>
                  <p className="m-0 text-[12px] text-fg-muted">Raw UO item</p>
                </div>
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setPickerOpen(true)}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Search size={14} aria-hidden />
                Browse UO items
              </Button>
              <div className="grid gap-3">
                <Field label="Item ID" htmlFor="item-template-item-id">
                  <Input
                    id="item-template-item-id"
                    value={state.itemId}
                    onChange={(event) => update("itemId", event.target.value)}
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
                <Field label="Hue" htmlFor="item-template-hue">
                  <Input
                    id="item-template-hue"
                    value={state.hue}
                    onChange={(event) => update("hue", event.target.value)}
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                </Field>
              </div>
            </div>
          </FormSection>

          <FormSection title="Graphic Variants">
            <div className="grid gap-2">
              {state.graphicVariants.length === 0 ? (
                <p className="m-0 text-[13px] text-fg-muted">No graphic variants.</p>
              ) : (
                state.graphicVariants.map((variant, index) => (
                  <div key={index} className="grid grid-cols-[40px_minmax(0,1fr)_32px] items-center gap-2">
                    <ItemImageCell src={imageUrlForItemId(variant.itemId)} alt={variant.itemId} />
                    <Input
                      value={variant.itemId}
                      onChange={(event) => updateVariant(index, event.target.value)}
                      className="h-8 bg-bg font-mono text-[13px]"
                    />
                    <IconButton label="Remove variant" onClick={() => removeVariant(index)}>
                      <Trash2 size={14} aria-hidden />
                    </IconButton>
                  </div>
                ))
              )}
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={addVariant}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Plus size={14} aria-hidden />
                Add variant
              </Button>
            </div>
          </FormSection>

          <div className="rounded-md border border-border bg-surface p-3">
            <div className="flex flex-wrap gap-1.5">
              <Badge variant="outline" className="rounded-md border-border bg-bg px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                {state.rarity}
              </Badge>
              {state.layer !== "none" && (
                <Badge variant="outline" className="rounded-md border-border bg-bg px-1.5 py-0.5 font-mono text-[11px] font-medium text-fg-muted">
                  {state.layer}
                </Badge>
              )}
              {state.isAbstract && (
                <Badge variant="outline" className="rounded-md border-border bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  Abstract
                </Badge>
              )}
            </div>
          </div>
        </div>
      </div>

      <UoItemPickerDialog
        open={pickerOpen}
        onOpenChange={setPickerOpen}
        accessToken={accessToken}
        onSelect={(item) => setState((current) => applyUoItemToItemTemplateFormState(current, item))}
      />
    </form>
  );

  function addParam() {
    setState((current) => ({
      ...current,
      params: [...current.params, { key: "", type: "String", value: "" }]
    }));
  }

  function updateParam(index: number, key: "key" | "type" | "value", value: string) {
    setState((current) => ({
      ...current,
      params: current.params.map((param, paramIndex) => (paramIndex === index ? { ...param, [key]: value } : param))
    }));
  }

  function removeParam(index: number) {
    setState((current) => ({
      ...current,
      params: current.params.filter((_, paramIndex) => paramIndex !== index)
    }));
  }

  function addVariant() {
    setState((current) => ({
      ...current,
      graphicVariants: [...current.graphicVariants, { itemId: current.itemId }]
    }));
  }

  function updateVariant(index: number, itemId: string) {
    setState((current) => ({
      ...current,
      graphicVariants: current.graphicVariants.map((variant, variantIndex) => (variantIndex === index ? { itemId } : variant))
    }));
  }

  function removeVariant(index: number) {
    setState((current) => ({
      ...current,
      graphicVariants: current.graphicVariants.filter((_, variantIndex) => variantIndex !== index)
    }));
  }
}

function FormSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="grid gap-3 rounded-md border border-border bg-surface p-4">
      <h4 className="m-0 text-[11px] font-semibold uppercase text-fg-subtle">{title}</h4>
      {children}
    </section>
  );
}

function Field({ label, htmlFor, children }: { label: string; htmlFor: string; children: ReactNode }) {
  return (
    <div className="grid gap-1.5">
      <Label htmlFor={htmlFor} className="text-[12px] font-semibold text-fg-muted">
        {label}
      </Label>
      {children}
    </div>
  );
}

function SelectField({
  label,
  value,
  onChange,
  options
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: string[];
}) {
  return (
    <div className="grid gap-1.5">
      <Label className="text-[12px] font-semibold text-fg-muted">{label}</Label>
      <Select value={value} onValueChange={onChange}>
        <SelectTrigger className="h-9 w-full bg-bg text-[13px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option} value={option}>
              {option === "none" ? "None" : option}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

function BooleanField({ checked, onChange, children }: { checked: boolean; onChange: (checked: boolean) => void; children: ReactNode }) {
  return (
    <Label className="flex items-center gap-2 text-[13px] font-semibold text-fg-muted">
      <Checkbox checked={checked} onCheckedChange={(next) => onChange(next === true)} />
      {children}
    </Label>
  );
}

function IconButton({ label, onClick, children }: { label: string; onClick: () => void; children: ReactNode }) {
  return (
    <Button
      type="button"
      variant="ghost"
      size="icon-sm"
      aria-label={label}
      onClick={onClick}
      className="h-8 w-8 text-fg-muted hover:bg-muted hover:text-fg"
    >
      {children}
    </Button>
  );
}

function imageUrlForItemId(value: string): string {
  const parsed = Number(value.trim());
  const itemId = Number.isFinite(parsed) ? Math.max(0, Math.trunc(parsed)) : 0;
  const itemIdHex = `0x${itemId.toString(16).toUpperCase().padStart(4, "0")}`;

  return `/api/items/${itemIdHex}.png`;
}
