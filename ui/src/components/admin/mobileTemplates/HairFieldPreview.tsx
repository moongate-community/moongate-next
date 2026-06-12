type HairFieldPreviewProps = {
  style: string;
  hue: string;
  facial: boolean;
};

/** Shows a small rendered preview of the current hair (or facial-hair) style over a reference body. */
export function HairFieldPreview({ style, hue, facial }: HairFieldPreviewProps) {
  const parsedStyle = Number.parseInt(style, 10);

  if (!Number.isFinite(parsedStyle) || parsedStyle <= 0) {
    return null;
  }

  const parsedHue = Number.parseInt(hue, 10);
  const hueParam = Number.isFinite(parsedHue) && parsedHue > 0 ? parsedHue : 0;

  return (
    <img
      src={`/api/mobiles/hair/${parsedStyle}.png?hue=${hueParam}&facial=${facial}`}
      alt=""
      className="h-16 w-16 object-contain"
      style={{ imageRendering: "pixelated" }}
      onError={(event) => {
        (event.target as HTMLImageElement).style.display = "none";
      }}
    />
  );
}
