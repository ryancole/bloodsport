import React from 'react';

/**
 * CAL GlitchText — the signature RGB-split wordmark treatment.
 * White text with a cyan fringe to the left and magenta to the right,
 * exactly like the "C" mark. Use for the wordmark, hero headlines and
 * big numbers — NOT for body copy.
 */
export function GlitchText({
  children,
  as = 'span',
  size = 48,
  weight = 900,
  split = null,        // px; defaults to ~5% of size
  intensity = 0.9,     // 0–1 fringe opacity
  style = {},
  ...rest
}) {
  const Tag = as;
  const dx = split == null ? Math.max(1, size * 0.045) : split;
  const cyan = `rgba(0,229,255,${intensity})`;
  const magenta = `rgba(255,46,152,${intensity})`;

  return (
    <Tag
      style={{
        fontFamily: 'var(--cal-font-display)',
        fontWeight: weight,
        fontSize: size,
        lineHeight: 1.02,
        letterSpacing: '-0.01em',
        textTransform: 'uppercase',
        color: 'var(--cal-text)',
        textShadow: `${-dx}px 0 0 ${cyan}, ${dx}px 0 0 ${magenta}`,
        margin: 0,
        ...style,
      }}
      {...rest}
    >
      {children}
    </Tag>
  );
}
