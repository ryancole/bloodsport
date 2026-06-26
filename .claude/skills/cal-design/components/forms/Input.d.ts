import React from 'react';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  /** Mono caps label above the field. */
  label?: string | null;
  /** Helper text below the field. */
  hint?: string | null;
  /** Use the mono family for the value (codes, scores). @default false */
  mono?: boolean;
}

/** Dark text field with a cyan focus ring. */
export function Input(props: InputProps): JSX.Element;
