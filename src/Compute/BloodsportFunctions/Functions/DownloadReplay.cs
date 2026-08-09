using Azure.Messaging.ServiceBus;
using Bloodsport.Entity.RiotApi;
using BloodsportFunctions.Services;
using Camille.Enums;
using Camille.RiotGames;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloodsportFunctions.Functions;

public class DownloadReplay
{
    private readonly ILogger<DownloadReplay> _logger;
    private readonly RiotGamesApi _riotApi;
    private readonly RiotReplayService _replayService;
    private readonly IConfiguration _config;

    public DownloadReplay(
        ILogger<DownloadReplay> logger,
        RiotGamesApi riotApi,
        RiotReplayService replayService,
        IConfiguration config)
    {
        _logger = logger;
        _riotApi = riotApi;
        _replayService = replayService;
        _config = config;
    }

    private RegionalRoute Route =>
        Enum.Parse<RegionalRoute>(_config["RiotApi:RegionalRoute"] ?? "AMERICAS", ignoreCase: true);

    [Function(nameof(DownloadReplay))]
    public async Task Run(
        [ServiceBusTrigger("download-replay", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var matchId = message.Body.ToString().Trim().Trim('"');

        if (string.IsNullOrEmpty(matchId))
        {
            _logger.LogError("Message {MessageId} had an empty match ID body.", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "EmptyMatchId");
            return;
        }

        if (RiotApiEndpoints.UseStub)
        {
            _logger.LogInformation("Riot API is stubbed; skipping replay download for match {MatchId}.", matchId);
            await messageActions.CompleteMessageAsync(message);
            return;
        }

        // Replay file names are built from the numeric half of the match ID (NA1_1234567890),
        // which is what we match the returned URLs against.
        var underscore = matchId.IndexOf('_');
        var gameId = underscore >= 0 ? matchId[(underscore + 1)..] : matchId;

        _logger.LogInformation("Looking up replay for match {MatchId} (game {GameId}).", matchId, gameId);

        try
        {
            var match = await _riotApi.MatchV5().GetMatchAsync(Route, matchId);
            var puuids = match?.Metadata?.Participants ?? [];

            if (puuids.Length == 0)
            {
                _logger.LogError("Match {MatchId} returned no participants.", matchId);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "NoParticipants");
                return;
            }

            // The replay endpoint is keyed by player, not by match, so walk the participants until
            // one of them still has the file for this game.
            string? fileUrl = null;

            for (var i = 0; i < puuids.Length && fileUrl is null; i++)
            {
                var urls = await _replayService.GetReplayUrlsAsync(Route, puuids[i]);

                fileUrl = urls.FirstOrDefault(u => u.Contains(gameId, StringComparison.Ordinal));

                _logger.LogInformation(
                    "Participant {Index} of {Total} returned {Count} replay URL(s) for match {MatchId}; match found: {Found}.",
                    i + 1, puuids.Length, urls.Count, matchId, fileUrl is not null);
            }

            if (fileUrl is null)
            {
                _logger.LogError(
                    "No participant had a replay file for match {MatchId}. The replay has likely expired with a patch.",
                    matchId);
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "ReplayNotAvailable");
                return;
            }

            using var response = await _replayService.DownloadAsync(fileUrl);

            // TODO: upload straight to blob storage instead of buffering. Buffering only exists so
            // the log below proves the download works end to end.
            using var buffer = new MemoryStream();
            await response.Content.CopyToAsync(buffer);

            _logger.LogInformation(
                "Downloaded {Bytes} byte replay for match {MatchId}. Storage upload is not implemented yet.",
                buffer.Length, matchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download replay for match {MatchId}.", matchId);
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "ReplayDownloadFailure",
                deadLetterErrorDescription: ex.Message);
            return;
        }

        await messageActions.CompleteMessageAsync(message);
    }
}
