import * as React from 'react';

/**
 * Standard dark panel — surface-2, deep shadow, inset top edge and the
 * signature cyan→magenta keyline. The container for every dashboard module.
 *
 * @startingPoint section="Core" subtitle="Dark panel with gradient keyline" viewport="700x260"
 */
export interface CardProps {
  children: React.ReactNode;
  /** Mono uppercase label rendered in the header row. */
  eyebrow?: React.ReactNode;
  /** Right-aligned header action (e.g. "All →" link). */
  action?: React.ReactNode;
  /** Show the 2px gradient keyline across the top. @default true */
  keyline?: boolean;
  /** @default "lg" */
  elevation?: 'sm' | 'md' | 'lg';
  /** Apply default body padding. @default true */
  padded?: boolean;
  style?: React.CSSProperties;
  bodyStyle?: React.CSSProperties;
}

export function Card(props: CardProps): JSX.Element;
