import { useEffect, useState } from "react";
import { getHue } from "../../../lib/adminHuesClient";
import type { HueSummary } from "../../../types/itemTemplates";
import { HueSwatch } from "../itemTemplates/HueSwatch";

type HueFieldPreviewProps = {
  accessToken: string;
  value: string;
};

/** Shows a HueSwatch for the current hue value of an Appearance field (fetched on change). */
export function HueFieldPreview({ accessToken, value }: HueFieldPreviewProps) {
  const [hue, setHue] = useState<HueSummary | null>(null);

  const parsed = Number.parseInt(value, 10);
  const valid = Number.isFinite(parsed) && parsed >= 0;

  useEffect(() => {
    if (!valid) {
      setHue(null);

      return;
    }

    let cancelled = false;

    getHue(accessToken, parsed)
      .then((result) => {
        if (!cancelled) {
          setHue(result);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setHue(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [accessToken, parsed, valid]);

  if (!hue) {
    return null;
  }

  return <HueSwatch hue={hue} mode="compact" />;
}
