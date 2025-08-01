using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class User
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public string? GroupId { get; set; } = null;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.None;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
