import React from 'react';

export interface GlitchTextProps {
  children: React.ReactNode;
  /** Rendered element. @default "span" */
  as?: keyof JSX.IntrinsicElements;
  /** Font size in px. @default 48 */
  size?: number;
  /** Font weight. @default 900 */
  weight?: number;
  /** Fringe offset in px. @default ~4.5% of size */
  split?: number;
  /** Fringe opacity 0–1. @default 0.9 */
  intensity?: number;
  style?: React.CSSProperties;
}

/**
 * The signature RGB-split wordmark treatment (cyan/magenta fringe on white).
 * @startingPoint section="Brand" subtitle="Glitch wordmark / hero headline" viewport="700x200"
 */
export function GlitchText(props: GlitchTextProps): JSX.Element;
