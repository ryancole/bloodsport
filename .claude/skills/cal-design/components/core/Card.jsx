import React from 'react';

/**
 * CAL Card — dark slab surface with hairline border, soft top edge
 * and deep ambient shadow. The default container for content.
 * accent: optional "cyan" | "magenta" | "gradient" top keyline.
 */
export function Card({ children, accent = null, interactive = false, padding = 24, style = {}, ...rest }) {
  const [hover, setHover] = React.useState(false);

  const accentBar = accent && {
    content: '""',
    position: 'absolute',
    top: 0, left: 0, right: 0,
    height: 2,
    background:
      accent === 'gradient' ? 'var(--cal-gradient)'
      : accent === 'magenta' ? 'var(--cal-magenta)'
      : 'var(--cal-cyan)',
  };

  return (
    <div
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        position: 'relative',
        background: hover && interactive ? 'var(--cal-surface-3)' : 'var(--cal-surface-2)',
        border: '1px solid var(--cal-border)',
        borderRadius: 'var(--cal-radius-lg)',
        boxShadow: hover && interactive
          ? 'var(--cal-shadow-xl), var(--cal-edge-top)'
          : 'var(--cal-shadow-lg), var(--cal-edge-top)',
        padding,
        overflow: 'hidden',
        transition: 'background var(--cal-dur) var(--cal-ease), box-shadow var(--cal-dur) var(--cal-ease), transform var(--cal-dur) var(--cal-ease)',
        transform: hover && interactive ? 'translateY(-2px)' : 'none',
        cursor: interactive ? 'pointer' : 'default',
        ...style,
      }}
      {...rest}
    >
      {accent && <span style={accentBar} />}
      {children}
    </div>
  );
}
