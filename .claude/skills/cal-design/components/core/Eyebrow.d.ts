import * as React from 'react';

/**
 * Mono uppercase kicker label — sits above titles and labels sections.
 */
export interface EyebrowProps {
  children: React.ReactNode;
  /** @default "wide" */
  tracking?: 'normal' | 'wide' | 'xwide';
  /** @default "faint" */
  color?: 'faint' | 'muted' | 'cyan' | 'magenta';
  style?: React.CSSProperties;
}

export function Eyebrow(props: EyebrowProps): JSX.Element;
