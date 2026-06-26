/* Champions Amateur League — UI kit mock data (fictional, LoL-flavored). */
window.CALData = (function () {
  const teams = [
    { id: 1, name: 'Void Reavers',          tag: 'VOID', w: 5, l: 1 },
    { id: 2, name: 'Shadow Isles',          tag: 'SHDW', w: 5, l: 1 },
    { id: 3, name: 'Piltover Pulse',        tag: 'PLT',  w: 4, l: 2 },
    { id: 4, name: 'Noxian Vanguard',       tag: 'NOX',  w: 4, l: 2 },
    { id: 5, name: 'Demacia Wardens',       tag: 'DEM',  w: 3, l: 3 },
    { id: 6, name: 'Bilgewater Buccaneers', tag: 'BILG', w: 2, l: 4 },
    { id: 7, name: 'Ionian Wind',           tag: 'ION',  w: 2, l: 4 },
    { id: 8, name: 'Freljord Frost',        tag: 'FRLJ', w: 1, l: 5 },
  ];

  const seasons = [
    { id: 12, name: 'Summer Split 2026',  status: 'Active',   regOpen: false, starts: 'May 2, 2026', ends: 'Jun 13, 2026', teams: 8, weeks: 6 },
    { id: 13, name: 'Clash Cup — June',   status: 'Preseason', regOpen: true,  starts: 'Jun 20, 2026', ends: 'Jul 4, 2026',  teams: 5, weeks: 3 },
    { id: 11, name: 'Spring Split 2026',  status: 'Completed', regOpen: false, starts: 'Feb 7, 2026', ends: 'Mar 28, 2026', teams: 8, weeks: 6 },
  ];

  const playoffs = [
    { id: 7, name: 'Summer Split 2026 — Playoffs', status: 'Active',    starts: 'Jun 6, 2026', ends: 'Jun 13, 2026', season: 'Summer Split 2026' },
    { id: 6, name: 'Spring Split 2026 — Playoffs', status: 'Completed', starts: 'Mar 21, 2026', ends: 'Mar 28, 2026', season: 'Spring Split 2026' },
  ];

  const news = [
    { id: 31, title: 'Summer Split playoffs are set — top 8 lock in their seeds', author: 'Ryan Cole', date: 'Jun 6, 2026', comments: 7,
      excerpt: 'With Week 6 in the books, the bracket is final. Void Reavers and Shadow Isles enter as co-favorites at 5–1, while a three-way tie for the final seed came down to head-to-head record.' },
    { id: 30, title: 'Rule clarification: best-of-one tiebreakers (4.10)', author: 'Ryan Cole', date: 'May 24, 2026', comments: 2,
      excerpt: 'Several captains asked how seeding ties are broken. Section 4.10 now spells out the order: head-to-head result first, then total game time, then a coin flip administered by an admin.' },
    { id: 29, title: 'Clash Cup registration is open — 5 spots, single weekend', author: 'Ryan Cole', date: 'May 18, 2026', comments: 4,
      excerpt: 'A short-format side event between splits. Three weeks of regular play, then a four-team bracket. Registration closes when the fifth team locks in.' },
  ];

  const activity = [
    { type: 'Playoff', winner: 'Void Reavers',    matchup: 'Quarterfinal · VOID vs FRLJ', context: 'Summer Split 2026 — Playoffs', time: '2h ago' },
    { type: 'Playoff', winner: 'Shadow Isles',    matchup: 'Quarterfinal · SHDW vs ION',  context: 'Summer Split 2026 — Playoffs', time: '3h ago' },
    { type: 'Regular', winner: 'Piltover Pulse',  matchup: 'Week 6 · PLT vs DEM',         context: 'Summer Split 2026', time: '1d ago' },
    { type: 'Regular', winner: 'Noxian Vanguard', matchup: 'Week 6 · NOX vs BILG',        context: 'Summer Split 2026', time: '1d ago' },
    { type: 'Playoff', winner: 'Piltover Pulse',  matchup: 'Quarterfinal · PLT vs DEM',   context: 'Summer Split 2026 — Playoffs', time: '2d ago' },
    { type: 'Regular', winner: 'Void Reavers',    matchup: 'Week 5 · VOID vs SHDW',       context: 'Summer Split 2026', time: '3d ago' },
  ];

  // Single-elimination, 8-team bracket. seed maps to team by standings order.
  const bracket = {
    rounds: [
      { name: 'Quarterfinals', matchups: [
        { a: 'Void Reavers',    aSeed: 1, b: 'Freljord Frost',  bSeed: 8, winner: 'a', code: '4F2A-9KQ7' },
        { a: 'Noxian Vanguard', aSeed: 4, b: 'Demacia Wardens', bSeed: 5, winner: 'b', code: '8H1C-2RM5' },
        { a: 'Piltover Pulse',  aSeed: 3, b: 'Bilgewater Buccaneers', bSeed: 6, winner: 'a', code: 'K0PZ-7T3W' },
        { a: 'Shadow Isles',    aSeed: 2, b: 'Ionian Wind',     bSeed: 7, winner: 'a', code: 'QX44-9LB2' },
      ]},
      { name: 'Semifinals', matchups: [
        { a: 'Void Reavers',   aSeed: 1, b: 'Demacia Wardens', bSeed: 5, winner: null, code: null },
        { a: 'Piltover Pulse', aSeed: 3, b: 'Shadow Isles',    bSeed: 2, winner: null, code: null },
      ]},
      { name: 'Grand Final', matchups: [
        { a: 'TBD', aSeed: null, b: 'TBD', bSeed: null, winner: null, code: null },
      ]},
    ],
  };

  return { teams, seasons, playoffs, news, activity, bracket };
})();
