import { useState } from "react";
import { ImageOff } from "lucide-react";

type ItemImageCellProps = {
  src: string;
  alt: string;
  size?: "small" | "large";
};

export function ItemImageCell({ src, alt, size = "small" }: ItemImageCellProps) {
  const [failed, setFailed] = useState(false);
  const boxClass = size === "large" ? "h-20 w-20" : "h-8 w-8";

  const zoomClass = size === "large" ? "group-hover:scale-125" : "group-hover:scale-[1.85]";

  return (
    <div
      className={`${boxClass} group inline-flex shrink-0 items-center justify-center overflow-hidden rounded-md border border-border bg-bg transition-colors duration-150 hover:border-border-strong`}
    >
      {failed ? (
        <ImageOff size={size === "large" ? 22 : 14} aria-hidden className="text-fg-subtle" />
      ) : (
        <img
          src={src}
          alt={alt}
          loading="lazy"
          onError={() => setFailed(true)}
          className={`max-h-full max-w-full object-contain transition-transform duration-200 ease-out ${zoomClass}`}
        />
      )}
    </div>
  );
}
