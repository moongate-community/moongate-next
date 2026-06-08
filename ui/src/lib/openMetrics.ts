import type { OpenMetricSample } from "../types/admin";

const metricLine = /^([a-zA-Z_:][a-zA-Z0-9_:]*)(?:\{([^}]*)\})?\s+(-?(?:\d+\.?\d*|\d*\.\d+)(?:[eE][+-]?\d+)?)$/;
const labelLine = /([a-zA-Z_][a-zA-Z0-9_]*)="((?:\\"|\\\\|\\n|[^"])*)"/g;

export function parseOpenMetrics(text: string): OpenMetricSample[] {
  const samples: OpenMetricSample[] = [];

  for (const rawLine of text.split(/\r?\n/)) {
    const line = rawLine.trim();

    if (line.length === 0 || line.startsWith("#")) {
      continue;
    }

    const match = metricLine.exec(line);

    if (!match) {
      continue;
    }

    samples.push({
      name: match[1],
      labels: parseLabels(match[2] ?? ""),
      value: Number(match[3])
    });
  }

  return samples;
}

export function toMetricMap(samples: OpenMetricSample[]): Record<string, number> {
  return Object.fromEntries(samples.map((sample) => [sample.name, sample.value]));
}

function parseLabels(input: string): Record<string, string> {
  const labels: Record<string, string> = {};

  for (const match of input.matchAll(labelLine)) {
    labels[match[1]] = match[2].replaceAll("\\n", "\n").replaceAll('\\"', '"').replaceAll("\\\\", "\\");
  }

  return labels;
}
