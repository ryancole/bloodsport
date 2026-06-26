/* CAL Dashboard — the flagship Home view: stat row, season/playoff panels,
   latest news, recent activity feed. Composes DS primitives. */
function statusTone(status) {
  return { Active: 'cyan', Preseason: 'warning', Completed: 'neutral', Upcoming: 'neutral' }[status] || 'neutral';
}

function PanelRow({ children, onClick }) {
  return (
    <div
      onClick={onClick}
      style={{
        display: 'grid', gridTemplateColumns: '1fr auto auto', alignItems: 'center', gap: 10,
        padding: '10px 20px', borderBottom: '1px solid var(--cal-border-faint)', cursor: onClick ? 'pointer' : 'default',
        transition: 'background var(--cal-dur-fast) var(--cal-ease)',
      }}
      onMouseEnter={(e) => { e.currentTarget.style.background = 'var(--cal-surface-3)'; }}
      onMouseLeave={(e) => { e.currentTarget.style.background = 'transparent'; }}
    >{children}</div>
  );
}

const dateCell = { fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.08em', color: 'var(--cal-text-faint)', whiteSpace: 'nowrap' };
const dateLabel = { textTransform: 'uppercase', opacity: 0.6, marginRight: 3 };
const rowName = { fontSize: 14, fontWeight: 600, color: 'var(--cal-text)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' };

function Dashboard({ onNavigate }) {
  const { Card, StatCard, Badge, TeamFlag } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const d = window.CALData;
  const seeAll = (to) => (
    <span onClick={() => onNavigate(to)} style={{
      fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600, letterSpacing: '0.14em',
      textTransform: 'uppercase', color: 'var(--cal-cyan)', cursor: 'pointer',
    }}>All →</span>
  );

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
      {/* stat row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 14 }}>
        <StatCard value={2} label="Active seasons" />
        <StatCard value={1} label="Active playoffs" />
        <StatCard value={6} label="Results this week" />
      </div>

      {/* seasons + playoffs panels */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14 }}>
        <Card eyebrow="Recent seasons" action={seeAll('seasons')} padded={false} elevation="md">
          <div style={{ padding: '8px 0 4px' }}>
            {d.seasons.map((s) => (
              <PanelRow key={s.id} onClick={() => onNavigate('season:' + s.id)}>
                <span style={rowName}>{s.name}</span>
                <div style={{ display: 'flex', gap: 6 }}>
                  <Badge tone={statusTone(s.status)}>{s.status}</Badge>
                  {s.regOpen && <Badge tone="success">Reg. Open</Badge>}
                </div>
                <span style={dateCell}><span style={dateLabel}>starts</span>{s.starts}</span>
              </PanelRow>
            ))}
          </div>
        </Card>

        <Card eyebrow="Recent playoffs" action={seeAll('playoffs')} padded={false} elevation="md">
          <div style={{ padding: '8px 0 4px' }}>
            {d.playoffs.map((p) => (
              <PanelRow key={p.id} onClick={() => onNavigate('playoffs')}>
                <span style={rowName}>{p.name}</span>
                <Badge tone={p.status === 'Active' ? 'cyan' : 'neutral'}>{p.status}</Badge>
                <span style={dateCell}><span style={dateLabel}>starts</span>{p.starts}</span>
              </PanelRow>
            ))}
          </div>
        </Card>
      </div>

      {/* news */}
      <Card eyebrow="Latest news" action={seeAll('news')} padded={false}>
        <div style={{ padding: '8px 0 4px' }}>
          {d.news.map((n) => (
            <div key={n.id} style={{ display: 'flex', flexDirection: 'column', gap: 5, padding: '14px 20px', borderBottom: '1px solid var(--cal-border-faint)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 5, fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: 'var(--cal-text-faint)' }}>
                <span style={{ color: 'var(--cal-text-muted)' }}>{n.author}</span>
                <span style={{ opacity: 0.4 }}>·</span><span>{n.date}</span>
                <span style={{ opacity: 0.4 }}>·</span><span>{n.comments} comments</span>
              </div>
              <span style={{ fontSize: 15, fontWeight: 700, color: 'var(--cal-text)', lineHeight: 1.3 }}>{n.title}</span>
              <p style={{ margin: 0, fontSize: 13, color: 'var(--cal-text-muted)', lineHeight: 1.55 }}>{n.excerpt}</p>
            </div>
          ))}
        </div>
      </Card>

      {/* activity feed */}
      <Card eyebrow="Recent activity" padded={false}>
        <div style={{ padding: '8px 0 4px' }}>
          {d.activity.map((a, i) => (
            <div key={i} style={{ display: 'grid', gridTemplateColumns: '76px 180px 1fr 200px 60px', alignItems: 'center', gap: 12, padding: '9px 20px', borderBottom: '1px solid var(--cal-border-faint)' }}>
              <Badge tone={a.type === 'Playoff' ? 'magenta' : 'cyan'}>{a.type}</Badge>
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 7, fontSize: 14, fontWeight: 600, color: 'var(--cal-text)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                <TeamFlag name={a.winner} size={20} /> {a.winner}
              </span>
              <span style={{ fontSize: 13, color: 'var(--cal-text-muted)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{a.matchup}</span>
              <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.10em', color: 'var(--cal-text-faint)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{a.context}</span>
              <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.10em', color: 'var(--cal-text-faint)', textAlign: 'right' }}>{a.time}</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}

Object.assign(window, { Dashboard });
