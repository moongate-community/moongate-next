import { useEffect, useRef } from "react";
import * as THREE from "three";
import FOG from "vanta/dist/vanta.fog.min";

// Fog palette tuned to the administrator login backdrop.
const ADMIN_FOG_OPTIONS = {
  mouseControls: true,
  touchControls: true,
  gyroControls: false,
  minHeight: 200,
  minWidth: 200,
  highlightColor: 0x2f5c3b,
  midtoneColor: 0x26382c,
  lowlightColor: 0x16231b,
  baseColor: 0x16231b,
  blurFactor: 0.6,
  speed: 1.1,
  zoom: 0.85
};

/**
 * Full-bleed animated Vanta FOG backdrop for the administrator login screen.
 * Respects prefers-reduced-motion (renders a static container, letting the CSS
 * gradient show through) and tears the effect down on unmount.
 */
export function VantaBackground() {
  const ref = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      return;
    }

    const el = ref.current;

    if (!el) {
      return;
    }

    const effect = FOG({ el, THREE, ...ADMIN_FOG_OPTIONS });

    return () => {
      effect.destroy();
    };
  }, []);

  return <div ref={ref} className="vanta-bg" aria-hidden="true" />;
}
