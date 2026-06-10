using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JobTracker.API.Models;

/// <summary>
/// Represents a user document stored in the MongoDB "Users" collection.
/// Passwords are never stored in plain text — only BCrypt hashes.
/// </summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public required string Username { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("passwordHash")]
    public required string PasswordHash { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
