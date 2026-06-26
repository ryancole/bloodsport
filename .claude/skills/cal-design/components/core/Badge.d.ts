import React from 'react';

export interface BadgeProps {
  children: React.ReactNode;
  /** @default "neutral" */
  tone?: 'live' | 'win' | 'loss' | 'neutral' | 'outline';
  /** Show a leading status dot. "live" always pulses. */
  dot?: boolean;
}

/** Compact status / category chip in mono caps. */
export function Badge(props: BadgeProps): JSX.Element;
