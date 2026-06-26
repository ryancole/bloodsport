import React from 'react';

/**
 * CAL Badge — pill status chip in JetBrains Mono.
 * Ghost-fill + colored text. Maps to league statuses (regular/playoff/
 * registration/preseason/win/loss).
 */
export function Badge({ children, tone = 'neutral', style = {}, ...rest }) {
  const tones = {
    neutral:  { background: 'var(--cal-surface-3)',     color: 'var(--cal-text-muted)' },
    cyan:     { background: 'var(--cal-cyan-ghost)',    color: 'var(--cal-cyan)' },
    magenta:  { background: 'var(--cal-magenta-ghost)', color: 'var(--cal-magenta)' },
    success:  { background: 'var(--cal-success-ghost)', color: 'var(--cal-success)' },
    warning:  { background: 'var(--cal-warning-ghost)', color: 'var(--cal-warning)' },
    danger:   { background: 'var(--cal-danger-ghost)',  color: 'var(--cal-danger)' },
  };
  const t = tones[tone] || tones.neutral;
  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        height: 20,
        padding: '0 8px',
        borderRadius: 'var(--cal-radius-pill)',
        fontFamily: 'var(--cal-font-mono)',
        fontWeight: 600,
        fontSize: 10,
        letterSpacing: '0.12em',
        textTransform: 'uppercase',
        whiteSpace: 'nowrap',
        ...t,
        ...style,
      }}
      {...rest}
    >
      {children}
    </span>
  );
}
