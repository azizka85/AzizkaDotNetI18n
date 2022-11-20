namespace AzizkaDotNetI18n.Options;

public class DataOptions
{
    public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    public List<ContextOptions>? Contexts { get; set; } = null;
}