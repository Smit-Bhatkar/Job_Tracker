// =============================================================================
// FILE: Settings/MongoDbSettings.cs
// PURPOSE: A simple "options" class that holds our MongoDB connection settings.
//          These values come from appsettings.json via the Options Pattern.
//
// KEY CONCEPT — The Options Pattern
//   Instead of hardcoding connection strings in your code (bad practice!),
//   ASP.NET Core lets you:
//     1. Define settings in appsettings.json (a config file)
//     2. Create a C# class that matches the JSON structure (this file!)
//     3. Use builder.Services.Configure<T>() to bind them together
//     4. Inject IOptions<T> wherever you need the settings
//
//   Benefits:
//     - Settings are in one place (appsettings.json), easy to change
//     - Different settings for Development vs Production (appsettings.Development.json)
//     - No secrets in source code (you can use environment variables or user secrets)
//     - Strongly typed — the compiler catches typos in property names
//
// KEY CONCEPT — Why not just read appsettings.json directly?
//   You could, but the Options Pattern gives you:
//     - Type safety (no casting from string to int, etc.)
//     - IntelliSense in your IDE
//     - Validation support
//     - Clean dependency injection
// =============================================================================

namespace JobTracker.API.Settings;

/// <summary>
/// Holds MongoDB connection configuration values.
/// This class structure must match the "MongoDbSettings" section in appsettings.json.
/// The property names here must exactly match the JSON key names (case-insensitive).
/// </summary>
public class MongoDbSettings
{
    // The full connection string to MongoDB Atlas (cloud) or a local MongoDB instance.
    // Example Atlas format: "mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/..."
    // Example local format: "mongodb://localhost:27017"
    public string ConnectionString { get; set; } = string.Empty;

    // The name of the database to use within MongoDB.
    // MongoDB creates the database automatically if it doesn't exist yet!
    // In our case: "JobTrackerDb"
    public string DatabaseName { get; set; } = string.Empty;

    // The name of the collection (like a "table" in SQL) that stores our documents.
    // MongoDB creates the collection automatically on first insert!
    // In our case: "Applications"
    public string CollectionName { get; set; } = string.Empty;
}
