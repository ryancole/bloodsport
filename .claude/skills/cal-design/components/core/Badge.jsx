import React from 'react';

/**
 * CAL Badge — compact status / category chip in mono caps.
 * tone: "live" (pulsing magenta) | "win" (cyan) | "loss" (magenta) |
 * "neutral" | "outline".
 */
export function Badge({ children, tone = 'neutral', dot = false, ...rest }) {
  const tones = {
    live:    { bg: 'var(--cal-magenta-ghost)', fg: 'var(--cal-magenta)', bd: 'transparent' },
    win:     { bg: 'var(--cal-cyan-ghost)',    fg: 'var(--cal-cyan)',    bd: 'transparent' },
    loss:    { bg: 'var(--cal-magenta-ghost)', fg: 'var(--cal-magenta)', bd: 'transparent' },
    neutral: { bg: 'var(--cal-surface-3)',     fg: 'var(--cal-text-muted)', bd: 'transparent' },
    outline: { bg: 'transparent',              fg: 'var(--cal-text-muted)', bd: 'var(--cal-border-strong)' },
  };
  const t = tones[tone] || tones.neutral;

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        height: 22,
        padding: '0 10px',
        background: t.bg,
        color: t.fg,
        border: `1px solid ${t.bd}`,
        borderRadius: 'var(--cal-radius-pill)',
        fontFamily: 'var(--cal-font-mono)',
        fontWeight: 600,
        fontSize: 11,
        letterSpacing: '0.14em',
        textTransform: 'uppercase',
        lineHeight: 1,
        whiteSpace: 'nowrap',
      }}
      {...rest}
    >
      {(dot || tone === 'live') && (
        <span
          style={{
            width: 6,
            height: 6,
            borderRadius: '50%',
            background: t.fg,
            boxShadow: tone === 'live' ? `0 0 0 0 ${t.fg}` : 'none',
            animation: tone === 'live' ? 'cal-pulse 1.6s var(--cal-ease-inout) infinite' : 'none',
          }}
        />
      )}
      {children}
      <style>{`@keyframes cal-pulse{0%{box-shadow:0 0 0 0 rgba(255,46,152,.5)}70%{box-shadow:0 0 0 6px rgba(255,46,152,0)}100%{box-shadow:0 0 0 0 rgba(255,46,152,0)}}`}</style>
    </span>
  );
}
