import { useCallback, useEffect, useMemo, useState } from "react";
import { CheckCircle2, Plug, RefreshCw, Save, TestTube2, XCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import {
  getPluginConfig,
  getPluginConfigForm,
  listPlugins,
  savePluginConfig,
  testPluginConfig
} from "../../../lib/adminPluginsClient";
import { cn } from "../../../lib/utils";
import type {
  PluginCatalogEntry,
  PluginConfigField,
  PluginConfigForm,
  PluginConfigValue,
  PluginConfigView,
  PluginTestResult
} from "../../../types/plugins";
import { Panel } from "../Panel";

type PluginManagementPanelProps = {
  accessToken: string;
};

type Notice =
  | { kind: "success"; message: string; details?: string[] }
  | { kind: "error"; message: string; details?: string[] }
  | null;

export function PluginManagementPanel({ accessToken }: PluginManagementPanelProps) {
  const [plugins, setPlugins] = useState<PluginCatalogEntry[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [config, setConfig] = useState<PluginConfigView | null>(null);
  const [form, setForm] = useState<PluginConfigForm | null>(null);
  const [values, setValues] = useState<Record<string, PluginConfigValue>>({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [notice, setNotice] = useState<Notice>(null);

  const selected = useMemo(
    () => plugins.find((plugin) => plugin.id === selectedId) ?? null,
    [plugins, selectedId]
  );

  const loadPlugins = useCallback(async () => {
    setLoading(true);
    setNotice(null);

    try {
      const next = await listPlugins(accessToken);

      setPlugins(next);
      setSelectedId((current) => current ?? next[0]?.id ?? null);
    } catch (caught) {
      setNotice({ kind: "error", message: caught instanceof Error ? caught.message : "Failed to load plugins" });
      setPlugins([]);
      setSelectedId(null);
    } finally {
      setLoading(false);
    }
  }, [accessToken]);

  const loadSelected = useCallback(async () => {
    if (!selectedId) {
      setConfig(null);
      setForm(null);
      setValues({});

      return;
    }

    setLoading(true);
    setNotice(null);

    try {
      const [configResult, formResult] = await Promise.all([
        getPluginConfig(accessToken, selectedId),
        getPluginConfigForm(accessToken, selectedId)
      ]);

      setConfig(configResult);
      setForm(formResult);
      setValues(formResult ? valuesFromForm(formResult) : {});
    } catch (caught) {
      setNotice({ kind: "error", message: caught instanceof Error ? caught.message : "Failed to load plugin config" });
      setConfig(null);
      setForm(null);
      setValues({});
    } finally {
      setLoading(false);
    }
  }, [accessToken, selectedId]);

  useEffect(() => {
    void loadPlugins();
  }, [loadPlugins]);

  useEffect(() => {
    void loadSelected();
  }, [loadSelected]);

  async function handleSave() {
    if (!selectedId) {
      return;
    }

    setSaving(true);
    setNotice(null);

    try {
      const result = await savePluginConfig(accessToken, selectedId, values);

      if (!result.success) {
        setNotice({ kind: "error", message: "Config validation failed.", details: result.errors });

        return;
      }

      setConfig(result.config);
      setNotice({
        kind: "success",
        message: result.requiresRestart ? "Config saved. Restart required." : "Config saved."
      });
    } catch (caught) {
      setNotice({ kind: "error", message: caught instanceof Error ? caught.message : "Failed to save plugin config" });
    } finally {
      setSaving(false);
    }
  }

  async function handleTest() {
    if (!selectedId) {
      return;
    }

    setTesting(true);
    setNotice(null);

    try {
      const result = await testPluginConfig(accessToken, selectedId);

      setNotice(testNotice(result));
    } catch (caught) {
      setNotice({ kind: "error", message: caught instanceof Error ? caught.message : "Failed to test plugin config" });
    } finally {
      setTesting(false);
    }
  }

  function updateValue(path: string, value: PluginConfigValue) {
    setValues((current) => ({
      ...current,
      [path]: value
    }));
  }

  return (
    <Panel
      title="Plugins"
      icon={Plug}
      action={
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => { void loadPlugins(); }}
          disabled={loading}
          className="min-h-[32px] gap-1.5 border-border bg-surface font-semibold text-fg hover:bg-muted"
        >
          <RefreshCw size={14} aria-hidden />
          Refresh
        </Button>
      }
    >
      <div className="grid gap-4 xl:grid-cols-[280px_minmax(0,1fr)]">
        <div className="grid content-start gap-2">
          {loading && plugins.length === 0 ? (
            <div className="grid gap-2 rounded-md bg-muted p-3">
              <Skeleton className="h-4 w-36" />
              <Skeleton className="h-14 w-full" />
              <Skeleton className="h-14 w-full" />
            </div>
          ) : (
            plugins.map((plugin) => (
              <button
                key={plugin.id}
                type="button"
                onClick={() => setSelectedId(plugin.id)}
                className={cn(
                  "grid min-h-[64px] gap-1 rounded-md border px-3 py-2 text-left transition-colors",
                  selectedId === plugin.id
                    ? "border-primary/35 bg-primary/10"
                    : "border-border bg-bg hover:bg-muted"
                )}
              >
                <span className="truncate text-[13px] font-semibold text-fg">{plugin.name}</span>
                <span className="truncate font-mono text-[11px] text-fg-subtle">{plugin.id}</span>
                <span className="flex flex-wrap gap-1">
                  {plugin.isConfigurable && <Badge variant="outline" className="rounded-md">Config</Badge>}
                  {plugin.isTestable && <Badge variant="outline" className="rounded-md">Test</Badge>}
                  <Badge variant="secondary" className="rounded-md">{plugin.version}</Badge>
                </span>
              </button>
            ))
          )}

          {!loading && plugins.length === 0 && (
            <p className="m-0 rounded-md bg-muted p-3 text-[13px] text-fg-muted">No plugins loaded.</p>
          )}
        </div>

        <div className="min-w-0">
          {selected ? (
            <div className="grid gap-4">
              <PluginHeader
                plugin={selected}
                saving={saving}
                testing={testing}
                onSave={form ? handleSave : null}
                onTest={selected.isTestable ? handleTest : null}
              />

              {notice && <NoticeBox notice={notice} />}

              {loading && !form && !config ? (
                <div className="grid gap-2 rounded-md bg-muted p-4">
                  <Skeleton className="h-4 w-48" />
                  <Skeleton className="h-32 w-full" />
                </div>
              ) : form ? (
                <PluginConfigFormView form={form} values={values} onChange={updateValue} />
              ) : (
                <ReadOnlyConfig config={config} />
              )}
            </div>
          ) : (
            <div className="rounded-md bg-muted p-4 text-[13px] text-fg-muted">Select a plugin.</div>
          )}
        </div>
      </div>
    </Panel>
  );
}

function PluginHeader({
  plugin,
  saving,
  testing,
  onSave,
  onTest
}: {
  plugin: PluginCatalogEntry;
  saving: boolean;
  testing: boolean;
  onSave: (() => void) | null;
  onTest: (() => void) | null;
}) {
  return (
    <div className="flex flex-wrap items-start justify-between gap-3 rounded-md border border-border bg-bg p-4">
      <div className="min-w-0">
        <h4 className="m-0 truncate text-sm font-semibold text-fg">{plugin.name}</h4>
        <p className="m-0 mt-1 truncate font-mono text-[11px] text-fg-subtle">{plugin.id}</p>
        {plugin.description && <p className="m-0 mt-2 max-w-3xl text-[13px] text-fg-muted">{plugin.description}</p>}
      </div>
      <div className="flex shrink-0 flex-wrap items-center gap-2">
        {onTest && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={testing}
            onClick={onTest}
            className="min-h-[32px] gap-1.5 border-border bg-surface font-semibold text-fg hover:bg-muted"
          >
            <TestTube2 size={14} aria-hidden />
            {testing ? "Testing" : "Test"}
          </Button>
        )}
        {onSave && (
          <Button
            type="button"
            size="sm"
            disabled={saving}
            onClick={onSave}
            className="min-h-[32px] gap-1.5 font-semibold"
          >
            <Save size={14} aria-hidden />
            {saving ? "Saving" : "Save"}
          </Button>
        )}
      </div>
    </div>
  );
}

function PluginConfigFormView({
  form,
  values,
  onChange
}: {
  form: PluginConfigForm;
  values: Record<string, PluginConfigValue>;
  onChange: (path: string, value: PluginConfigValue) => void;
}) {
  return (
    <div className="grid gap-4">
      {form.sections.map((section) => (
        <section key={section.id} className="grid gap-3 rounded-md border border-border bg-bg p-4">
          <h4 className="m-0 text-[13px] font-semibold text-fg">{section.label}</h4>
          <div className="grid gap-3 md:grid-cols-2">
            {section.fields.map((field) => (
              <PluginField
                key={field.path}
                field={field}
                value={values[field.path] ?? null}
                onChange={(value) => onChange(field.path, value)}
              />
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

function PluginField({
  field,
  value,
  onChange
}: {
  field: PluginConfigField;
  value: PluginConfigValue;
  onChange: (value: PluginConfigValue) => void;
}) {
  const id = `plugin-field-${field.path.replaceAll(".", "-")}`;

  if (field.type === "boolean") {
    return (
      <div className="flex min-h-[58px] items-start gap-2 rounded-md border border-border bg-surface p-3">
        <Checkbox
          id={id}
          checked={Boolean(value)}
          onCheckedChange={(checked) => onChange(checked === true)}
          className="mt-0.5"
        />
        <div className="grid gap-1">
          <Label htmlFor={id} className="text-[13px] font-medium text-fg">{field.label}</Label>
          {field.help && <p className="m-0 text-[11px] text-fg-subtle">{field.help}</p>}
        </div>
      </div>
    );
  }

  return (
    <div className={cn("grid gap-1.5", field.type === "textarea" && "md:col-span-2")}>
      <div className="flex items-center gap-2">
        <Label htmlFor={id} className="text-[12px] font-medium text-fg">{field.label}</Label>
        {field.required && <span className="text-[11px] font-semibold text-danger">*</span>}
        {field.secretReference && <Badge variant="outline" className="rounded-md">Secret ref</Badge>}
      </div>
      <FieldControl id={id} field={field} value={value} onChange={onChange} />
      {field.help && <p className="m-0 text-[11px] text-fg-subtle">{field.help}</p>}
    </div>
  );
}

function FieldControl({
  id,
  field,
  value,
  onChange
}: {
  id: string;
  field: PluginConfigField;
  value: PluginConfigValue;
  onChange: (value: PluginConfigValue) => void;
}) {
  if (field.type === "select") {
    return (
      <Select value={String(value ?? "")} onValueChange={(next) => onChange(next)}>
        <SelectTrigger id={id} className="h-9 w-full bg-surface text-fg">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {field.options.map((option) => (
            <SelectItem key={option} value={option}>{option}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    );
  }

  if (field.type === "textarea") {
    return (
      <textarea
        id={id}
        value={String(value ?? "")}
        onChange={(event) => onChange(event.target.value)}
        className="min-h-[84px] w-full resize-y rounded-md border border-input bg-surface px-2.5 py-2 text-xs text-fg outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-1 focus-visible:ring-ring/50"
      />
    );
  }

  return (
    <Input
      id={id}
      type={field.type === "number" ? "number" : "text"}
      value={String(value ?? "")}
      onChange={(event) => {
        if (field.type === "number") {
          onChange(event.target.value.length === 0 ? "" : Number(event.target.value));

          return;
        }

        onChange(event.target.value);
      }}
      className="h-9 bg-surface text-fg"
    />
  );
}

function ReadOnlyConfig({ config }: { config: PluginConfigView | null }) {
  if (!config?.exists) {
    return <div className="rounded-md bg-muted p-4 text-[13px] text-fg-muted">No plugin config file.</div>;
  }

  return (
    <div className="grid gap-2">
      <div className="flex items-center justify-between gap-2 text-xs text-fg-subtle">
        <span className="font-mono">{config.configPath}</span>
        {config.redactedKeys.length > 0 && <Badge variant="outline" className="rounded-md">{config.redactedKeys.length} redacted</Badge>}
      </div>
      <pre className="max-h-[520px] overflow-auto rounded-md border border-border bg-bg p-4 text-xs leading-relaxed text-fg">
        {config.sanitizedYaml}
      </pre>
    </div>
  );
}

function NoticeBox({ notice }: { notice: Exclude<Notice, null> }) {
  const isSuccess = notice.kind === "success";

  return (
    <div
      className={cn(
        "flex items-start gap-2 rounded-md p-3 text-[13px] font-medium",
        isSuccess ? "bg-success/10 text-success" : "bg-danger/10 text-danger"
      )}
    >
      {isSuccess ? <CheckCircle2 size={16} aria-hidden /> : <XCircle size={16} aria-hidden />}
      <div className="grid gap-1">
        <span>{notice.message}</span>
        {notice.details?.length ? (
          <ul className="m-0 grid gap-0.5 pl-4 text-[12px] font-normal">
            {notice.details.map((detail) => <li key={detail}>{detail}</li>)}
          </ul>
        ) : null}
      </div>
    </div>
  );
}

function valuesFromForm(form: PluginConfigForm): Record<string, PluginConfigValue> {
  return Object.fromEntries(
    form.sections.flatMap((section) => section.fields.map((field) => [field.path, field.value]))
  );
}

function testNotice(result: PluginTestResult): Exclude<Notice, null> {
  return {
    kind: result.success ? "success" : "error",
    message: result.message,
    details: result.details
  };
}
