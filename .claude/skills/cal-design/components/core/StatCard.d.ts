import * as React from 'react';

/**
 * Dashboard KPI tile — gradient Archivo Black numeral over a mono label.
 *
 * @startingPoint section="Core" subtitle="Gradient KPI stat tile" viewport="700x160"
 */
export interface StatCardProps {
  /** The big numeral / value. */
  value: React.ReactNode;
  /** Mono uppercase caption. */
  label: React.ReactNode;
  style?: React.CSSProperties;
}

export function StatCard(props: StatCardProps): JSX.Element;
