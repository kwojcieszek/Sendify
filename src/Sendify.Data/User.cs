using MongoDB.Bson.Serialization.Attributes;

namespace Sendify.Data;

public class User
{
    [BsonId]
    public string Id { get; set; }
    public string? GroupId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool IsHashed { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.None;
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
