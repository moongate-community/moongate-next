import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import { Plus, Save, Search, Trash2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { createMobileTemplate, updateMobileTemplate } from "../../../lib/adminMobileTemplatesClient";
import {
  emptyMobileTemplateFormState,
  mobileTemplateEditRequestFromState,
  mobileTemplateFormStateFromDetail,
  mobileTemplateGenderOptions,
  mobileTemplateNotorietyOptions,
  mobileTemplateParamTypeOptions,
  type MobileTemplateFormState,
  type MobileTemplateNotoriety,
  type MobileTemplateSaveResult
} from "../../../lib/mobileTemplateFormModel";
import type { MobileTemplateDetail } from "../../../types/mobileTemplates";
import { notorietyColor } from "../../../lib/notorietyColors";
import { ItemTemplatePickerDialog } from "../itemTemplates/ItemTemplatePickerDialog";
import { MobileTemplatePickerDialog } from "./MobileTemplatePickerDialog";
import { BodyPickerDialog } from "./BodyPickerDialog";
import { HuePickerDialog } from "./HuePickerDialog";

type MobileTemplateFormProps = {
  accessToken: string;
  mode: "create" | "edit";
  template: MobileTemplateDetail | null;
  onCancel: () => void;
  onSaved: (result: MobileTemplateSaveResult) => void;
};

export function MobileTemplateForm({ accessToken, mode, template, onCancel, onSaved }: MobileTemplateFormProps) {
  const [state, setState] = useState<MobileTemplateFormState>(() =>
    template ? mobileTemplateFormStateFromDetail(template) : emptyMobileTemplateFormState()
  );
  const [baseMobilePickerOpen, setBaseMobilePickerOpen] = useState(false);
  const [equipmentPickerOpen, setEquipmentPickerOpen] = useState(false);
  const [bodyPickerOpen, setBodyPickerOpen] = useState(false);
  const [huePickerField, setHuePickerField] = useState<"skinHue" | "hairHue" | "facialHairHue" | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setState(template ? mobileTemplateFormStateFromDetail(template) : emptyMobileTemplateFormState());
  }, [template?.id]);

  function update<K extends keyof MobileTemplateFormState>(key: K, value: MobileTemplateFormState[K]) {
    setState((current) => ({ ...current, [key]: value }));
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setSaving(true);
    setError(null);

    try {
      const request = mobileTemplateEditRequestFromState(state);
      const result =
        mode === "create"
          ? await createMobileTemplate(accessToken, request)
          : await updateMobileTemplate(accessToken, template?.id ?? request.id, request);

      onSaved(result);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Failed to save mobile template.");
    } finally {
      setSaving(false);
    }
  }

  const bodyNum = parseInt(state.body, 10);
  const showBodyPreview = Number.isFinite(bodyNum) && bodyNum > 0;

  return (
    <form onSubmit={submit} className="grid gap-3">
      <div className="flex min-h-[48px] flex-wrap items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-2">
        <div className="min-w-0">
          <h3 className="m-0 truncate text-sm font-semibold tracking-tight text-fg">
            {mode === "create" ? "New Mobile Template" : `Edit ${template?.id ?? state.id}`}
          </h3>
          <p className="m-0 truncate font-mono text-[11px] text-fg-subtle">
            {mode === "create" ? "_web.yaml" : template?.id}
          </p>
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
              <Field label="Template ID" htmlFor="mobile-template-id">
                <Input
                  id="mobile-template-id"
                  value={state.id}
                  onChange={(event) => update("id", event.target.value)}
                  disabled={mode === "edit"}
                  className="h-9 bg-bg text-[13px]"
                />
              </Field>
              <Field label="Name" htmlFor="mobile-template-name">
                <Input
                  id="mobile-template-name"
                  value={state.name}
                  onChange={(event) => update("name", event.target.value)}
                  className="h-9 bg-bg text-[13px]"
                />
              </Field>
              <Field label="Title" htmlFor="mobile-template-title">
                <Input
                  id="mobile-template-title"
                  value={state.title}
                  onChange={(event) => update("title", event.target.value)}
                  className="h-9 bg-bg text-[13px]"
                />
              </Field>
              <Field label="Base mobile" htmlFor="mobile-template-base-mobile">
                <div className="flex gap-1.5">
                  <Input
                    id="mobile-template-base-mobile"
                    value={state.baseMobile}
                    onChange={(event) => update("baseMobile", event.target.value)}
                    placeholder="e.g. orc"
                    className="h-9 bg-bg font-mono text-[13px]"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setBaseMobilePickerOpen(true)}
                    className="h-9 gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                  >
                    <Search size={14} aria-hidden />
                    Browse
                  </Button>
                </div>
              </Field>
            </div>
            <div className="flex flex-wrap gap-4">
              <BooleanField checked={state.isAbstract} onChange={(checked) => update("isAbstract", checked)}>
                Abstract
              </BooleanField>
            </div>
          </FormSection>

          <FormSection title="Standing">
            <div className="grid gap-3 md:grid-cols-3">
              <div className="grid gap-1.5">
                <Label className="text-[12px] font-semibold text-fg-muted">Notoriety</Label>
                <Select
                  value={state.notoriety}
                  onValueChange={(value) => update("notoriety", value as MobileTemplateNotoriety)}
                >
                  <SelectTrigger className="h-9 w-full bg-bg text-[13px]">
                    <SelectValue>
                      <span style={{ color: notorietyColor(state.notoriety) }}>{state.notoriety}</span>
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    {mobileTemplateNotorietyOptions.map((option) => (
                      <SelectItem key={option} value={option}>
                        <span style={{ color: notorietyColor(option) }}>{option}</span>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <Field label="Karma" htmlFor="mobile-template-karma">
                <Input
                  id="mobile-template-karma"
                  value={state.karma}
                  onChange={(event) => update("karma", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Fame" htmlFor="mobile-template-fame">
                <Input
                  id="mobile-template-fame"
                  value={state.fame}
                  onChange={(event) => update("fame", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Brain" htmlFor="mobile-template-brain">
                <Input
                  id="mobile-template-brain"
                  value={state.brain}
                  onChange={(event) => update("brain", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Faction ID" htmlFor="mobile-template-faction-id">
                <Input
                  id="mobile-template-faction-id"
                  value={state.factionId}
                  onChange={(event) => update("factionId", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
          </FormSection>

          <FormSection title="Attributes">
            <div className="grid gap-3 md:grid-cols-3">
              <Field label="Strength" htmlFor="mobile-template-strength">
                <Input
                  id="mobile-template-strength"
                  value={state.stats.strength}
                  onChange={(event) => update("stats", { ...state.stats, strength: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Dexterity" htmlFor="mobile-template-dexterity">
                <Input
                  id="mobile-template-dexterity"
                  value={state.stats.dexterity}
                  onChange={(event) => update("stats", { ...state.stats, dexterity: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Intelligence" htmlFor="mobile-template-intelligence">
                <Input
                  id="mobile-template-intelligence"
                  value={state.stats.intelligence}
                  onChange={(event) => update("stats", { ...state.stats, intelligence: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
            <h5 className="m-0 text-[11px] font-semibold uppercase text-fg-subtle">Resources</h5>
            <div className="grid gap-3 md:grid-cols-3">
              <Field label="Hits" htmlFor="mobile-template-hits">
                <Input
                  id="mobile-template-hits"
                  value={state.resources.hits}
                  onChange={(event) => update("resources", { ...state.resources, hits: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Mana" htmlFor="mobile-template-mana">
                <Input
                  id="mobile-template-mana"
                  value={state.resources.mana}
                  onChange={(event) => update("resources", { ...state.resources, mana: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Stamina" htmlFor="mobile-template-stamina">
                <Input
                  id="mobile-template-stamina"
                  value={state.resources.stamina}
                  onChange={(event) => update("resources", { ...state.resources, stamina: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
            <h5 className="m-0 text-[11px] font-semibold uppercase text-fg-subtle">Resistances</h5>
            <div className="grid gap-3 md:grid-cols-5">
              <Field label="Physical" htmlFor="mobile-template-physical">
                <Input
                  id="mobile-template-physical"
                  value={state.resistances.physical}
                  onChange={(event) => update("resistances", { ...state.resistances, physical: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Fire" htmlFor="mobile-template-fire">
                <Input
                  id="mobile-template-fire"
                  value={state.resistances.fire}
                  onChange={(event) => update("resistances", { ...state.resistances, fire: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Cold" htmlFor="mobile-template-cold">
                <Input
                  id="mobile-template-cold"
                  value={state.resistances.cold}
                  onChange={(event) => update("resistances", { ...state.resistances, cold: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Poison" htmlFor="mobile-template-poison">
                <Input
                  id="mobile-template-poison"
                  value={state.resistances.poison}
                  onChange={(event) => update("resistances", { ...state.resistances, poison: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Energy" htmlFor="mobile-template-energy">
                <Input
                  id="mobile-template-energy"
                  value={state.resistances.energy}
                  onChange={(event) => update("resistances", { ...state.resistances, energy: event.target.value })}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
          </FormSection>

          <FormSection title="Skills">
            <div className="grid gap-2">
              {state.skills.length === 0 ? (
                <p className="m-0 text-[13px] text-fg-muted">No skills.</p>
              ) : (
                state.skills.map((skill, index) => (
                  <div key={index} className="grid gap-2 rounded-md border border-border bg-bg p-2 md:grid-cols-[minmax(180px,2fr)_120px_32px]">
                    <Input
                      value={skill.name}
                      onChange={(event) => updateSkill(index, "name", event.target.value)}
                      placeholder="skill name"
                      className="h-8 bg-surface font-mono text-[13px]"
                    />
                    <Input
                      value={String(skill.value)}
                      onChange={(event) => updateSkill(index, "value", event.target.value)}
                      placeholder="value"
                      className="h-8 bg-surface font-mono text-[13px]"
                    />
                    <IconButton label="Remove skill" onClick={() => removeSkill(index)}>
                      <Trash2 size={14} aria-hidden />
                    </IconButton>
                  </div>
                ))
              )}
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={addSkill}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Plus size={14} aria-hidden />
                Add skill
              </Button>
            </div>
          </FormSection>

          <FormSection title="Equipment">
            <div className="grid gap-2">
              {state.equipment.length === 0 ? (
                <p className="m-0 text-[13px] text-fg-muted">No equipment.</p>
              ) : (
                state.equipment.map((itemId, index) => (
                  <div key={index} className="grid grid-cols-[minmax(0,1fr)_32px] items-center gap-2">
                    <Input
                      value={itemId}
                      onChange={(event) => updateEquipmentItem(index, event.target.value)}
                      placeholder="item template id"
                      className="h-8 bg-bg font-mono text-[13px]"
                    />
                    <IconButton label="Remove equipment item" onClick={() => removeEquipmentItem(index)}>
                      <Trash2 size={14} aria-hidden />
                    </IconButton>
                  </div>
                ))
              )}
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setEquipmentPickerOpen(true)}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Plus size={14} aria-hidden />
                Add item
              </Button>
            </div>
          </FormSection>

          <FormSection title="Loot">
            <Field label="Backpack template" htmlFor="mobile-template-backpack">
              <Input
                id="mobile-template-backpack"
                value={state.backpackTemplate}
                onChange={(event) => update("backpackTemplate", event.target.value)}
                className="h-9 bg-bg font-mono text-[13px]"
              />
            </Field>
            <div className="grid gap-1.5">
              <Label className="text-[12px] font-semibold text-fg-muted">Loot tables</Label>
              <div className="grid gap-2">
                {state.lootTables.length === 0 ? (
                  <p className="m-0 text-[13px] text-fg-muted">No loot tables.</p>
                ) : (
                  state.lootTables.map((lootId, index) => (
                    <div key={index} className="grid grid-cols-[minmax(0,1fr)_32px] items-center gap-2">
                      <Input
                        value={lootId}
                        onChange={(event) => updateLootTable(index, event.target.value)}
                        placeholder="loot table id"
                        className="h-8 bg-bg font-mono text-[13px]"
                      />
                      <IconButton label="Remove loot table" onClick={() => removeLootTable(index)}>
                        <Trash2 size={14} aria-hidden />
                      </IconButton>
                    </div>
                  ))
                )}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addLootTable}
                  className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                >
                  <Plus size={14} aria-hidden />
                  Add loot table
                </Button>
              </div>
            </div>
          </FormSection>

          <FormSection title="Tags">
            <div className="grid gap-2">
              {state.tags.length === 0 ? (
                <p className="m-0 text-[13px] text-fg-muted">No tags.</p>
              ) : (
                state.tags.map((tag, index) => (
                  <div key={index} className="grid grid-cols-[minmax(0,1fr)_32px] items-center gap-2">
                    <Input
                      value={tag}
                      onChange={(event) => updateTag(index, event.target.value)}
                      placeholder="tag"
                      className="h-8 bg-bg text-[13px]"
                    />
                    <IconButton label="Remove tag" onClick={() => removeTag(index)}>
                      <Trash2 size={14} aria-hidden />
                    </IconButton>
                  </div>
                ))
              )}
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={addTag}
                className="w-fit gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
              >
                <Plus size={14} aria-hidden />
                Add tag
              </Button>
            </div>
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
                        {mobileTemplateParamTypeOptions.map((option) => (
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
          <FormSection title="Appearance">
            <div className="grid gap-3">
              <Field label="Body" htmlFor="mobile-template-body">
                <div className="flex items-center gap-2">
                  <Input
                    id="mobile-template-body"
                    value={state.body}
                    onChange={(event) => update("body", event.target.value)}
                    className="h-9 flex-1 bg-bg font-mono text-[13px]"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setBodyPickerOpen(true)}
                    className="h-9 gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                  >
                    <Search size={14} aria-hidden />
                    Browse
                  </Button>
                </div>
              </Field>
              {showBodyPreview && (
                <img
                  src={`/api/mobiles/${state.body}.png`}
                  alt={state.name || state.id}
                  className="h-16 w-16 object-contain"
                  style={{ imageRendering: "pixelated" }}
                  onError={(event) => {
                    (event.target as HTMLImageElement).style.display = "none";
                  }}
                />
              )}
              <SelectField
                label="Gender"
                value={state.gender}
                onChange={(value) => update("gender", value as "Male" | "Female")}
                options={mobileTemplateGenderOptions}
              />
              <Field label="Race index" htmlFor="mobile-template-race-index">
                <Input
                  id="mobile-template-race-index"
                  value={state.raceIndex}
                  onChange={(event) => update("raceIndex", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Skin hue" htmlFor="mobile-template-skin-hue">
                <div className="flex items-center gap-2">
                  <Input
                    id="mobile-template-skin-hue"
                    value={state.skinHue}
                    onChange={(event) => update("skinHue", event.target.value)}
                    className="h-9 flex-1 bg-bg font-mono text-[13px]"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setHuePickerField("skinHue")}
                    className="h-9 gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                  >
                    <Search size={14} aria-hidden />
                    Browse
                  </Button>
                </div>
              </Field>
              <Field label="Hair hue" htmlFor="mobile-template-hair-hue">
                <div className="flex items-center gap-2">
                  <Input
                    id="mobile-template-hair-hue"
                    value={state.hairHue}
                    onChange={(event) => update("hairHue", event.target.value)}
                    className="h-9 flex-1 bg-bg font-mono text-[13px]"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setHuePickerField("hairHue")}
                    className="h-9 gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                  >
                    <Search size={14} aria-hidden />
                    Browse
                  </Button>
                </div>
              </Field>
              <Field label="Hair style" htmlFor="mobile-template-hair-style">
                <Input
                  id="mobile-template-hair-style"
                  value={state.hairStyle}
                  onChange={(event) => update("hairStyle", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
              <Field label="Facial hair hue" htmlFor="mobile-template-facial-hair-hue">
                <div className="flex items-center gap-2">
                  <Input
                    id="mobile-template-facial-hair-hue"
                    value={state.facialHairHue}
                    onChange={(event) => update("facialHairHue", event.target.value)}
                    className="h-9 flex-1 bg-bg font-mono text-[13px]"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setHuePickerField("facialHairHue")}
                    className="h-9 gap-1.5 border-border bg-bg px-2.5 text-[13px] font-semibold text-fg hover:bg-muted"
                  >
                    <Search size={14} aria-hidden />
                    Browse
                  </Button>
                </div>
              </Field>
              <Field label="Facial hair style" htmlFor="mobile-template-facial-hair-style">
                <Input
                  id="mobile-template-facial-hair-style"
                  value={state.facialHairStyle}
                  onChange={(event) => update("facialHairStyle", event.target.value)}
                  className="h-9 bg-bg font-mono text-[13px]"
                />
              </Field>
            </div>
          </FormSection>
        </div>
      </div>

      <MobileTemplatePickerDialog
        open={baseMobilePickerOpen}
        onOpenChange={setBaseMobilePickerOpen}
        accessToken={accessToken}
        includeAbstract
        title="Select base mobile"
        onSelect={(id) => {
          update("baseMobile", id);
          setBaseMobilePickerOpen(false);
        }}
      />

      <ItemTemplatePickerDialog
        open={equipmentPickerOpen}
        onOpenChange={setEquipmentPickerOpen}
        accessToken={accessToken}
        onSelect={(id) => {
          setState((current) => ({ ...current, equipment: [...current.equipment, id] }));
          setEquipmentPickerOpen(false);
        }}
      />

      <BodyPickerDialog
        open={bodyPickerOpen}
        onOpenChange={setBodyPickerOpen}
        accessToken={accessToken}
        onSelect={(body) => {
          update("body", String(body));
        }}
      />

      <HuePickerDialog
        open={huePickerField !== null}
        onOpenChange={(open) => {
          if (!open) {
            setHuePickerField(null);
          }
        }}
        accessToken={accessToken}
        onSelect={(hue) => {
          if (huePickerField) {
            update(huePickerField, String(hue));
          }
          setHuePickerField(null);
        }}
      />
    </form>
  );

  function addSkill() {
    setState((current) => ({
      ...current,
      skills: [...current.skills, { name: "", value: 0 }]
    }));
  }

  function updateSkill(index: number, key: "name" | "value", value: string) {
    setState((current) => ({
      ...current,
      skills: current.skills.map((skill, skillIndex) => {
        if (skillIndex !== index) {
          return skill;
        }

        return key === "name" ? { ...skill, name: value } : { ...skill, value: Number(value) || 0 };
      })
    }));
  }

  function removeSkill(index: number) {
    setState((current) => ({
      ...current,
      skills: current.skills.filter((_, skillIndex) => skillIndex !== index)
    }));
  }

  function updateEquipmentItem(index: number, value: string) {
    setState((current) => ({
      ...current,
      equipment: current.equipment.map((itemId, equipIndex) => (equipIndex === index ? value : itemId))
    }));
  }

  function removeEquipmentItem(index: number) {
    setState((current) => ({
      ...current,
      equipment: current.equipment.filter((_, equipIndex) => equipIndex !== index)
    }));
  }

  function addLootTable() {
    setState((current) => ({
      ...current,
      lootTables: [...current.lootTables, ""]
    }));
  }

  function updateLootTable(index: number, value: string) {
    setState((current) => ({
      ...current,
      lootTables: current.lootTables.map((lootId, lootIndex) => (lootIndex === index ? value : lootId))
    }));
  }

  function removeLootTable(index: number) {
    setState((current) => ({
      ...current,
      lootTables: current.lootTables.filter((_, lootIndex) => lootIndex !== index)
    }));
  }

  function addTag() {
    setState((current) => ({
      ...current,
      tags: [...current.tags, ""]
    }));
  }

  function updateTag(index: number, value: string) {
    setState((current) => ({
      ...current,
      tags: current.tags.map((tag, tagIndex) => (tagIndex === index ? value : tag))
    }));
  }

  function removeTag(index: number) {
    setState((current) => ({
      ...current,
      tags: current.tags.filter((_, tagIndex) => tagIndex !== index)
    }));
  }

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
              {option}
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
