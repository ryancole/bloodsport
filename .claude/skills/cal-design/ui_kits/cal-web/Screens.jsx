/* CAL secondary screens: Teams index, Seasons index, Season detail
   (standings + schedule), and News list. */

function PageTitle({ children, action }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 18 }}>
      <h1 style={{ margin: 0, fontFamily: 'var(--cal-font-heading)', fontSize: 32, fontWeight: 700, letterSpacing: '-0.01em', color: 'var(--cal-text)', borderLeft: '4px solid var(--cal-magenta)', paddingLeft: 12, lineHeight: 1.1, whiteSpace: 'nowrap' }}>{children}</h1>
      {action}
    </div>
  );
}

/* ---------- Teams ---------- */
function Teams({ onNavigate }) {
  const { Card, Badge, TeamFlag, Button } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const teams = [...window.CALData.teams].sort((a, b) => b.w - a.w || a.l - b.l);
  return (
    <div>
      <PageTitle action={<Button variant="primary" size="sm">Create Team</Button>}>Teams</PageTitle>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 14 }}>
        {teams.map((t) => (
          <Card key={t.id} elevation="md" keyline={false}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
              <TeamFlag name={t.name} size={44} />
              <div style={{ minWidth: 0, flex: 1 }}>
                <div style={{ fontSize: 16, fontWeight: 700, color: 'var(--cal-text)' }}>{t.name}</div>
                <div style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.16em', textTransform: 'uppercase', color: 'var(--cal-text-faint)', marginTop: 2 }}>{t.tag} · 5 players</div>
              </div>
              <div style={{ display: 'flex', gap: 6 }}>
                <Badge tone="success">{t.w} W</Badge>
                <Badge tone="danger">{t.l} L</Badge>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}

/* ---------- Seasons index ---------- */
function statusTone2(status) {
  return { Active: 'cyan', Preseason: 'warning', Completed: 'neutral' }[status] || 'neutral';
}
function Seasons({ onNavigate }) {
  const { Card, Badge, Button } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return (
    <div>
      <PageTitle action={<Button variant="primary" size="sm">Create Season</Button>}>Seasons</PageTitle>
      <Card padded={false}>
        <div style={{ padding: '8px 0 4px' }}>
          {window.CALData.seasons.map((s) => (
            <div key={s.id} onClick={() => onNavigate('season:' + s.id)}
              style={{ display: 'grid', gridTemplateColumns: '1fr auto auto auto', alignItems: 'center', gap: 12, padding: '14px 20px', borderBottom: '1px solid var(--cal-border-faint)', cursor: 'pointer', transition: 'background var(--cal-dur-fast) var(--cal-ease)' }}
              onMouseEnter={(e) => { e.currentTarget.style.background = 'var(--cal-surface-3)'; }}
              onMouseLeave={(e) => { e.currentTarget.style.background = 'transparent'; }}>
              <div>
                <div style={{ fontSize: 15, fontWeight: 700, color: 'var(--cal-text)' }}>{s.name}</div>
                <div style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--cal-text-faint)', marginTop: 3 }}>{s.teams} teams · {s.weeks} weeks</div>
              </div>
              <div style={{ display: 'flex', gap: 6 }}>
                <Badge tone={statusTone2(s.status)}>{s.status}</Badge>
                {s.regOpen && <Badge tone="success">Reg. Open</Badge>}
              </div>
              <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, color: 'var(--cal-text-faint)', whiteSpace: 'nowrap' }}>{s.starts}</span>
              <span style={{ color: 'var(--cal-cyan)', fontSize: 18 }}>→</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}

/* ---------- Season detail (standings + schedule) ---------- */
function SeasonDetail({ seasonId, onNavigate }) {
  const { Card, Badge, TeamFlag, Eyebrow } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const s = window.CALData.seasons.find((x) => x.id === seasonId) || window.CALData.seasons[0];
  const standings = [...window.CALData.teams].sort((a, b) => b.w - a.w || a.l - b.l);

  const th = { fontFamily: 'var(--cal-font-mono)', fontSize: 10, fontWeight: 600, letterSpacing: '0.16em', textTransform: 'uppercase', color: 'var(--cal-text-faint)', textAlign: 'left', padding: '0 0 10px' };

  return (
    <div>
      <div style={{ marginBottom: 6 }}>
        <span onClick={() => onNavigate('seasons')} style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, letterSpacing: '0.12em', textTransform: 'uppercase', color: 'var(--cal-text-faint)', cursor: 'pointer' }}>← Seasons</span>
      </div>
      <PageTitle action={<div style={{ display: 'flex', gap: 6 }}><Badge tone={statusTone2(s.status)}>{s.status}</Badge></div>}>{s.name}</PageTitle>

      <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr', gap: 14 }}>
        <Card eyebrow="Team standings">
          <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 4 }}>
            <thead><tr>
              <th style={{ ...th, width: 36 }}>#</th>
              <th style={th}>Team</th>
              <th style={{ ...th, textAlign: 'center', width: 44 }}>W</th>
              <th style={{ ...th, textAlign: 'center', width: 44 }}>L</th>
            </tr></thead>
            <tbody>
              {standings.map((t, i) => (
                <tr key={t.id} style={{ borderTop: '1px solid var(--cal-border-faint)' }}>
                  <td style={{ padding: '11px 0', fontFamily: 'var(--cal-font-mono)', fontSize: 13, color: i < 4 ? 'var(--cal-cyan)' : 'var(--cal-text-faint)', fontWeight: 600 }}>{i + 1}</td>
                  <td style={{ padding: '11px 0' }}>
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 9, fontSize: 14, fontWeight: 600, color: 'var(--cal-text)' }}><TeamFlag name={t.name} size={22} /> {t.name}</span>
                  </td>
                  <td style={{ padding: '11px 0', textAlign: 'center', fontFamily: 'var(--cal-font-mono)', fontWeight: 600, color: 'var(--cal-success)' }}>{t.w}</td>
                  <td style={{ padding: '11px 0', textAlign: 'center', fontFamily: 'var(--cal-font-mono)', fontWeight: 600, color: 'var(--cal-text-faint)' }}>{t.l}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--cal-border-faint)' }}>
            <Eyebrow color="cyan" tracking="normal">Top 4 qualify for playoffs</Eyebrow>
          </div>
        </Card>

        <Card eyebrow="Week 6 schedule" padded={false} elevation="md">
          <div style={{ padding: '8px 0 4px' }}>
            {[['Void Reavers', 'Shadow Isles', 'a'], ['Piltover Pulse', 'Demacia Wardens', 'a'], ['Noxian Vanguard', 'Bilgewater Buccaneers', 'a'], ['Ionian Wind', 'Freljord Frost', 'b']].map((m, i) => (
              <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '12px 20px', borderBottom: '1px solid var(--cal-border-faint)' }}>
                <span style={{ flex: 1, display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, fontWeight: m[2] === 'a' ? 700 : 500, color: m[2] === 'a' ? 'var(--cal-text)' : 'var(--cal-text-muted)' }}><TeamFlag name={m[0]} size={18} />{m[0]}</span>
                <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 10, letterSpacing: '0.1em', color: 'var(--cal-text-faint)' }}>VS</span>
                <span style={{ flex: 1, display: 'flex', alignItems: 'center', gap: 8, justifyContent: 'flex-end', fontSize: 13, fontWeight: m[2] === 'b' ? 700 : 500, color: m[2] === 'b' ? 'var(--cal-text)' : 'var(--cal-text-muted)' }}>{m[1]}<TeamFlag name={m[1]} size={18} /></span>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
}

/* ---------- News ---------- */
function News() {
  const { Eyebrow } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return (
    <div>
      <PageTitle>News</PageTitle>
      <div style={{ maxWidth: 680 }}>
        {window.CALData.news.map((n, i) => (
          <article key={n.id} style={{ display: 'flex', flexDirection: 'column', gap: 8, padding: i === 0 ? '0 0 28px' : '28px 0', borderBottom: '1px solid var(--cal-border)' }}>
            <span style={{ fontSize: 22, fontWeight: 700, letterSpacing: '-0.01em', color: 'var(--cal-text)', lineHeight: 1.25 }}>{n.title}</span>
            <div style={{ display: 'flex', gap: 6 }}>
              <Eyebrow tracking="normal">{n.date}</Eyebrow><span style={{ color: 'var(--cal-text-faint)' }}>·</span>
              <Eyebrow tracking="normal" color="muted">{n.author}</Eyebrow><span style={{ color: 'var(--cal-text-faint)' }}>·</span>
              <Eyebrow tracking="normal">{n.comments} comments</Eyebrow>
            </div>
            <p style={{ margin: 0, color: 'var(--cal-text-muted)', lineHeight: 1.6, fontSize: 14 }}>{n.excerpt}</p>
            <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 11, fontWeight: 600, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--cal-cyan)', cursor: 'pointer' }}>Read more →</span>
          </article>
        ))}
      </div>
    </div>
  );
}

Object.assign(window, { Teams, Seasons, SeasonDetail, News });
