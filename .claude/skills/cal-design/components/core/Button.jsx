import React from 'react';

/**
 * CAL Button — Rajdhani uppercase action button.
 * Variants: primary (solid cyan), neon (cyan ghost), secondary (outline),
 * ghost (text only), danger. Sizes: sm, md, lg.
 */
export function Button({
  children,
  variant = 'primary',
  size = 'md',
  disabled = false,
  type = 'button',
  iconLeft = null,
  iconRight = null,
  style = {},
  ...rest
}) {
  const sizes = {
    sm: { height: 30, padding: '0 12px', fontSize: 12 },
    md: { height: 38, padding: '0 18px', fontSize: 13 },
    lg: { height: 46, padding: '0 26px', fontSize: 15 },
  };

  const variants = {
    primary: {
      background: 'var(--cal-cyan)',
      color: 'var(--cal-text-inverse)',
      border: '1px solid var(--cal-cyan)',
    },
    neon: {
      background: 'var(--cal-cyan-ghost)',
      color: 'var(--cal-cyan)',
      border: '1px solid rgba(0,229,255,0.45)',
    },
    secondary: {
      background: 'var(--cal-surface-3)',
      color: 'var(--cal-text)',
      border: '1px solid var(--cal-border-strong)',
    },
    ghost: {
      background: 'transparent',
      color: 'var(--cal-text-muted)',
      border: '1px solid transparent',
    },
    danger: {
      background: 'var(--cal-danger-ghost)',
      color: 'var(--cal-danger)',
      border: '1px solid rgba(255,77,77,0.45)',
    },
  };

  const s = sizes[size] || sizes.md;
  const v = variants[variant] || variants.primary;

  return (
    <button
      type={type}
      disabled={disabled}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 8,
        height: s.height,
        padding: s.padding,
        fontSize: s.fontSize,
        fontFamily: 'var(--cal-font-heading)',
        fontWeight: 600,
        letterSpacing: '0.05em',
        textTransform: 'uppercase',
        borderRadius: 'var(--cal-radius-sm)',
        cursor: disabled ? 'not-allowed' : 'pointer',
        opacity: disabled ? 0.45 : 1,
        whiteSpace: 'nowrap',
        transition: 'filter var(--cal-dur) var(--cal-ease), transform var(--cal-dur-fast) var(--cal-ease), background var(--cal-dur) var(--cal-ease)',
        ...v,
        ...style,
      }}
      onMouseDown={(e) => { if (!disabled) e.currentTarget.style.transform = 'translateY(1px)'; }}
      onMouseUp={(e) => { e.currentTarget.style.transform = 'translateY(0)'; }}
      onMouseEnter={(e) => { if (!disabled) e.currentTarget.style.filter = 'brightness(1.12)'; }}
      onMouseLeave={(e) => { e.currentTarget.style.filter = 'none'; e.currentTarget.style.transform = 'translateY(0)'; }}
      {...rest}
    >
      {iconLeft}
      {children}
      {iconRight}
    </button>
  );
}
