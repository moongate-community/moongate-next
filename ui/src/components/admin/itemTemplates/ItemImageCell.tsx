import { ImageOff } from "lucide-react";
import { useState } from "react";
import { createPortal } from "react-dom";

type ItemImageCellProps = {
  src: string;
  alt: string;
  size?: "small" | "large";
};

type PreviewPosition = {
  left: number;
  top: number;
};

const PREVIEW_SIZE = 176;
const PREVIEW_GAP = 10;

export function ItemImageCell({ src, alt, size = "small" }: ItemImageCellProps) {
  const [failed, setFailed] = useState(false);
  const [previewPosition, setPreviewPosition] = useState<PreviewPosition | null>(null);
  const boxClass = size === "large" ? "h-20 w-20" : "h-8 w-8";

  function showPreview(target: HTMLDivElement) {
    const rect = target.getBoundingClientRect();
    const fitsRight = rect.right + PREVIEW_GAP + PREVIEW_SIZE <= window.innerWidth;
    const left = fitsRight ? rect.right + PREVIEW_GAP : Math.max(PREVIEW_GAP, rect.left - PREVIEW_GAP - PREVIEW_SIZE);
    const top = Math.min(
      Math.max(PREVIEW_GAP, rect.top + rect.height / 2 - PREVIEW_SIZE / 2),
      window.innerHeight - PREVIEW_SIZE - PREVIEW_GAP
    );

    setPreviewPosition({ left, top });
  }

  return (
    <>
      <div
        className={`${boxClass} inline-flex shrink-0 items-center justify-center rounded-md border border-border bg-bg transition-colors duration-150 hover:border-border-strong`}
        onMouseEnter={(event) => {
          if (!failed) {
            showPreview(event.currentTarget);
          }
        }}
        onMouseLeave={() => setPreviewPosition(null)}
      >
        {failed ? (
          <ImageOff size={size === "large" ? 22 : 14} aria-hidden className="text-fg-subtle" />
        ) : (
          <img
            src={src}
            alt={alt}
            loading="lazy"
            onError={() => {
              setFailed(true);
              setPreviewPosition(null);
            }}
            className="max-h-full max-w-full object-contain"
          />
        )}
      </div>

      {!failed && previewPosition && (
        createPortal(
          <div
            className="pointer-events-none fixed z-50 flex h-44 w-44 items-center justify-center rounded-md border border-border-strong bg-surface p-3 shadow-raised"
            style={{ left: previewPosition.left, top: previewPosition.top }}
          >
            <img
              src={src}
              alt=""
              aria-hidden
              className="h-full w-full object-fill"
              style={{ imageRendering: "pixelated" }}
            />
          </div>,
          document.body
        )
      )}
    </>
  );
}
