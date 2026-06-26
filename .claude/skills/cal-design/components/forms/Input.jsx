import React from 'react';

/**
 * CAL Input — dark text field. Surface-1 fill, hairline border that lifts
 * to cyan on focus. Pairs with an optional mono uppercase label.
 */
export function Input({ label = null, hint = null, invalid = false, style = {}, id, ...rest }) {
  const fieldId = id || (label ? `cal-${String(label).replace(/\W+/g, '-').toLowerCase()}` : undefined);
  return (
    <label htmlFor={fieldId} style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
      {label && (
        <span style={{
          fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600,
          letterSpacing: '0.14em', textTransform: 'uppercase', color: 'var(--cal-text-faint)',
        }}>{label}</span>
      )}
      <input
        id={fieldId}
        style={{
          height: 38, padding: '0 12px',
          background: 'var(--cal-surface-1)',
          color: 'var(--cal-text)',
          border: `1px solid ${invalid ? 'var(--cal-danger)' : 'var(--cal-border-strong)'}`,
          borderRadius: 'var(--cal-radius-sm)',
          fontFamily: 'var(--cal-font-display)', fontSize: 14,
          outline: 'none',
          transition: 'border-color var(--cal-dur) var(--cal-ease), box-shadow var(--cal-dur) var(--cal-ease)',
          ...style,
        }}
        onFocus={(e) => { if (!invalid) { e.target.style.borderColor = 'var(--cal-cyan)'; e.target.style.boxShadow = '0 0 0 3px var(--cal-cyan-ghost)'; } }}
        onBlur={(e) => { e.target.style.borderColor = invalid ? 'var(--cal-danger)' : 'var(--cal-border-strong)'; e.target.style.boxShadow = 'none'; }}
        {...rest}
      />
      {hint && (
        <span style={{ fontSize: 12, color: invalid ? 'var(--cal-danger)' : 'var(--cal-text-faint)' }}>{hint}</span>
      )}
    </label>
  );
}
