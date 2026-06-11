import { useState } from "react";

type BodyImageCellProps = {
  imageUrl: string;
  body: number;
  bodyHex: string;
};

export function BodyImageCell({ imageUrl, body, bodyHex }: BodyImageCellProps) {
  const [failed, setFailed] = useState(false);

  if (failed) {
    return (
      <div
        className="flex h-12 w-12 shrink-0 flex-col items-center justify-center rounded-md border border-border bg-surface text-[10px] font-semibold leading-tight text-fg-muted"
        title={`Body ${body} (${bodyHex})`}
        aria-label={`Body ${body}`}
      >
        <span>{body}</span>
        <span className="text-[9px] opacity-70">{bodyHex}</span>
      </div>
    );
  }

  return (
    <img
      src={imageUrl}
      alt={`Body ${body}`}
      title={`Body ${body} (${bodyHex})`}
      className="h-12 w-12 shrink-0 rounded-md border border-border bg-surface object-contain"
      loading="lazy"
      onError={() => setFailed(true)}
    />
  );
}
