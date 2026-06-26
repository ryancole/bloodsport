/* @ds-bundle: {"format":3,"namespace":"ChampionsAmateurLeagueDesignSystem_df3b47","components":[{"name":"Badge","sourcePath":"components/core/Badge.jsx"},{"name":"Button","sourcePath":"components/core/Button.jsx"},{"name":"Card","sourcePath":"components/core/Card.jsx"},{"name":"Eyebrow","sourcePath":"components/core/Eyebrow.jsx"},{"name":"StatCard","sourcePath":"components/core/StatCard.jsx"},{"name":"TeamFlag","sourcePath":"components/data/TeamFlag.jsx"},{"name":"Input","sourcePath":"components/forms/Input.jsx"}],"sourceHashes":{"components/core/Badge.jsx":"56172f2696db","components/core/Button.jsx":"c481a490101c","components/core/Card.jsx":"0d2c15914a5a","components/core/Eyebrow.jsx":"3e67bb137158","components/core/StatCard.jsx":"7312926972de","components/data/TeamFlag.jsx":"43d84cef7e2b","components/forms/Input.jsx":"48fc052e5681","ui_kits/cal-web/Dashboard.jsx":"d8b8c8166b57","ui_kits/cal-web/Playoffs.jsx":"7fc47f81d1ae","ui_kits/cal-web/Screens.jsx":"d5e56bb0a62f","ui_kits/cal-web/TopNav.jsx":"5c535481a518","ui_kits/cal-web/data.js":"293fca7e93f4"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.ChampionsAmateurLeagueDesignSystem_df3b47 = window.ChampionsAmateurLeagueDesignSystem_df3b47 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/core/Badge.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL Badge — pill status chip in JetBrains Mono.
 * Ghost-fill + colored text. Maps to league statuses (regular/playoff/
 * registration/preseason/win/loss).
 */
function Badge({
  children,
  tone = 'neutral',
  style = {},
  ...rest
}) {
  const tones = {
    neutral: {
      background: 'var(--cal-surface-3)',
      color: 'var(--cal-text-muted)'
    },
    cyan: {
      background: 'var(--cal-cyan-ghost)',
      color: 'var(--cal-cyan)'
    },
    magenta: {
      background: 'var(--cal-magenta-ghost)',
      color: 'var(--cal-magenta)'
    },
    success: {
      background: 'var(--cal-success-ghost)',
      color: 'var(--cal-success)'
    },
    warning: {
      background: 'var(--cal-warning-ghost)',
      color: 'var(--cal-warning)'
    },
    danger: {
      background: 'var(--cal-danger-ghost)',
      color: 'var(--cal-danger)'
    }
  };
  const t = tones[tone] || tones.neutral;
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      height: 20,
      padding: '0 8px',
      borderRadius: 'var(--cal-radius-pill)',
      fontFamily: 'var(--cal-font-mono)',
      fontWeight: 600,
      fontSize: 10,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      whiteSpace: 'nowrap',
      ...t,
      ...style
    }
  }, rest), children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Badge.jsx", error: String((e && e.message) || e) }); }

// components/core/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL Button — Rajdhani uppercase action button.
 * Variants: primary (solid cyan), neon (cyan ghost), secondary (outline),
 * ghost (text only), danger. Sizes: sm, md, lg.
 */
function Button({
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
    sm: {
      height: 30,
      padding: '0 12px',
      fontSize: 12
    },
    md: {
      height: 38,
      padding: '0 18px',
      fontSize: 13
    },
    lg: {
      height: 46,
      padding: '0 26px',
      fontSize: 15
    }
  };
  const variants = {
    primary: {
      background: 'var(--cal-cyan)',
      color: 'var(--cal-text-inverse)',
      border: '1px solid var(--cal-cyan)'
    },
    neon: {
      background: 'var(--cal-cyan-ghost)',
      color: 'var(--cal-cyan)',
      border: '1px solid rgba(0,229,255,0.45)'
    },
    secondary: {
      background: 'var(--cal-surface-3)',
      color: 'var(--cal-text)',
      border: '1px solid var(--cal-border-strong)'
    },
    ghost: {
      background: 'transparent',
      color: 'var(--cal-text-muted)',
      border: '1px solid transparent'
    },
    danger: {
      background: 'var(--cal-danger-ghost)',
      color: 'var(--cal-danger)',
      border: '1px solid rgba(255,77,77,0.45)'
    }
  };
  const s = sizes[size] || sizes.md;
  const v = variants[variant] || variants.primary;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: type,
    disabled: disabled,
    style: {
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
      ...style
    },
    onMouseDown: e => {
      if (!disabled) e.currentTarget.style.transform = 'translateY(1px)';
    },
    onMouseUp: e => {
      e.currentTarget.style.transform = 'translateY(0)';
    },
    onMouseEnter: e => {
      if (!disabled) e.currentTarget.style.filter = 'brightness(1.12)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.filter = 'none';
      e.currentTarget.style.transform = 'translateY(0)';
    }
  }, rest), iconLeft, children, iconRight);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Button.jsx", error: String((e && e.message) || e) }); }

// components/core/Card.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL Card — the standard dark panel. Surface-2 with hairline border,
 * deep shadow + inset top edge, and the signature 2px cyan→magenta
 * keyline across the top. Optional header (eyebrow + action).
 */
function Card({
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
    lg: 'var(--cal-shadow-lg), var(--cal-edge-top)'
  };
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      overflow: 'hidden',
      background: 'var(--cal-surface-2)',
      border: '1px solid var(--cal-border)',
      borderRadius: 'var(--cal-radius-lg)',
      boxShadow: shadows[elevation] || shadows.lg,
      ...style
    }
  }, rest), keyline && /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      top: 0,
      left: 0,
      right: 0,
      height: 2,
      background: 'var(--cal-gradient)'
    }
  }), (eyebrow || action) && /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '18px 20px 0'
    }
  }, eyebrow && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: '0.24em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)'
    }
  }, eyebrow), action), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: padded ? '16px 20px 18px' : 0,
      ...bodyStyle
    }
  }, children));
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Card.jsx", error: String((e && e.message) || e) }); }

// components/core/Eyebrow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL Eyebrow — mono uppercase label used above titles and as section
 * kickers. The connective tissue of the dark dashboard.
 */
function Eyebrow({
  children,
  tracking = 'wide',
  color = 'faint',
  style = {},
  ...rest
}) {
  const tracks = {
    normal: '0.12em',
    wide: '0.18em',
    xwide: '0.24em'
  };
  const colors = {
    faint: 'var(--cal-text-faint)',
    muted: 'var(--cal-text-muted)',
    cyan: 'var(--cal-cyan)',
    magenta: 'var(--cal-magenta)'
  };
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: tracks[tracking] || tracks.wide,
      textTransform: 'uppercase',
      color: colors[color] || colors.faint,
      ...style
    }
  }, rest), children);
}
Object.assign(__ds_scope, { Eyebrow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Eyebrow.jsx", error: String((e && e.message) || e) }); }

// components/core/StatCard.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL StatCard — dashboard KPI tile. Gradient-filled Archivo Black numeral
 * over a mono uppercase label, with the gradient keyline on top.
 */
function StatCard({
  value,
  label,
  style = {},
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      position: 'relative',
      overflow: 'hidden',
      background: 'var(--cal-surface-2)',
      border: '1px solid var(--cal-border)',
      borderRadius: 'var(--cal-radius-lg)',
      boxShadow: 'var(--cal-shadow-md), var(--cal-edge-top)',
      padding: '24px 22px 20px',
      display: 'flex',
      flexDirection: 'column',
      gap: 6,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("div", {
    style: {
      position: 'absolute',
      top: 0,
      left: 0,
      right: 0,
      height: 2,
      background: 'var(--cal-gradient)'
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-display)',
      fontWeight: 900,
      fontSize: 44,
      lineHeight: 1,
      letterSpacing: '-0.02em',
      background: 'var(--cal-gradient)',
      WebkitBackgroundClip: 'text',
      backgroundClip: 'text',
      WebkitTextFillColor: 'transparent'
    }
  }, value), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: '0.18em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)'
    }
  }, label));
}
Object.assign(__ds_scope, { StatCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/StatCard.jsx", error: String((e && e.message) || e) }); }

// components/data/TeamFlag.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL TeamFlag — small team/user avatar. Shows a logo image when provided,
 * otherwise a gradient tile with the team's initials.
 */
function TeamFlag({
  name = '',
  logoUrl = null,
  size = 24,
  style = {},
  ...rest
}) {
  const initials = name.split(/\s+/).filter(Boolean).slice(0, 2).map(w => w[0]).join('').toUpperCase() || '?';
  return /*#__PURE__*/React.createElement("span", _extends({
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      justifyContent: 'center',
      width: size,
      height: size,
      flexShrink: 0,
      borderRadius: Math.max(4, size * 0.22),
      border: '1px solid var(--cal-border-strong)',
      background: logoUrl ? `center/cover no-repeat url(${logoUrl})` : 'var(--cal-gradient)',
      color: 'var(--cal-text-inverse)',
      fontFamily: 'var(--cal-font-display)',
      fontWeight: 800,
      fontSize: Math.round(size * 0.42),
      letterSpacing: '-0.02em',
      overflow: 'hidden',
      ...style
    },
    title: name || undefined
  }, rest), !logoUrl && initials);
}
Object.assign(__ds_scope, { TeamFlag });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/TeamFlag.jsx", error: String((e && e.message) || e) }); }

// components/forms/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * CAL Input — dark text field. Surface-1 fill, hairline border that lifts
 * to cyan on focus. Pairs with an optional mono uppercase label.
 */
function Input({
  label = null,
  hint = null,
  invalid = false,
  style = {},
  id,
  ...rest
}) {
  const fieldId = id || (label ? `cal-${String(label).replace(/\W+/g, '-').toLowerCase()}` : undefined);
  return /*#__PURE__*/React.createElement("label", {
    htmlFor: fieldId,
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 6
    }
  }, label && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)'
    }
  }, label), /*#__PURE__*/React.createElement("input", _extends({
    id: fieldId,
    style: {
      height: 38,
      padding: '0 12px',
      background: 'var(--cal-surface-1)',
      color: 'var(--cal-text)',
      border: `1px solid ${invalid ? 'var(--cal-danger)' : 'var(--cal-border-strong)'}`,
      borderRadius: 'var(--cal-radius-sm)',
      fontFamily: 'var(--cal-font-display)',
      fontSize: 14,
      outline: 'none',
      transition: 'border-color var(--cal-dur) var(--cal-ease), box-shadow var(--cal-dur) var(--cal-ease)',
      ...style
    },
    onFocus: e => {
      if (!invalid) {
        e.target.style.borderColor = 'var(--cal-cyan)';
        e.target.style.boxShadow = '0 0 0 3px var(--cal-cyan-ghost)';
      }
    },
    onBlur: e => {
      e.target.style.borderColor = invalid ? 'var(--cal-danger)' : 'var(--cal-border-strong)';
      e.target.style.boxShadow = 'none';
    }
  }, rest)), hint && /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12,
      color: invalid ? 'var(--cal-danger)' : 'var(--cal-text-faint)'
    }
  }, hint));
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Input.jsx", error: String((e && e.message) || e) }); }

// ui_kits/cal-web/Dashboard.jsx
try { (() => {
/* CAL Dashboard — the flagship Home view: stat row, season/playoff panels,
   latest news, recent activity feed. Composes DS primitives. */
function statusTone(status) {
  return {
    Active: 'cyan',
    Preseason: 'warning',
    Completed: 'neutral',
    Upcoming: 'neutral'
  }[status] || 'neutral';
}
function PanelRow({
  children,
  onClick
}) {
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClick,
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr auto auto',
      alignItems: 'center',
      gap: 10,
      padding: '10px 20px',
      borderBottom: '1px solid var(--cal-border-faint)',
      cursor: onClick ? 'pointer' : 'default',
      transition: 'background var(--cal-dur-fast) var(--cal-ease)'
    },
    onMouseEnter: e => {
      e.currentTarget.style.background = 'var(--cal-surface-3)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.background = 'transparent';
    }
  }, children);
}
const dateCell = {
  fontFamily: 'var(--cal-font-mono)',
  fontSize: 11,
  letterSpacing: '0.08em',
  color: 'var(--cal-text-faint)',
  whiteSpace: 'nowrap'
};
const dateLabel = {
  textTransform: 'uppercase',
  opacity: 0.6,
  marginRight: 3
};
const rowName = {
  fontSize: 14,
  fontWeight: 600,
  color: 'var(--cal-text)',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap'
};
function Dashboard({
  onNavigate
}) {
  const {
    Card,
    StatCard,
    Badge,
    TeamFlag
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const d = window.CALData;
  const seeAll = to => /*#__PURE__*/React.createElement("span", {
    onClick: () => onNavigate(to),
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color: 'var(--cal-cyan)',
      cursor: 'pointer'
    }
  }, "All \u2192");
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(3, 1fr)',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement(StatCard, {
    value: 2,
    label: "Active seasons"
  }), /*#__PURE__*/React.createElement(StatCard, {
    value: 1,
    label: "Active playoffs"
  }), /*#__PURE__*/React.createElement(StatCard, {
    value: 6,
    label: "Results this week"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Recent seasons",
    action: seeAll('seasons'),
    padded: false,
    elevation: "md"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, d.seasons.map(s => /*#__PURE__*/React.createElement(PanelRow, {
    key: s.id,
    onClick: () => onNavigate('season:' + s.id)
  }, /*#__PURE__*/React.createElement("span", {
    style: rowName
  }, s.name), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: statusTone(s.status)
  }, s.status), s.regOpen && /*#__PURE__*/React.createElement(Badge, {
    tone: "success"
  }, "Reg. Open")), /*#__PURE__*/React.createElement("span", {
    style: dateCell
  }, /*#__PURE__*/React.createElement("span", {
    style: dateLabel
  }, "starts"), s.starts))))), /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Recent playoffs",
    action: seeAll('playoffs'),
    padded: false,
    elevation: "md"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, d.playoffs.map(p => /*#__PURE__*/React.createElement(PanelRow, {
    key: p.id,
    onClick: () => onNavigate('playoffs')
  }, /*#__PURE__*/React.createElement("span", {
    style: rowName
  }, p.name), /*#__PURE__*/React.createElement(Badge, {
    tone: p.status === 'Active' ? 'cyan' : 'neutral'
  }, p.status), /*#__PURE__*/React.createElement("span", {
    style: dateCell
  }, /*#__PURE__*/React.createElement("span", {
    style: dateLabel
  }, "starts"), p.starts)))))), /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Latest news",
    action: seeAll('news'),
    padded: false
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, d.news.map(n => /*#__PURE__*/React.createElement("div", {
    key: n.id,
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 5,
      padding: '14px 20px',
      borderBottom: '1px solid var(--cal-border-faint)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 5,
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--cal-text-muted)'
    }
  }, n.author), /*#__PURE__*/React.createElement("span", {
    style: {
      opacity: 0.4
    }
  }, "\xB7"), /*#__PURE__*/React.createElement("span", null, n.date), /*#__PURE__*/React.createElement("span", {
    style: {
      opacity: 0.4
    }
  }, "\xB7"), /*#__PURE__*/React.createElement("span", null, n.comments, " comments")), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 15,
      fontWeight: 700,
      color: 'var(--cal-text)',
      lineHeight: 1.3
    }
  }, n.title), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      fontSize: 13,
      color: 'var(--cal-text-muted)',
      lineHeight: 1.55
    }
  }, n.excerpt))))), /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Recent activity",
    padded: false
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, d.activity.map((a, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: 'grid',
      gridTemplateColumns: '76px 180px 1fr 200px 60px',
      alignItems: 'center',
      gap: 12,
      padding: '9px 20px',
      borderBottom: '1px solid var(--cal-border-faint)'
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: a.type === 'Playoff' ? 'magenta' : 'cyan'
  }, a.type), /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 7,
      fontSize: 14,
      fontWeight: 600,
      color: 'var(--cal-text)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, /*#__PURE__*/React.createElement(TeamFlag, {
    name: a.winner,
    size: 20
  }), " ", a.winner), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 13,
      color: 'var(--cal-text-muted)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, a.matchup), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.10em',
      color: 'var(--cal-text-faint)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, a.context), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.10em',
      color: 'var(--cal-text-faint)',
      textAlign: 'right'
    }
  }, a.time))))));
}
Object.assign(window, {
  Dashboard
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/cal-web/Dashboard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/cal-web/Playoffs.jsx
try { (() => {
/* CAL Playoffs — single-elimination bracket. Columns per round, winner
   highlighted in cyan, lobby code shown on the active matchup. */
function MatchTeam({
  name,
  seed,
  isWinner,
  decided
}) {
  const {
    TeamFlag
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 9,
      padding: '9px 12px',
      background: isWinner ? 'var(--cal-cyan-ghost)' : 'transparent',
      borderLeft: isWinner ? '2px solid var(--cal-cyan)' : '2px solid transparent'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 10,
      color: 'var(--cal-text-faint)',
      width: 14
    }
  }, seed || '–'), /*#__PURE__*/React.createElement(TeamFlag, {
    name: name === 'TBD' ? '' : name,
    size: 20
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      fontSize: 13,
      fontWeight: isWinner ? 700 : 600,
      color: name === 'TBD' ? 'var(--cal-text-faint)' : isWinner || !decided ? 'var(--cal-text)' : 'var(--cal-text-muted)',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap'
    }
  }, name), isWinner && /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--cal-cyan)',
      fontSize: 12
    }
  }, "\u2713"));
}
function MatchCard({
  m
}) {
  const decided = !!m.winner;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      width: 240,
      background: 'var(--cal-surface-2)',
      border: '1px solid var(--cal-border)',
      borderRadius: 'var(--cal-radius-md)',
      overflow: 'hidden',
      boxShadow: 'var(--cal-shadow-sm), var(--cal-edge-top)'
    }
  }, /*#__PURE__*/React.createElement(MatchTeam, {
    name: m.a,
    seed: m.aSeed,
    isWinner: m.winner === 'a',
    decided: decided
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      height: 1,
      background: 'var(--cal-border-faint)'
    }
  }), /*#__PURE__*/React.createElement(MatchTeam, {
    name: m.b,
    seed: m.bSeed,
    isWinner: m.winner === 'b',
    decided: decided
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '6px 12px',
      borderTop: '1px solid var(--cal-border-faint)',
      background: 'var(--cal-surface-1)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 9,
      letterSpacing: '0.14em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)'
    }
  }, "Best of One"), m.code ? /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 10,
      letterSpacing: '0.06em',
      color: 'var(--cal-cyan)'
    }
  }, m.code) : /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 9,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      color: decided ? 'var(--cal-text-faint)' : 'var(--cal-warning)'
    }
  }, decided ? 'Final' : 'Pending')));
}
function Playoffs({
  onNavigate
}) {
  const {
    Badge,
    Eyebrow
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const {
    rounds
  } = window.CALData.bracket;
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 18
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--cal-font-heading)',
      fontSize: 32,
      fontWeight: 700,
      letterSpacing: '-0.01em',
      color: 'var(--cal-text)',
      borderLeft: '4px solid var(--cal-magenta)',
      paddingLeft: 12,
      lineHeight: 1.1,
      whiteSpace: 'nowrap'
    }
  }, "Summer Split 2026 \u2014 Playoffs"), /*#__PURE__*/React.createElement(Badge, {
    tone: "magenta"
  }, "Active")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 28,
      alignItems: 'stretch',
      overflowX: 'auto',
      paddingBottom: 8
    }
  }, rounds.map((round, ri) => /*#__PURE__*/React.createElement("div", {
    key: ri,
    style: {
      display: 'flex',
      flexDirection: 'column',
      minWidth: 240
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      marginBottom: 14
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    color: ri === rounds.length - 1 ? 'magenta' : 'faint'
  }, round.name)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'space-around',
      flex: 1,
      gap: 16
    }
  }, round.matchups.map((m, mi) => /*#__PURE__*/React.createElement(MatchCard, {
    key: mi,
    m: m
  })))))));
}
Object.assign(window, {
  Playoffs
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/cal-web/Playoffs.jsx", error: String((e && e.message) || e) }); }

// ui_kits/cal-web/Screens.jsx
try { (() => {
/* CAL secondary screens: Teams index, Seasons index, Season detail
   (standings + schedule), and News list. */

function PageTitle({
  children,
  action
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 18
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: 'var(--cal-font-heading)',
      fontSize: 32,
      fontWeight: 700,
      letterSpacing: '-0.01em',
      color: 'var(--cal-text)',
      borderLeft: '4px solid var(--cal-magenta)',
      paddingLeft: 12,
      lineHeight: 1.1,
      whiteSpace: 'nowrap'
    }
  }, children), action);
}

/* ---------- Teams ---------- */
function Teams({
  onNavigate
}) {
  const {
    Card,
    Badge,
    TeamFlag,
    Button
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const teams = [...window.CALData.teams].sort((a, b) => b.w - a.w || a.l - b.l);
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(PageTitle, {
    action: /*#__PURE__*/React.createElement(Button, {
      variant: "primary",
      size: "sm"
    }, "Create Team")
  }, "Teams"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: 'repeat(2, 1fr)',
      gap: 14
    }
  }, teams.map(t => /*#__PURE__*/React.createElement(Card, {
    key: t.id,
    elevation: "md",
    keyline: false
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement(TeamFlag, {
    name: t.name,
    size: 44
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 16,
      fontWeight: 700,
      color: 'var(--cal-text)'
    }
  }, t.name), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.16em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)',
      marginTop: 2
    }
  }, t.tag, " \xB7 5 players")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: "success"
  }, t.w, " W"), /*#__PURE__*/React.createElement(Badge, {
    tone: "danger"
  }, t.l, " L")))))));
}

/* ---------- Seasons index ---------- */
function statusTone2(status) {
  return {
    Active: 'cyan',
    Preseason: 'warning',
    Completed: 'neutral'
  }[status] || 'neutral';
}
function Seasons({
  onNavigate
}) {
  const {
    Card,
    Badge,
    Button
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(PageTitle, {
    action: /*#__PURE__*/React.createElement(Button, {
      variant: "primary",
      size: "sm"
    }, "Create Season")
  }, "Seasons"), /*#__PURE__*/React.createElement(Card, {
    padded: false
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, window.CALData.seasons.map(s => /*#__PURE__*/React.createElement("div", {
    key: s.id,
    onClick: () => onNavigate('season:' + s.id),
    style: {
      display: 'grid',
      gridTemplateColumns: '1fr auto auto auto',
      alignItems: 'center',
      gap: 12,
      padding: '14px 20px',
      borderBottom: '1px solid var(--cal-border-faint)',
      cursor: 'pointer',
      transition: 'background var(--cal-dur-fast) var(--cal-ease)'
    },
    onMouseEnter: e => {
      e.currentTarget.style.background = 'var(--cal-surface-3)';
    },
    onMouseLeave: e => {
      e.currentTarget.style.background = 'transparent';
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 15,
      fontWeight: 700,
      color: 'var(--cal-text)'
    }
  }, s.name), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.1em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)',
      marginTop: 3
    }
  }, s.teams, " teams \xB7 ", s.weeks, " weeks")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: statusTone2(s.status)
  }, s.status), s.regOpen && /*#__PURE__*/React.createElement(Badge, {
    tone: "success"
  }, "Reg. Open")), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      color: 'var(--cal-text-faint)',
      whiteSpace: 'nowrap'
    }
  }, s.starts), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--cal-cyan)',
      fontSize: 18
    }
  }, "\u2192"))))));
}

/* ---------- Season detail (standings + schedule) ---------- */
function SeasonDetail({
  seasonId,
  onNavigate
}) {
  const {
    Card,
    Badge,
    TeamFlag,
    Eyebrow
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const s = window.CALData.seasons.find(x => x.id === seasonId) || window.CALData.seasons[0];
  const standings = [...window.CALData.teams].sort((a, b) => b.w - a.w || a.l - b.l);
  const th = {
    fontFamily: 'var(--cal-font-mono)',
    fontSize: 10,
    fontWeight: 600,
    letterSpacing: '0.16em',
    textTransform: 'uppercase',
    color: 'var(--cal-text-faint)',
    textAlign: 'left',
    padding: '0 0 10px'
  };
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      marginBottom: 6
    }
  }, /*#__PURE__*/React.createElement("span", {
    onClick: () => onNavigate('seasons'),
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      letterSpacing: '0.12em',
      textTransform: 'uppercase',
      color: 'var(--cal-text-faint)',
      cursor: 'pointer'
    }
  }, "\u2190 Seasons")), /*#__PURE__*/React.createElement(PageTitle, {
    action: /*#__PURE__*/React.createElement("div", {
      style: {
        display: 'flex',
        gap: 6
      }
    }, /*#__PURE__*/React.createElement(Badge, {
      tone: statusTone2(s.status)
    }, s.status))
  }, s.name), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'grid',
      gridTemplateColumns: '1.4fr 1fr',
      gap: 14
    }
  }, /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Team standings"
  }, /*#__PURE__*/React.createElement("table", {
    style: {
      width: '100%',
      borderCollapse: 'collapse',
      marginTop: 4
    }
  }, /*#__PURE__*/React.createElement("thead", null, /*#__PURE__*/React.createElement("tr", null, /*#__PURE__*/React.createElement("th", {
    style: {
      ...th,
      width: 36
    }
  }, "#"), /*#__PURE__*/React.createElement("th", {
    style: th
  }, "Team"), /*#__PURE__*/React.createElement("th", {
    style: {
      ...th,
      textAlign: 'center',
      width: 44
    }
  }, "W"), /*#__PURE__*/React.createElement("th", {
    style: {
      ...th,
      textAlign: 'center',
      width: 44
    }
  }, "L"))), /*#__PURE__*/React.createElement("tbody", null, standings.map((t, i) => /*#__PURE__*/React.createElement("tr", {
    key: t.id,
    style: {
      borderTop: '1px solid var(--cal-border-faint)'
    }
  }, /*#__PURE__*/React.createElement("td", {
    style: {
      padding: '11px 0',
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 13,
      color: i < 4 ? 'var(--cal-cyan)' : 'var(--cal-text-faint)',
      fontWeight: 600
    }
  }, i + 1), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: '11px 0'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 9,
      fontSize: 14,
      fontWeight: 600,
      color: 'var(--cal-text)'
    }
  }, /*#__PURE__*/React.createElement(TeamFlag, {
    name: t.name,
    size: 22
  }), " ", t.name)), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: '11px 0',
      textAlign: 'center',
      fontFamily: 'var(--cal-font-mono)',
      fontWeight: 600,
      color: 'var(--cal-success)'
    }
  }, t.w), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: '11px 0',
      textAlign: 'center',
      fontFamily: 'var(--cal-font-mono)',
      fontWeight: 600,
      color: 'var(--cal-text-faint)'
    }
  }, t.l))))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 12,
      paddingTop: 12,
      borderTop: '1px solid var(--cal-border-faint)'
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    color: "cyan",
    tracking: "normal"
  }, "Top 4 qualify for playoffs"))), /*#__PURE__*/React.createElement(Card, {
    eyebrow: "Week 6 schedule",
    padded: false,
    elevation: "md"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      padding: '8px 0 4px'
    }
  }, [['Void Reavers', 'Shadow Isles', 'a'], ['Piltover Pulse', 'Demacia Wardens', 'a'], ['Noxian Vanguard', 'Bilgewater Buccaneers', 'a'], ['Ionian Wind', 'Freljord Frost', 'b']].map((m, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 10,
      padding: '12px 20px',
      borderBottom: '1px solid var(--cal-border-faint)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      display: 'flex',
      alignItems: 'center',
      gap: 8,
      fontSize: 13,
      fontWeight: m[2] === 'a' ? 700 : 500,
      color: m[2] === 'a' ? 'var(--cal-text)' : 'var(--cal-text-muted)'
    }
  }, /*#__PURE__*/React.createElement(TeamFlag, {
    name: m[0],
    size: 18
  }), m[0]), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 10,
      letterSpacing: '0.1em',
      color: 'var(--cal-text-faint)'
    }
  }, "VS"), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1,
      display: 'flex',
      alignItems: 'center',
      gap: 8,
      justifyContent: 'flex-end',
      fontSize: 13,
      fontWeight: m[2] === 'b' ? 700 : 500,
      color: m[2] === 'b' ? 'var(--cal-text)' : 'var(--cal-text-muted)'
    }
  }, m[1], /*#__PURE__*/React.createElement(TeamFlag, {
    name: m[1],
    size: 18
  }))))))));
}

/* ---------- News ---------- */
function News() {
  const {
    Eyebrow
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement(PageTitle, null, "News"), /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 680
    }
  }, window.CALData.news.map((n, i) => /*#__PURE__*/React.createElement("article", {
    key: n.id,
    style: {
      display: 'flex',
      flexDirection: 'column',
      gap: 8,
      padding: i === 0 ? '0 0 28px' : '28px 0',
      borderBottom: '1px solid var(--cal-border)'
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 22,
      fontWeight: 700,
      letterSpacing: '-0.01em',
      color: 'var(--cal-text)',
      lineHeight: 1.25
    }
  }, n.title), /*#__PURE__*/React.createElement("div", {
    style: {
      display: 'flex',
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Eyebrow, {
    tracking: "normal"
  }, n.date), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--cal-text-faint)'
    }
  }, "\xB7"), /*#__PURE__*/React.createElement(Eyebrow, {
    tracking: "normal",
    color: "muted"
  }, n.author), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'var(--cal-text-faint)'
    }
  }, "\xB7"), /*#__PURE__*/React.createElement(Eyebrow, {
    tracking: "normal"
  }, n.comments, " comments")), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      color: 'var(--cal-text-muted)',
      lineHeight: 1.6,
      fontSize: 14
    }
  }, n.excerpt), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: 'var(--cal-font-mono)',
      fontSize: 11,
      fontWeight: 600,
      letterSpacing: '0.1em',
      textTransform: 'uppercase',
      color: 'var(--cal-cyan)',
      cursor: 'pointer'
    }
  }, "Read more \u2192")))));
}
Object.assign(window, {
  Teams,
  Seasons,
  SeasonDetail,
  News
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/cal-web/Screens.jsx", error: String((e && e.message) || e) }); }

// ui_kits/cal-web/TopNav.jsx
try { (() => {
/* CAL TopNav — sticky dark nav with neon masthead + Rajdhani uppercase links. */
function TopNav({
  active,
  onNavigate
}) {
  const {
    TeamFlag
  } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const links = [['news', 'News'], ['format', 'Format'], ['rules', 'Rules'], '|', ['teams', 'Teams'], ['users', 'Users'], '|', ['seasons', 'Seasons'], ['playoffs', 'Playoffs']];
  const linkStyle = isActive => ({
    fontFamily: 'var(--cal-font-heading)',
    fontWeight: 600,
    fontSize: 14,
    letterSpacing: '0.04em',
    textTransform: 'uppercase',
    color: isActive ? 'var(--cal-cyan)' : 'rgba(255,255,255,0.75)',
    background: isActive ? 'var(--cal-cyan-ghost)' : 'transparent',
    padding: '6px 12px',
    borderRadius: 4,
    cursor: 'pointer',
    whiteSpace: 'nowrap',
    transition: 'color var(--cal-dur) var(--cal-ease), background var(--cal-dur) var(--cal-ease)'
  });
  return /*#__PURE__*/React.createElement("header", {
    style: {
      position: 'sticky',
      top: 0,
      zIndex: 1000,
      display: 'flex',
      alignItems: 'center',
      gap: 16,
      height: '3.5rem',
      padding: '0 24px',
      background: '#2a2a2a',
      boxShadow: '0 6px 16px rgba(0,0,0,0.4)'
    }
  }, /*#__PURE__*/React.createElement("div", {
    onClick: () => onNavigate('dashboard'),
    style: {
      display: 'flex',
      alignItems: 'center',
      cursor: 'pointer',
      flexShrink: 0
    },
    title: "Champions Amateur League"
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo/CAL-masthead-full.png",
    alt: "Champions Amateur League",
    style: {
      height: 28,
      width: 'auto'
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1,
      height: 20,
      background: 'rgba(255,255,255,0.25)'
    }
  }), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: 'flex',
      alignItems: 'center',
      gap: 2
    }
  }, links.map((l, i) => l === '|' ? /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      width: 1,
      height: 20,
      background: 'rgba(255,255,255,0.25)',
      margin: '0 6px'
    }
  }) : /*#__PURE__*/React.createElement("span", {
    key: i,
    style: linkStyle(active === l[0]),
    onClick: () => onNavigate(l[0]),
    onMouseEnter: e => {
      if (active !== l[0]) {
        e.currentTarget.style.color = 'var(--cal-cyan)';
        e.currentTarget.style.background = 'rgba(0,229,255,0.08)';
      }
    },
    onMouseLeave: e => {
      if (active !== l[0]) {
        e.currentTarget.style.color = 'rgba(255,255,255,0.75)';
        e.currentTarget.style.background = 'transparent';
      }
    }
  }, l[1]))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: 'auto',
      display: 'flex',
      alignItems: 'center',
      gap: 8
    }
  }, /*#__PURE__*/React.createElement(TeamFlag, {
    name: "Ryan Cole",
    size: 22
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      color: 'rgba(255,255,255,0.85)',
      fontSize: 14,
      fontWeight: 500
    }
  }, "Ryan Cole")));
}
Object.assign(window, {
  TopNav
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/cal-web/TopNav.jsx", error: String((e && e.message) || e) }); }

// ui_kits/cal-web/data.js
try { (() => {
/* Champions Amateur League — UI kit mock data (fictional, LoL-flavored). */
window.CALData = function () {
  const teams = [{
    id: 1,
    name: 'Void Reavers',
    tag: 'VOID',
    w: 5,
    l: 1
  }, {
    id: 2,
    name: 'Shadow Isles',
    tag: 'SHDW',
    w: 5,
    l: 1
  }, {
    id: 3,
    name: 'Piltover Pulse',
    tag: 'PLT',
    w: 4,
    l: 2
  }, {
    id: 4,
    name: 'Noxian Vanguard',
    tag: 'NOX',
    w: 4,
    l: 2
  }, {
    id: 5,
    name: 'Demacia Wardens',
    tag: 'DEM',
    w: 3,
    l: 3
  }, {
    id: 6,
    name: 'Bilgewater Buccaneers',
    tag: 'BILG',
    w: 2,
    l: 4
  }, {
    id: 7,
    name: 'Ionian Wind',
    tag: 'ION',
    w: 2,
    l: 4
  }, {
    id: 8,
    name: 'Freljord Frost',
    tag: 'FRLJ',
    w: 1,
    l: 5
  }];
  const seasons = [{
    id: 12,
    name: 'Summer Split 2026',
    status: 'Active',
    regOpen: false,
    starts: 'May 2, 2026',
    ends: 'Jun 13, 2026',
    teams: 8,
    weeks: 6
  }, {
    id: 13,
    name: 'Clash Cup — June',
    status: 'Preseason',
    regOpen: true,
    starts: 'Jun 20, 2026',
    ends: 'Jul 4, 2026',
    teams: 5,
    weeks: 3
  }, {
    id: 11,
    name: 'Spring Split 2026',
    status: 'Completed',
    regOpen: false,
    starts: 'Feb 7, 2026',
    ends: 'Mar 28, 2026',
    teams: 8,
    weeks: 6
  }];
  const playoffs = [{
    id: 7,
    name: 'Summer Split 2026 — Playoffs',
    status: 'Active',
    starts: 'Jun 6, 2026',
    ends: 'Jun 13, 2026',
    season: 'Summer Split 2026'
  }, {
    id: 6,
    name: 'Spring Split 2026 — Playoffs',
    status: 'Completed',
    starts: 'Mar 21, 2026',
    ends: 'Mar 28, 2026',
    season: 'Spring Split 2026'
  }];
  const news = [{
    id: 31,
    title: 'Summer Split playoffs are set — top 8 lock in their seeds',
    author: 'Ryan Cole',
    date: 'Jun 6, 2026',
    comments: 7,
    excerpt: 'With Week 6 in the books, the bracket is final. Void Reavers and Shadow Isles enter as co-favorites at 5–1, while a three-way tie for the final seed came down to head-to-head record.'
  }, {
    id: 30,
    title: 'Rule clarification: best-of-one tiebreakers (4.10)',
    author: 'Ryan Cole',
    date: 'May 24, 2026',
    comments: 2,
    excerpt: 'Several captains asked how seeding ties are broken. Section 4.10 now spells out the order: head-to-head result first, then total game time, then a coin flip administered by an admin.'
  }, {
    id: 29,
    title: 'Clash Cup registration is open — 5 spots, single weekend',
    author: 'Ryan Cole',
    date: 'May 18, 2026',
    comments: 4,
    excerpt: 'A short-format side event between splits. Three weeks of regular play, then a four-team bracket. Registration closes when the fifth team locks in.'
  }];
  const activity = [{
    type: 'Playoff',
    winner: 'Void Reavers',
    matchup: 'Quarterfinal · VOID vs FRLJ',
    context: 'Summer Split 2026 — Playoffs',
    time: '2h ago'
  }, {
    type: 'Playoff',
    winner: 'Shadow Isles',
    matchup: 'Quarterfinal · SHDW vs ION',
    context: 'Summer Split 2026 — Playoffs',
    time: '3h ago'
  }, {
    type: 'Regular',
    winner: 'Piltover Pulse',
    matchup: 'Week 6 · PLT vs DEM',
    context: 'Summer Split 2026',
    time: '1d ago'
  }, {
    type: 'Regular',
    winner: 'Noxian Vanguard',
    matchup: 'Week 6 · NOX vs BILG',
    context: 'Summer Split 2026',
    time: '1d ago'
  }, {
    type: 'Playoff',
    winner: 'Piltover Pulse',
    matchup: 'Quarterfinal · PLT vs DEM',
    context: 'Summer Split 2026 — Playoffs',
    time: '2d ago'
  }, {
    type: 'Regular',
    winner: 'Void Reavers',
    matchup: 'Week 5 · VOID vs SHDW',
    context: 'Summer Split 2026',
    time: '3d ago'
  }];

  // Single-elimination, 8-team bracket. seed maps to team by standings order.
  const bracket = {
    rounds: [{
      name: 'Quarterfinals',
      matchups: [{
        a: 'Void Reavers',
        aSeed: 1,
        b: 'Freljord Frost',
        bSeed: 8,
        winner: 'a',
        code: '4F2A-9KQ7'
      }, {
        a: 'Noxian Vanguard',
        aSeed: 4,
        b: 'Demacia Wardens',
        bSeed: 5,
        winner: 'b',
        code: '8H1C-2RM5'
      }, {
        a: 'Piltover Pulse',
        aSeed: 3,
        b: 'Bilgewater Buccaneers',
        bSeed: 6,
        winner: 'a',
        code: 'K0PZ-7T3W'
      }, {
        a: 'Shadow Isles',
        aSeed: 2,
        b: 'Ionian Wind',
        bSeed: 7,
        winner: 'a',
        code: 'QX44-9LB2'
      }]
    }, {
      name: 'Semifinals',
      matchups: [{
        a: 'Void Reavers',
        aSeed: 1,
        b: 'Demacia Wardens',
        bSeed: 5,
        winner: null,
        code: null
      }, {
        a: 'Piltover Pulse',
        aSeed: 3,
        b: 'Shadow Isles',
        bSeed: 2,
        winner: null,
        code: null
      }]
    }, {
      name: 'Grand Final',
      matchups: [{
        a: 'TBD',
        aSeed: null,
        b: 'TBD',
        bSeed: null,
        winner: null,
        code: null
      }]
    }]
  };
  return {
    teams,
    seasons,
    playoffs,
    news,
    activity,
    bracket
  };
}();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/cal-web/data.js", error: String((e && e.message) || e) }); }

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.Eyebrow = __ds_scope.Eyebrow;

__ds_ns.StatCard = __ds_scope.StatCard;

__ds_ns.TeamFlag = __ds_scope.TeamFlag;

__ds_ns.Input = __ds_scope.Input;

})();
