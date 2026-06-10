import { useState } from "react";
import { ImageOff } from "lucide-react";

type ItemImageCellProps = {
  src: string;
  alt: string;
  size?: "small" | "large";
};

export function ItemImageCell({ src, alt, size = "small" }: ItemImageCellProps) {
  const [failed, setFailed] = useState(false);
  const boxClass = size === "large" ? "h-24 w-24" : "h-10 w-10";

  return (
    <div className={`${boxClass} inline-flex shrink-0 items-center justify-center rounded-md border border-border bg-muted`}>
      {failed ? (
        <ImageOff size={size === "large" ? 24 : 16} aria-hidden className="text-fg-subtle" />
      ) : (
        <img
          src={src}
          alt={alt}
          loading="lazy"
          onError={() => setFailed(true)}
          className="max-h-full max-w-full object-contain"
        />
      )}
    </div>
  );
}
