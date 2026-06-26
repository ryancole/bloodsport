import * as React from 'react';

/**
 * Solid-cyan action button in Rajdhani uppercase. The default CAL CTA.
 *
 * @startingPoint section="Core" subtitle="Uppercase Rajdhani action button" viewport="700x120"
 */
export interface ButtonProps {
  children: React.ReactNode;
  /** Visual style. @default "primary" */
  variant?: 'primary' | 'neon' | 'secondary' | 'ghost' | 'danger';
  /** @default "md" */
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  type?: 'button' | 'submit' | 'reset';
  iconLeft?: React.ReactNode;
  iconRight?: React.ReactNode;
  style?: React.CSSProperties;
  onClick?: (e: React.MouseEvent<HTMLButtonElement>) => void;
}

export function Button(props: ButtonProps): JSX.Element;
