import type { ReactNode } from "react";

export type DefinitionItem = {
  term: string;
  value: ReactNode;
  /** Render the value in the tabular mono face (for technical values). */
  mono?: boolean;
};

type DefinitionListProps = {
  items: DefinitionItem[];
};

export function DefinitionList({ items }: DefinitionListProps) {
  return (
    <dl className="m-0 grid divide-y divide-border">
      {items.map((item) => (
        <div key={item.term} className="grid gap-1 py-2.5 sm:grid-cols-[112px_minmax(0,1fr)] sm:gap-3">
          <dt className="text-[12px] font-medium leading-snug text-fg-subtle">{item.term}</dt>
          <dd className={`m-0 break-words text-[13px] font-medium leading-snug text-fg ${item.mono ? "font-mono" : ""}`}>
            {item.value}
          </dd>
        </div>
      ))}
    </dl>
  );
}
