export type PluginCatalogEntry = {
  id: string;
  name: string;
  version: string;
  author: string;
  description: string | null;
  dependencies: string[];
  assemblyName: string;
  directoryName: string;
  hasConfig: boolean;
  isConfigurable: boolean;
  isTestable: boolean;
};

export type PluginConfigView = {
  pluginId: string;
  exists: boolean;
  configPath: string;
  sanitizedYaml: string;
  redactedKeys: string[];
};

export type PluginConfigValue = string | number | boolean | null;

export type PluginConfigField = {
  path: string;
  label: string;
  type: "text" | "number" | "boolean" | "select" | "textarea";
  required: boolean;
  help: string | null;
  options: string[];
  value: PluginConfigValue;
  defaultValue: PluginConfigValue;
  secretReference: boolean;
};

export type PluginConfigSection = {
  id: string;
  label: string;
  fields: PluginConfigField[];
};

export type PluginConfigForm = {
  sections: PluginConfigSection[];
};

export type PluginConfigSaveResult = {
  success: boolean;
  requiresRestart: boolean;
  errors: string[];
  config: PluginConfigView | null;
};

export type PluginTestResult = {
  success: boolean;
  message: string;
  details: string[];
};
