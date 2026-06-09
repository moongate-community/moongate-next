declare module "vanta/dist/vanta.fog.min" {
  /** Handle returned by a Vanta effect; call destroy() to tear it down. */
  interface VantaEffect {
    destroy: () => void;
  }

  /** Initializes the Vanta FOG effect on the given element. */
  const FOG: (options: Record<string, unknown>) => VantaEffect;

  export default FOG;
}
