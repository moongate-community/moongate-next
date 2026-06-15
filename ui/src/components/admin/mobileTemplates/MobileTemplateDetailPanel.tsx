import { Pencil, User } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import type { MobileTemplateDetail } from "../../../types/mobileTemplates";
import { DefinitionList } from "../DefinitionList";
import { Paperdoll } from "../../common/Paperdoll";
import { BodyImageCell } from "./BodyImageCell";
import { NotorietyBadge } from "./NotorietyBadge";

type MobileTemplateDetailPanelProps = {
  template: MobileTemplateDetail | null;
  loading: boolean;
  error: string | null;
  onEdit?: () => void;
  onLootTemplateOpen?: (id: string) => void;
};

export function MobileTemplateDetailPanel({
  template,
  loading,
  error,
  onEdit,
  onLootTemplateOpen
}: MobileTemplateDetailPanelProps) {
  if (loading) {
    return (
      <Card className="rounded-md border-border bg-surface py-0 shadow-none">
        <CardContent className="grid gap-3 p-4">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-32 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="rounded-md border-danger/20 bg-danger/10 py-0 text-sm font-medium text-danger shadow-none">
        <CardContent className="p-4">{error}</CardContent>
      </Card>
    );
  }

  if (!template) {
    return (
      <Card className="rounded-md border-dashed border-border bg-bg py-0 text-sm text-fg-muted shadow-none">
        <CardContent className="p-4">
          <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted">
            <User size={17} aria-hidden />
          </div>
          <p className="m-0 font-medium text-fg">Select a mobile template</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="gap-0 rounded-md border-border bg-surface py-0 shadow-none">
      <CardHeader className="relative flex flex-row items-start gap-3 border-b border-border p-4">
        {onEdit && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={onEdit}
            className="absolute right-3 top-3 min-h-[30px] gap-1.5 px-2.5 text-[13px] font-medium text-fg-muted hover:bg-muted hover:text-fg"
          >
            <Pencil size={14} aria-hidden />
            Edit
          </Button>
        )}
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
              <Badge variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">Abstract</Badge>
            )}
          </div>
        </div>
      </CardHeader>

      <CardContent className="grid gap-4 p-4">
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
          {template.paperdollImageUrl && (
            <div className="mb-3 flex justify-center">
              <Paperdoll src={template.paperdollImageUrl} alt={template.name} className="h-48 w-auto object-contain" />
            </div>
          )}
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
                <Card key={skill.name} className="gap-1 rounded-md border-border bg-bg p-2 py-2 shadow-none">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{skill.name}</span>
                    <span className="font-mono text-[11px] font-medium text-fg-muted">{skill.value}</span>
                  </div>
                </Card>
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
                onLootTemplateOpen ? (
                  <button
                    key={loot}
                    type="button"
                    onClick={() => onLootTemplateOpen(loot)}
                    className="rounded-md bg-muted px-1.5 py-0.5 font-mono text-[11px] font-medium text-fg-muted transition-colors hover:bg-border/60 hover:text-fg"
                  >
                    {loot}
                  </button>
                ) : (
                  <Badge key={loot} variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                    {loot}
                  </Badge>
                )
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
                <Badge key={tag} variant="outline" className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                  {tag}
                </Badge>
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
                <Card key={param.key} className="gap-1 rounded-md border-border bg-bg p-2 py-2 shadow-none">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-medium text-fg">{param.key}</span>
                    <span className="text-[11px] font-medium text-fg-muted">{param.type}</span>
                  </div>
                  <p className="m-0 mt-1 break-all font-mono text-xs text-fg-muted">{param.value}</p>
                </Card>
              ))}
            </div>
          )}
        </section>
      </CardContent>
    </Card>
  );
}
