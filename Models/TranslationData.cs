public class TranslationData
{
    public string Name { get; set; }
    public DateTime Last_updated { get; set; }
    public Dictionary<string, Dictionary<string, string>>? Translations { get; set; }
}