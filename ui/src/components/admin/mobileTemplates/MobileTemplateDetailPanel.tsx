import { User } from "lucide-react";
import type { MobileTemplateDetail } from "../../../types/mobileTemplates";
import { DefinitionList } from "../DefinitionList";
import { BodyImageCell } from "./BodyImageCell";
import { NotorietyBadge } from "./NotorietyBadge";

type MobileTemplateDetailPanelProps = {
  template: MobileTemplateDetail | null;
  loading: boolean;
  error: string | null;
};

export function MobileTemplateDetailPanel({ template, loading, error }: MobileTemplateDetailPanelProps) {
  if (loading) {
    return (
      <aside className="rounded-md border border-border bg-surface p-4 text-sm font-medium text-fg-muted">
        Loading template...
      </aside>
    );
  }

  if (error) {
    return (
      <aside className="rounded-md border border-danger/20 bg-danger/10 p-4 text-sm font-medium text-danger">
        {error}
      </aside>
    );
  }

  if (!template) {
    return (
      <aside className="rounded-md border border-dashed border-border bg-bg p-4 text-sm text-fg-muted">
        <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted">
          <User size={17} aria-hidden />
        </div>
        <p className="m-0 font-medium text-fg">Select a mobile template</p>
        <p className="m-0 mt-1 text-[13px] leading-relaxed">Choose a row to inspect its full read-only definition.</p>
      </aside>
    );
  }

  return (
    <aside className="rounded-md border border-border bg-surface">
      {/* Header */}
      <div className="flex items-start gap-3 border-b border-border p-4">
        <BodyImageCell key={template.id} imageUrl={template.imageUrl} body={template.body} bodyHex={template.bodyHex} />
        <div className="min-w-0">
          <h3 className="m-0 text-base font-semibold text-fg">{template.name || template.id}</h3>
          {template.title && (
            <p className="m-0 mt-0.5 text-[13px] leading-snug text-fg-muted">{template.title}</p>
          )}
          <p className="m-0 mt-1 font-mono text-xs text-fg-muted">
            {template.id} · {template.bodyHex}
          </p>
          <div className="mt-2 flex items-center gap-2">
            <NotorietyBadge notoriety={template.notoriety} mode="detail" />
            {template.isAbstract && (
              <span className="rounded-md bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">Abstract</span>
            )}
          </div>
        </div>
      </div>

      <div className="grid gap-4 p-4">
        {/* Identity */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Identity</h4>
          <DefinitionList
            items={[
              { term: "ID", value: template.id, mono: true },
              { term: "Base mobile", value: template.baseMobile ?? "—", mono: true },
              { term: "Body", value: `${template.body} (${template.bodyHex})`, mono: true },
              { term: "Gender", value: template.gender },
              { term: "Race index", value: String(template.raceIndex), mono: true },
              { term: "Brain", value: template.brain || "—", mono: true },
              { term: "Faction", value: template.factionId || "—", mono: true },
              { term: "Karma", value: String(template.karma), mono: true },
              { term: "Fame", value: String(template.fame), mono: true }
            ]}
          />
        </section>

        {/* Appearance */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Appearance</h4>
          <DefinitionList
            items={[
              { term: "Skin hue", value: String(template.skinHue), mono: true },
              { term: "Hair hue", value: String(template.hairHue), mono: true },
              { term: "Hair style", value: String(template.hairStyle), mono: true },
              { term: "Facial hair hue", value: String(template.facialHairHue), mono: true },
              { term: "Facial hair style", value: String(template.facialHairStyle), mono: true }
            ]}
          />
        </section>

        {/* Stats */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Stats</h4>
          {template.stats === null ? (
            <span className="text-xs text-fg-muted">No stats</span>
          ) : (
            <DefinitionList
              items={[
                { term: "Strength", value: String(template.stats.strength), mono: true },
                { term: "Dexterity", value: String(template.stats.dexterity), mono: true },
                { term: "Intelligence", value: String(template.stats.intelligence), mono: true }
              ]}
            />
          )}
        </section>

        {/* Resources */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Resources</h4>
          {template.resources === null ? (
            <span className="text-xs text-fg-muted">No resources</span>
          ) : (
            <DefinitionList
              items={[
                { term: "Hits", value: String(template.resources.hits), mono: true },
                { term: "Mana", value: String(template.resources.mana), mono: true },
                { term: "Stamina", value: String(template.resources.stamina), mono: true }
              ]}
            />
          )}
        </section>

        {/* Resistances */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Resistances</h4>
          {template.resistances === null ? (
            <span className="text-xs text-fg-muted">No resistances</span>
          ) : (
            <DefinitionList
              items={[
                { term: "Physical", value: String(template.resistances.physical), mono: true },
                { term: "Fire", value: String(template.resistances.fire), mono: true },
                { term: "Cold", value: String(template.resistances.cold), mono: true },
                { term: "Poison", value: String(template.resistances.poison), mono: true },
                { term: "Energy", value: String(template.resistances.energy), mono: true }
              ]}
            />
          )}
        </section>

        {/* Skills */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Skills</h4>
          {template.skills.length === 0 ? (
            <span className="text-xs text-fg-muted">No skills</span>
          ) : (
            <div className="grid gap-2">
              {template.skills.map((skill) => (
                <div key={skill.name} className="rounded-md border border-border bg-bg p-2">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{skill.name}</span>
                    <span className="font-mono text-[11px] font-medium text-fg-muted">{skill.value}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>

        {/* Equipment */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Equipment</h4>
          {template.equipment.length === 0 ? (
            <span className="text-xs text-fg-muted">No equipment</span>
          ) : (
            <div className="grid gap-1">
              {template.equipment.map((itemId) => (
                <span key={itemId} className="font-mono text-xs text-fg-muted">
                  {itemId}
                </span>
              ))}
            </div>
          )}
          <div className="mt-2">
            <DefinitionList
              items={[{ term: "Backpack", value: template.backpackTemplate ?? "—", mono: true }]}
            />
          </div>
        </section>

        {/* Loot tables */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Loot Tables</h4>
          {template.lootTables.length === 0 ? (
            <span className="text-xs text-fg-muted">No loot tables</span>
          ) : (
            <div className="flex flex-wrap gap-1.5">
              {template.lootTables.map((loot) => (
                <span key={loot} className="rounded-md bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  {loot}
                </span>
              ))}
            </div>
          )}
        </section>

        {/* Tags */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Tags</h4>
          <div className="flex flex-wrap gap-1.5">
            {template.tags.length === 0 ? (
              <span className="text-xs text-fg-muted">No tags</span>
            ) : (
              template.tags.map((tag) => (
                <span key={tag} className="rounded-md bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  {tag}
                </span>
              ))
            )}
          </div>
        </section>

        {/* Params */}
        <section>
          <h4 className="mb-2 text-[11px] font-medium text-fg-subtle">Params</h4>
          {template.params.length === 0 ? (
            <span className="text-xs text-fg-muted">No params</span>
          ) : (
            <div className="grid gap-2">
              {template.params.map((param) => (
                <div key={param.key} className="rounded-md border border-border bg-bg p-2">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{param.key}</span>
                    <span className="text-[11px] font-medium text-fg-muted">{param.type}</span>
                  </div>
                  <p className="m-0 mt-1 break-all font-mono text-xs text-fg-muted">{param.value}</p>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </aside>
  );
}
