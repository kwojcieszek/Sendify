namespace Sendify.FilterService;

public class FilterResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
}