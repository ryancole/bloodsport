import React from 'react';

/**
 * CAL Button — the primary action primitive.
 * Variants: "primary" (cyan fill), "gradient" (cyan→magenta), "secondary"
 * (hairline outline), "ghost" (text only). Sizes: sm | md | lg.
 */
export function Button({
  children,
  variant = 'primary',
  size = 'md',
  disabled = false,
  iconLeft = null,
  iconRight = null,
  onClick,
  type = 'button',
  ...rest
}) {
  const sizes = {
    sm: { padding: '0 14px', height: 34, fontSize: 13 },
    md: { padding: '0 20px', height: 42, fontSize: 14 },
    lg: { padding: '0 28px', height: 52, fontSize: 16 },
  };

  const base = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    height: sizes[size].height,
    padding: sizes[size].padding,
    fontFamily: 'var(--cal-font-display)',
    fontWeight: 700,
    fontSize: sizes[size].fontSize,
    letterSpacing: '0.02em',
    textTransform: 'uppercase',
    border: '1px solid transparent',
    borderRadius: 'var(--cal-radius-sm)',
    cursor: disabled ? 'not-allowed' : 'pointer',
    opacity: disabled ? 0.45 : 1,
    transition: 'transform var(--cal-dur-fast) var(--cal-ease), filter var(--cal-dur) var(--cal-ease), background var(--cal-dur) var(--cal-ease)',
    whiteSpace: 'nowrap',
  };

  const variants = {
    primary: {
      background: 'var(--cal-cyan)',
      color: 'var(--cal-text-inverse)',
    },
    gradient: {
      background: 'var(--cal-gradient)',
      color: 'var(--cal-text-inverse)',
    },
    secondary: {
      background: 'transparent',
      color: 'var(--cal-text)',
      borderColor: 'var(--cal-border-strong)',
    },
    ghost: {
      background: 'transparent',
      color: 'var(--cal-text-muted)',
    },
  };

  const [hover, setHover] = React.useState(false);
  const [press, setPress] = React.useState(false);

  const hoverStyle = !disabled && hover
    ? (variant === 'secondary'
        ? { borderColor: 'var(--cal-cyan)', color: 'var(--cal-cyan)' }
        : variant === 'ghost'
          ? { color: 'var(--cal-text)' }
          : { filter: 'brightness(1.08)' })
    : null;

  return (
    <button
      type={type}
      disabled={disabled}
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => { setHover(false); setPress(false); }}
      onMouseDown={() => setPress(true)}
      onMouseUp={() => setPress(false)}
      style={{
        ...base,
        ...variants[variant],
        ...hoverStyle,
        transform: press && !disabled ? 'translateY(1px) scale(0.99)' : 'none',
      }}
      {...rest}
    >
      {iconLeft}
      {children}
      {iconRight}
    </button>
  );
}
