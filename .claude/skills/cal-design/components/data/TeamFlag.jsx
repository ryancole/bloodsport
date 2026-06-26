import React from 'react';

/**
 * CAL TeamFlag — small team/user avatar. Shows a logo image when provided,
 * otherwise a gradient tile with the team's initials.
 */
export function TeamFlag({ name = '', logoUrl = null, size = 24, style = {}, ...rest }) {
  const initials = name
    .split(/\s+/).filter(Boolean).slice(0, 2)
    .map((w) => w[0]).join('').toUpperCase() || '?';
  return (
    <span
      style={{
        display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
        width: size, height: size, flexShrink: 0,
        borderRadius: Math.max(4, size * 0.22),
        border: '1px solid var(--cal-border-strong)',
        background: logoUrl ? `center/cover no-repeat url(${logoUrl})` : 'var(--cal-gradient)',
        color: 'var(--cal-text-inverse)',
        fontFamily: 'var(--cal-font-display)', fontWeight: 800,
        fontSize: Math.round(size * 0.42), letterSpacing: '-0.02em',
        overflow: 'hidden',
        ...style,
      }}
      title={name || undefined}
      {...rest}
    >
      {!logoUrl && initials}
    </span>
  );
}
