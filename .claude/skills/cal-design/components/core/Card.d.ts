import React from 'react';

export interface CardProps {
  children: React.ReactNode;
  /** Optional top keyline accent. @default null */
  accent?: 'cyan' | 'magenta' | 'gradient' | null;
  /** Lift + brighten on hover. @default false */
  interactive?: boolean;
  /** Inner padding in px. @default 24 */
  padding?: number;
  style?: React.CSSProperties;
}

/** Dark slab surface — the default content container. */
export function Card(props: CardProps): JSX.Element;
