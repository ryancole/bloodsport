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
                _logger.LogInformation("Season start email batch sent to {count} recipients for season {seasonId}.", batch.Count, season.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send season start email batch for season {seasonId}.", season.Id);
            }
        }
    }
}
