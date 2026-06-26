import React from 'react';

/**
 * CAL Eyebrow — mono uppercase label used above titles and as section
 * kickers. The connective tissue of the dark dashboard.
 */
export function Eyebrow({ children, tracking = 'wide', color = 'faint', style = {}, ...rest }) {
  const tracks = { normal: '0.12em', wide: '0.18em', xwide: '0.24em' };
  const colors = {
    faint: 'var(--cal-text-faint)',
    muted: 'var(--cal-text-muted)',
    cyan: 'var(--cal-cyan)',
    magenta: 'var(--cal-magenta)',
  };
  return (
    <span
      style={{
        fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600,
        letterSpacing: tracks[tracking] || tracks.wide,
        textTransform: 'uppercase',
        color: colors[color] || colors.faint,
        ...style,
      }}
      {...rest}
    >{children}</span>
  );
}
