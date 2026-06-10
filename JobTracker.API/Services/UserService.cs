using Microsoft.Extensions.Options;
using MongoDB.Driver;
using JobTracker.API.Models;
using JobTracker.API.Settings;

namespace JobTracker.API.Services;

/// <summary>
/// Handles user registration and login with BCrypt password hashing.
/// Registered as a singleton in DI — shares one MongoClient connection pool.
/// </summary>
public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>(settings.UsersCollectionName);

        // Create a unique index on username to prevent duplicates at the DB level.
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
        var indexOptions = new CreateIndexOptions { Unique = true };
        _users.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, indexOptions));

        // Create a unique index on email as well.
        var emailIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        _users.Indexes.CreateOne(new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions));
    }

    /// <summary>
    /// Registers a new user. Returns the created user or null if username/email already exists.
    /// </summary>
    public async Task<User?> RegisterAsync(string username, string email, string password)
    {
        // Check if username or email already exists
        var existingUser = await _users.Find(u =>
            u.Username == username || u.Email == email
        ).FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return null; // Username or email already taken
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await _users.InsertOneAsync(user);
        return user;
    }

    /// <summary>
    /// Authenticates a user by username and password.
    /// Returns the user if credentials are valid, null otherwise.
    /// </summary>
    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        if (user == null)
        {
            return null; // User not found
        }

        // Verify the password against the stored BCrypt hash
        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return isValid ? user : null;
    }

    /// <summary>
    /// Retrieves a user by their MongoDB ObjectId.
    /// </summary>
    public async Task<User?> GetByIdAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }
}
