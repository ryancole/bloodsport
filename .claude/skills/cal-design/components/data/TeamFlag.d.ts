import * as React from 'react';

/**
 * Team / user avatar — logo image, or gradient initials fallback.
 *
 * @startingPoint section="Data" subtitle="Team avatar with initials fallback" viewport="700x100"
 */
export interface TeamFlagProps {
  /** Team or user display name (drives initials + title). */
  name?: string;
  /** Logo image URL; when set, replaces the gradient/initials. */
  logoUrl?: string | null;
  /** Pixel size of the square. @default 24 */
  size?: number;
  style?: React.CSSProperties;
}

export function TeamFlag(props: TeamFlagProps): JSX.Element;
