import React from 'react';

/**
 * CAL StatCard — dashboard KPI tile. Gradient-filled Archivo Black numeral
 * over a mono uppercase label, with the gradient keyline on top.
 */
export function StatCard({ value, label, style = {}, ...rest }) {
  return (
    <div
      style={{
        position: 'relative', overflow: 'hidden',
        background: 'var(--cal-surface-2)',
        border: '1px solid var(--cal-border)',
        borderRadius: 'var(--cal-radius-lg)',
        boxShadow: 'var(--cal-shadow-md), var(--cal-edge-top)',
        padding: '24px 22px 20px',
        display: 'flex', flexDirection: 'column', gap: 6,
        ...style,
      }}
      {...rest}
    >
      <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 2, background: 'var(--cal-gradient)' }} />
      <span style={{
        fontFamily: 'var(--cal-font-display)', fontWeight: 900, fontSize: 44, lineHeight: 1,
        letterSpacing: '-0.02em',
        background: 'var(--cal-gradient)', WebkitBackgroundClip: 'text', backgroundClip: 'text',
        WebkitTextFillColor: 'transparent',
      }}>{value}</span>
      <span style={{
        fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600,
        letterSpacing: '0.18em', textTransform: 'uppercase', color: 'var(--cal-text-faint)',
      }}>{label}</span>
    </div>
  );
}
