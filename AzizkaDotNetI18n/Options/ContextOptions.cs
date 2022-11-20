namespace AzizkaDotNetI18n.Options;

public class ContextOptions
{
    public Dictionary<string, string> Matches { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
}