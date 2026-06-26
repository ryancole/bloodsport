/* CAL Playoffs — single-elimination bracket. Columns per round, winner
   highlighted in cyan, lobby code shown on the active matchup. */
function MatchTeam({ name, seed, isWinner, decided }) {
  const { TeamFlag } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 9, padding: '9px 12px',
      background: isWinner ? 'var(--cal-cyan-ghost)' : 'transparent',
      borderLeft: isWinner ? '2px solid var(--cal-cyan)' : '2px solid transparent',
    }}>
      <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 10, color: 'var(--cal-text-faint)', width: 14 }}>{seed || '–'}</span>
      <TeamFlag name={name === 'TBD' ? '' : name} size={20} />
      <span style={{ flex: 1, fontSize: 13, fontWeight: isWinner ? 700 : 600,
        color: name === 'TBD' ? 'var(--cal-text-faint)' : (isWinner || !decided ? 'var(--cal-text)' : 'var(--cal-text-muted)'),
        overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{name}</span>
      {isWinner && <span style={{ color: 'var(--cal-cyan)', fontSize: 12 }}>✓</span>}
    </div>
  );
}

function MatchCard({ m }) {
  const decided = !!m.winner;
  return (
    <div style={{
      width: 240, background: 'var(--cal-surface-2)', border: '1px solid var(--cal-border)',
      borderRadius: 'var(--cal-radius-md)', overflow: 'hidden', boxShadow: 'var(--cal-shadow-sm), var(--cal-edge-top)',
    }}>
      <MatchTeam name={m.a} seed={m.aSeed} isWinner={m.winner === 'a'} decided={decided} />
      <div style={{ height: 1, background: 'var(--cal-border-faint)' }} />
      <MatchTeam name={m.b} seed={m.bSeed} isWinner={m.winner === 'b'} decided={decided} />
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '6px 12px', borderTop: '1px solid var(--cal-border-faint)', background: 'var(--cal-surface-1)' }}>
        <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 9, letterSpacing: '0.14em', textTransform: 'uppercase', color: 'var(--cal-text-faint)' }}>Best of One</span>
        {m.code
          ? <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 10, letterSpacing: '0.06em', color: 'var(--cal-cyan)' }}>{m.code}</span>
          : <span style={{ fontFamily: 'var(--cal-font-mono)', fontSize: 9, letterSpacing: '0.12em', textTransform: 'uppercase', color: decided ? 'var(--cal-text-faint)' : 'var(--cal-warning)' }}>{decided ? 'Final' : 'Pending'}</span>}
      </div>
    </div>
  );
}

function Playoffs({ onNavigate }) {
  const { Badge, Eyebrow } = window.ChampionsAmateurLeagueDesignSystem_df3b47;
  const { rounds } = window.CALData.bracket;
  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 18 }}>
        <h1 style={{ margin: 0, fontFamily: 'var(--cal-font-heading)', fontSize: 32, fontWeight: 700, letterSpacing: '-0.01em', color: 'var(--cal-text)', borderLeft: '4px solid var(--cal-magenta)', paddingLeft: 12, lineHeight: 1.1, whiteSpace: 'nowrap' }}>Summer Split 2026 — Playoffs</h1>
        <Badge tone="magenta">Active</Badge>
      </div>

      <div style={{ display: 'flex', gap: 28, alignItems: 'stretch', overflowX: 'auto', paddingBottom: 8 }}>
        {rounds.map((round, ri) => (
          <div key={ri} style={{ display: 'flex', flexDirection: 'column', minWidth: 240 }}>
            <div style={{ marginBottom: 14 }}>
              <Eyebrow color={ri === rounds.length - 1 ? 'magenta' : 'faint'}>{round.name}</Eyebrow>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-around', flex: 1, gap: 16 }}>
              {round.matchups.map((m, mi) => <MatchCard key={mi} m={m} />)}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

Object.assign(window, { Playoffs });
