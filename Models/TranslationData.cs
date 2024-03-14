public class TranslationData
{
    public string? BasicPath { get; set; }
    public DateTime LastUpdate { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Translations { get; set; }
}