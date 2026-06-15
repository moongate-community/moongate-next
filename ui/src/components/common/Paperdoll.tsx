type PaperdollProps = {
  src: string;
  alt?: string;
  className?: string;
};

/** Reusable presentational paperdoll image (source-agnostic). Hides itself if the image fails to load. */
export function Paperdoll({ src, alt = "Paperdoll", className }: PaperdollProps) {
  return (
    <img
      src={src}
      alt={alt}
      className={className}
      style={{ imageRendering: "pixelated" }}
      onError={(event) => {
        (event.target as HTMLImageElement).style.display = "none";
      }}
    />
  );
}
