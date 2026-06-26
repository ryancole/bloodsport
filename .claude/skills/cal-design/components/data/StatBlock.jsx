import React from 'react';

/**
 * CAL StatBlock — a big display figure over a mono caps label.
 * The standard way CAL presents data points (standings, scores, KPIs).
 */
export function StatBlock({ value, label, accent = 'plain', align = 'left' }) {
  const colors = {
    plain:   'var(--cal-text)',
    cyan:    'var(--cal-cyan)',
    magenta: 'var(--cal-magenta)',
  };
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 6, alignItems: align === 'center' ? 'center' : 'flex-start' }}>
      <span style={{
        fontFamily: 'var(--cal-font-display)',
        fontWeight: 900,
        fontSize: 44,
        lineHeight: 1,
        letterSpacing: '-0.01em',
        color: colors[accent] || colors.plain,
        fontVariantNumeric: 'tabular-nums',
      }}>{value}</span>
      <span style={{
        fontFamily: 'var(--cal-font-mono)',
        fontWeight: 600,
        fontSize: 11,
        letterSpacing: '0.24em',
        textTransform: 'uppercase',
        color: 'var(--cal-text-faint)',
      }}>{label}</span>
    </div>
  );
}
