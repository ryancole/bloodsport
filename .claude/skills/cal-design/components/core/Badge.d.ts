import * as React from 'react';

/**
 * Pill status chip (JetBrains Mono, uppercase). Conveys league status —
 * regular season, playoffs, registration, win/loss.
 *
 * @startingPoint section="Core" subtitle="Mono pill status chip" viewport="700x100"
 */
export interface BadgeProps {
  children: React.ReactNode;
  /** Status tone. @default "neutral" */
  tone?: 'neutral' | 'cyan' | 'magenta' | 'success' | 'warning' | 'danger';
  style?: React.CSSProperties;
}

export function Badge(props: BadgeProps): JSX.Element;
