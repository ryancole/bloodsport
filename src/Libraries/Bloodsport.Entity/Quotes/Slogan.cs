namespace Bloodsport.Entity.Quotes
{
    // A quote and who it belongs to. The Attribution is either a film title (for
    // poster taglines that nobody speaks) or a character / real person (for lines
    // that are actually spoken). The two cases are split across MovieSlogans and
    // MovieQuotes respectively.
    public record Slogan(string Quote, string Attribution);
}
