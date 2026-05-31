using Moserware.Skills;
using Bloodsport.Core.Models;

namespace Bloodsport.Core.Rating;

public class TrueSkillService
{
    private readonly GameInfo _gameInfo;

    public TrueSkillService()
    {
        // Tuned for LoL: no draws, higher beta = more decisive skill separation
        _gameInfo = new GameInfo(
            initialMean: 25.0,
            initialStandardDeviation: 25.0 / 3.0,
            beta: 25.0 / 6.0,
            dynamicsFactor: 25.0 / 300.0,
            drawProbability: 0.0);
    }

    public (RatingUpdate Winner, RatingUpdate Loser) CalculateMatchOutcome(Player winner, Player loser)
    {
        var winnerRating = new Moserware.Skills.Rating(winner.TrueSkillMu, winner.TrueSkillSigma);
        var loserRating = new Moserware.Skills.Rating(loser.TrueSkillMu, loser.TrueSkillSigma);

        var winnerTeam = new Team(new Moserware.Skills.Player(winner.Id), winnerRating);
        var loserTeam = new Team(new Moserware.Skills.Player(loser.Id), loserRating);

        var teams = Teams.Concat(winnerTeam, loserTeam);
        var newRatings = TrueSkillCalculator.CalculateNewRatings(_gameInfo, teams, 1, 2);

        var newWinnerRating = newRatings[new Moserware.Skills.Player(winner.Id)];
        var newLoserRating = newRatings[new Moserware.Skills.Player(loser.Id)];

        return (
            new RatingUpdate(winner.Id, newWinnerRating.Mean, newWinnerRating.StandardDeviation),
            new RatingUpdate(loser.Id, newLoserRating.Mean, newLoserRating.StandardDeviation)
        );
    }

    public double GetDisplayRating(double mu, double sigma) => mu - 3 * sigma;

    // Win probability for matchup display
    public double WinProbability(Player player, Player opponent)
    {
        var deltaMu = player.TrueSkillMu - opponent.TrueSkillMu;
        var sumSigmaSquared = player.TrueSkillSigma * player.TrueSkillSigma
                            + opponent.TrueSkillSigma * opponent.TrueSkillSigma;
        var denom = Math.Sqrt(2 * _gameInfo.Beta * _gameInfo.Beta + sumSigmaSquared);
        return Phi(deltaMu / denom);
    }

    private static double Phi(double x)
    {
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    }

    private static double Erf(double x)
    {
        // Abramowitz and Stegun approximation
        const double a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741;
        const double a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911;
        double sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
        return sign * y;
    }
}

public record RatingUpdate(Guid PlayerId, double NewMu, double NewSigma);
