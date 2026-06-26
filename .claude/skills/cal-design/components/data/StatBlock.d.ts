import React from 'react';

export interface StatBlockProps {
  value: React.ReactNode;
  label: string;
  /** Figure color. @default "plain" */
  accent?: 'plain' | 'cyan' | 'magenta';
  /** @default "left" */
  align?: 'left' | 'center';
}

/** Big display figure over a mono caps label — CAL's data unit. */
export function StatBlock(props: StatBlockProps): JSX.Element;
