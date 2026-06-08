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
    <dl className="m-0 grid grid-cols-1 gap-x-4 gap-y-4 sm:grid-cols-2">
      {items.map((item) => (
        <div key={item.term} className="grid gap-1">
          <dt className="text-[11px] font-bold uppercase tracking-wide text-fg-subtle">{item.term}</dt>
          <dd className={`m-0 break-words text-sm font-medium leading-snug text-fg ${item.mono ? "font-mono" : ""}`}>
            {item.value}
          </dd>
        </div>
      ))}
    </dl>
  );
}
