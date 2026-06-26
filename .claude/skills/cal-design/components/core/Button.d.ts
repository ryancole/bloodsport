import React from 'react';

export interface ButtonProps {
  children: React.ReactNode;
  /** Visual style. @default "primary" */
  variant?: 'primary' | 'gradient' | 'secondary' | 'ghost';
  /** @default "md" */
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  iconLeft?: React.ReactNode;
  iconRight?: React.ReactNode;
  onClick?: (e: React.MouseEvent<HTMLButtonElement>) => void;
  type?: 'button' | 'submit' | 'reset';
}

/**
 * The primary action element. Uppercase Archivo label.
 * @startingPoint section="Core" subtitle="Action buttons in 4 variants" viewport="700x180"
 */
export function Button(props: ButtonProps): JSX.Element;
