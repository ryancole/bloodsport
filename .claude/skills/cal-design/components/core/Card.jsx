import React from 'react';

/**
 * CAL Card — the standard dark panel. Surface-2 with hairline border,
 * deep shadow + inset top edge, and the signature 2px cyan→magenta
 * keyline across the top. Optional header (eyebrow + action).
 */
export function Card({
  children,
  eyebrow = null,
  action = null,
  keyline = true,
  elevation = 'lg',
  padded = true,
  style = {},
  bodyStyle = {},
  ...rest
}) {
  const shadows = {
    sm: 'var(--cal-shadow-sm), var(--cal-edge-top)',
    md: 'var(--cal-shadow-md), var(--cal-edge-top)',
    lg: 'var(--cal-shadow-lg), var(--cal-edge-top)',
  };
  return (
    <div
      style={{
        position: 'relative',
        overflow: 'hidden',
        background: 'var(--cal-surface-2)',
        border: '1px solid var(--cal-border)',
        borderRadius: 'var(--cal-radius-lg)',
        boxShadow: shadows[elevation] || shadows.lg,
        ...style,
      }}
      {...rest}
    >
      {keyline && (
        <div style={{ position: 'absolute', top: 0, left: 0, right: 0, height: 2, background: 'var(--cal-gradient)' }} />
      )}
      {(eyebrow || action) && (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '18px 20px 0' }}>
          {eyebrow && (
            <span style={{
              fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600,
              letterSpacing: '0.24em', textTransform: 'uppercase', color: 'var(--cal-text-faint)',
            }}>{eyebrow}</span>
          )}
          {action}
        </div>
      )}
      <div style={{ padding: padded ? '16px 20px 18px' : 0, ...bodyStyle }}>
        {children}
      </div>
    </div>
  );
}
