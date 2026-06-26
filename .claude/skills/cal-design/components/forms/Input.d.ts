import * as React from 'react';

/**
 * Dark text field with optional mono label and hint; border lifts to cyan on focus.
 *
 * @startingPoint section="Forms" subtitle="Dark labelled text field" viewport="700x140"
 */
export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  /** Mono uppercase label above the field. */
  label?: React.ReactNode;
  /** Helper or error text below the field. */
  hint?: React.ReactNode;
  /** Red border + hint when true. @default false */
  invalid?: boolean;
}

export function Input(props: InputProps): JSX.Element;
