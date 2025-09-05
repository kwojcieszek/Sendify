namespace Sendify.Data;

public class NewTokenModel
{
    public string TokenName { get; set; }
    public string Description { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}