/* CAL TopNav — sticky dark nav with neon masthead + Rajdhani uppercase links. */
function TopNav({ active, onNavigate }) {
  const { TeamFlag } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const links = [
    ['news', 'News'], ['format', 'Format'], ['rules', 'Rules'], '|',
    ['teams', 'Teams'], ['users', 'Users'], '|',
    ['seasons', 'Seasons'], ['playoffs', 'Playoffs'],
  ];

  const linkStyle = (isActive) => ({
    fontFamily: 'var(--cal-font-heading)', fontWeight: 600, fontSize: 14,
    letterSpacing: '0.04em', textTransform: 'uppercase',
    color: isActive ? 'var(--cal-cyan)' : 'rgba(255,255,255,0.75)',
    background: isActive ? 'var(--cal-cyan-ghost)' : 'transparent',
    padding: '6px 12px', borderRadius: 4, cursor: 'pointer', whiteSpace: 'nowrap',
    transition: 'color var(--cal-dur) var(--cal-ease), background var(--cal-dur) var(--cal-ease)',
  });

  return (
    <header style={{
      position: 'sticky', top: 0, zIndex: 1000,
      display: 'flex', alignItems: 'center', gap: 16, height: '3.5rem', padding: '0 24px',
      background: '#2a2a2a', boxShadow: '0 6px 16px rgba(0,0,0,0.4)',
    }}>
      <div onClick={() => onNavigate('dashboard')} style={{ display: 'flex', alignItems: 'center', cursor: 'pointer', flexShrink: 0 }} title="Champions Amateur League">
        <img src="../../assets/logo/CAL-masthead-full.png" alt="Champions Amateur League" style={{ height: 28, width: 'auto' }} />
      </div>
      <div style={{ width: 1, height: 20, background: 'rgba(255,255,255,0.25)' }} />
      <nav style={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        {links.map((l, i) =>
          l === '|'
            ? <div key={i} style={{ width: 1, height: 20, background: 'rgba(255,255,255,0.25)', margin: '0 6px' }} />
            : (
              <span key={i}
                style={linkStyle(active === l[0])}
                onClick={() => onNavigate(l[0])}
                onMouseEnter={(e) => { if (active !== l[0]) { e.currentTarget.style.color = 'var(--cal-cyan)'; e.currentTarget.style.background = 'rgba(0,229,255,0.08)'; } }}
                onMouseLeave={(e) => { if (active !== l[0]) { e.currentTarget.style.color = 'rgba(255,255,255,0.75)'; e.currentTarget.style.background = 'transparent'; } }}
              >{l[1]}</span>
            )
        )}
      </nav>
      <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 8 }}>
        <TeamFlag name="Ryan Cole" size={22} />
        <span style={{ color: 'rgba(255,255,255,0.85)', fontSize: 14, fontWeight: 500 }}>Ryan Cole</span>
      </div>
    </header>
  );
}

Object.assign(window, { TopNav });
