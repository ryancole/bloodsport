using Azure.Communication.Email;
using Bloodsport.Entity.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Services;

public class EmailService
{
    private const int BatchSize = 100;

    private readonly EmailClient _emailClient;
    private readonly EmailTemplateRenderer _templateRenderer;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailClient emailClient, EmailTemplateRenderer templateRenderer, IConfiguration config, ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _templateRenderer = templateRenderer;
        _config = config;
        _logger = logger;
    }

    public async Task SendSeasonStartedAsync(Season season, IEnumerable<User> members)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        var recipients = members
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .Select(u => new EmailAddress(u.Email!))
            .ToList();

        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients with email addresses found for season {seasonId} start notification.", season.Id);
            return;
        }

        var html = await _templateRenderer.RenderAsync("SeasonStarted.html", new { season_name = season.Name });

        var content = new EmailContent($"Season \"{season.Name}\" has started!")
        {
            PlainText = $"The season \"{season.Name}\" is now underway. Good luck to your team!",
            Html = html
        };

        var batches = recipients
            .Select((r, i) => (r, i))
            .GroupBy(x => x.i / BatchSize)
            .Select(g => g.Select(x => x.r).ToList());

        await SendInBatchesAsync(batches, content, logContext: $"season {season.Id}");
    }

    public async Task SendPlayoffStartedAsync(Playoff playoff, IEnumerable<(Team Team, IEnumerable<User> Members)> teamsWithMembers)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        foreach (var (team, members) in teamsWithMembers)
        {
            var recipients = members
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .Select(u => new EmailAddress(u.Email!))
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("No recipients with email addresses found for team {teamId} in playoff {playoffId} notification.", team.Id, playoff.Id);
                continue;
            }

            var html = await _templateRenderer.RenderAsync("PlayoffStarted.html", new { team_name = team.Name, playoff_name = playoff.Name });

            var content = new EmailContent($"Your team has made the {playoff.Name} playoffs!")
            {
                PlainText = $"Congratulations! {team.Name} has qualified for the {playoff.Name} playoffs. Head to the site to view your bracket.",
                Html = html
            };

            var batches = recipients
                .Select((r, i) => (r, i))
                .GroupBy(x => x.i / BatchSize)
                .Select(g => g.Select(x => x.r).ToList());

            await SendInBatchesAsync(batches, content, logContext: $"team {team.Id} in playoff {playoff.Id}");
        }
    }

    public async Task SendPlayoffRoundEndedAsync(PlayoffRound round, IEnumerable<User> members)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        var recipients = members
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .Select(u => new EmailAddress(u.Email!))
            .ToList();

        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients with email addresses found for playoff round {roundId} end notification.", round.Id);
            return;
        }

        var html = await _templateRenderer.RenderAsync("PlayoffRoundEnded.html", new
        {
            round_name = round.Name,
            playoff_name = round.Playoff.Name
        });

        var content = new EmailContent($"{round.Name} of the {round.Playoff.Name} playoffs is complete!")
        {
            PlainText = $"Thanks for competing in {round.Name} of the {round.Playoff.Name} playoffs. Head to the site to check the updated bracket.",
            Html = html
        };

        var batches = recipients
            .Select((r, i) => (r, i))
            .GroupBy(x => x.i / BatchSize)
            .Select(g => g.Select(x => x.r).ToList());

        await SendInBatchesAsync(batches, content, logContext: $"playoff round {round.Id}");
    }

    public async Task SendSeasonEndedAsync(Season season, IEnumerable<(Team Team, int WinCount, int LoseCount, IEnumerable<User> Members)> teamResults)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        foreach (var (team, winCount, loseCount, members) in teamResults)
        {
            var recipients = members
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .Select(u => new EmailAddress(u.Email!))
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("No recipients with email addresses found for team {teamId} in season {seasonId} end notification.", team.Id, season.Id);
                continue;
            }

            var html = await _templateRenderer.RenderAsync("SeasonEnded.html", new
            {
                season_name = season.Name,
                team_name = team.Name,
                win_count = winCount,
                lose_count = loseCount
            });

            var content = new EmailContent($"{season.Name} is over — {team.Name} finished {winCount}W-{loseCount}L")
            {
                PlainText = $"{season.Name} has ended. {team.Name} finished with a record of {winCount}-{loseCount}. Head to the site to view the full standings.",
                Html = html
            };

            var batches = recipients
                .Select((r, i) => (r, i))
                .GroupBy(x => x.i / BatchSize)
                .Select(g => g.Select(x => x.r).ToList());

            await SendInBatchesAsync(batches, content, logContext: $"team {team.Id} in season {season.Id}");
        }
    }

    public async Task SendMatchupScheduledAsync(SeasonWeek week, SeasonWeekMatchup matchup, Team team, Team opponent, IEnumerable<User> members)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        var recipients = members
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .Select(u => new EmailAddress(u.Email!))
            .ToList();

        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients with email addresses found for matchup {matchupId} notification.", matchup.Id);
            return;
        }

        var html = await _templateRenderer.RenderAsync("MatchupScheduled.html", new
        {
            week_name = week.Name,
            season_name = week.Season.Name,
            team_name = team.Name,
            opponent_name = opponent.Name,
            date_end = week.DateEnd.ToString("MMMM d, yyyy")
        });

        var content = new EmailContent($"{week.Name}: {team.Name} vs {opponent.Name}")
        {
            PlainText = $"{week.Name} is now underway. {team.Name} vs {opponent.Name}. Complete your match by {week.DateEnd:MMMM d, yyyy}.",
            Html = html
        };

        var batches = recipients
            .Select((r, i) => (r, i))
            .GroupBy(x => x.i / BatchSize)
            .Select(g => g.Select(x => x.r).ToList());

        await SendInBatchesAsync(batches, content, logContext: $"matchup {matchup.Id}");
    }

    private async Task SendInBatchesAsync(IEnumerable<List<EmailAddress>> batches, EmailContent content, string logContext)
    {
        var sender = _config["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured.");

        foreach (var batch in batches)
        {
            var emailRecipients = new EmailRecipients([]);
            foreach (var address in batch)
                emailRecipients.BCC.Add(address);

            var message = new EmailMessage(
                senderAddress: sender,
                recipients: emailRecipients,
                content: content);

            try
            {
                await _emailClient.SendAsync(Azure.WaitUntil.Started, message);
                _logger.LogInformation("Email batch sent to {count} recipients for {logContext}.", batch.Count, logContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email batch for {logContext}.", logContext);
            }
        }
    }
}
