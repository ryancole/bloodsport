import React from 'react';

/** CAL Input — dark field with a cyan focus ring. */
export function Input({ label = null, hint = null, mono = false, style = {}, ...rest }) {
  const [focus, setFocus] = React.useState(false);
  return (
    <label style={{ display: 'flex', flexDirection: 'column', gap: 8, width: '100%' }}>
      {label && (
        <span style={{
          fontFamily: 'var(--cal-font-mono)', fontWeight: 600, fontSize: 11,
          letterSpacing: '0.18em', textTransform: 'uppercase', color: 'var(--cal-text-faint)',
        }}>{label}</span>
      )}
      <input
        onFocus={(e) => { setFocus(true); rest.onFocus && rest.onFocus(e); }}
        onBlur={(e) => { setFocus(false); rest.onBlur && rest.onBlur(e); }}
        style={{
          height: 44,
          padding: '0 14px',
          background: 'var(--cal-surface-1)',
          color: 'var(--cal-text)',
          border: `1px solid ${focus ? 'var(--cal-cyan)' : 'var(--cal-border-strong)'}`,
          borderRadius: 'var(--cal-radius-sm)',
          fontFamily: mono ? 'var(--cal-font-mono)' : 'var(--cal-font-body)',
          fontSize: 15,
          outline: 'none',
          boxShadow: focus ? '0 0 0 3px var(--cal-cyan-ghost)' : 'none',
          transition: 'border-color var(--cal-dur) var(--cal-ease), box-shadow var(--cal-dur) var(--cal-ease)',
          ...style,
        }}
        {...rest}
      />
      {hint && (
        <span style={{ fontSize: 12, color: 'var(--cal-text-faint)', fontFamily: 'var(--cal-font-body)' }}>{hint}</span>
      )}
    </label>
  );
}
