import { useState } from "react";
import { createPortal } from "react-dom";

type BodyImageCellProps = {
  imageUrl: string;
  body: number;
  bodyHex: string;
};

type PreviewPosition = {
  left: number;
  top: number;
};

const PREVIEW_SIZE = 224;
const PREVIEW_GAP = 10;

export function BodyImageCell({ imageUrl, body, bodyHex }: BodyImageCellProps) {
  const [failed, setFailed] = useState(false);
  const [previewPosition, setPreviewPosition] = useState<PreviewPosition | null>(null);

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
        className="flex h-12 w-12 shrink-0 flex-col items-center justify-center rounded-md border border-border bg-surface text-[10px] font-semibold leading-tight text-fg-muted"
        title={`Body ${body} (${bodyHex})`}
        aria-label={`Body ${body}`}
        onMouseEnter={(event) => {
          if (!failed) {
            showPreview(event.currentTarget);
          }
        }}
        onMouseLeave={() => setPreviewPosition(null)}
      >
        {failed ? (
          <>
            <span>{body}</span>
            <span className="text-[9px] opacity-70">{bodyHex}</span>
          </>
        ) : (
          <img
            src={imageUrl}
            alt={`Body ${body}`}
            className="h-full w-full object-contain"
            loading="lazy"
            onError={() => {
              setFailed(true);
              setPreviewPosition(null);
            }}
          />
        )}
      </div>

      {!failed && previewPosition && (
        createPortal(
          <div
            className="pointer-events-none fixed z-50 flex h-56 w-56 items-center justify-center rounded-md border border-border-strong bg-surface p-3 shadow-raised"
            style={{ left: previewPosition.left, top: previewPosition.top }}
          >
            <img
              src={imageUrl}
              alt=""
              aria-hidden
              className="h-full w-full object-contain"
              style={{ imageRendering: "pixelated" }}
            />
          </div>,
          document.body
        )
      )}
    </>
  );
}
