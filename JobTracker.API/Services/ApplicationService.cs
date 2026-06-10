// =============================================================================
// FILE: Services/ApplicationService.cs
// PURPOSE: The "service layer" — contains all the business logic and database
//          operations for job applications. This keeps our controller thin
//          and our code organized.
//
// KEY CONCEPT — Service Layer Pattern
//   Instead of putting database code directly in the controller, we create
//   a separate service class. This gives us:
//     - Separation of Concerns: Controller handles HTTP, Service handles data
//     - Testability: Easy to mock the service in unit tests
//     - Reusability: Multiple controllers could use the same service
//     - Clean Code: Each class has one job (Single Responsibility Principle)
//
// KEY CONCEPT — Dependency Injection (DI)
//   This service receives its dependencies (settings) through its constructor.
//   ASP.NET Core's DI container creates and provides these automatically.
//   We never write "new ApplicationService()" ourselves — the framework does it!
//
// KEY CONCEPT — Async/Await
//   All database methods are async (return Task<T>). This means:
//     - The thread isn't blocked while waiting for MongoDB to respond
//     - Your API can handle other requests during the wait
//     - Always use 'await' when calling async methods
//   Think of it like ordering food: you don't stand at the counter waiting,
//   you sit down and they call your name when it's ready.
// =============================================================================

using Microsoft.Extensions.Options;    // For IOptions<T>
using MongoDB.Driver;                  // For MongoClient, IMongoCollection, etc.
using JobTracker.API.Models;           // For our Application model
using JobTracker.API.Settings;         // For MongoDbSettings

namespace JobTracker.API.Services;

/// <summary>
/// Handles all CRUD (Create, Read, Update, Delete) operations for job applications.
/// This service talks directly to MongoDB using the official C# driver.
/// </summary>
public class ApplicationService
{
    // -------------------------------------------------------------------------
    // _collection holds a reference to our MongoDB collection.
    // Think of a collection like a "table" in SQL, but it stores JSON-like
    // documents instead of rows. IMongoCollection<Application> means this
    // collection stores Application documents.
    //
    // 'readonly' means this field can only be set in the constructor.
    // This is a safety measure — once initialized, the collection reference
    // should never change during the lifetime of the service.
    // -------------------------------------------------------------------------
    private readonly IMongoCollection<Application> _collection;

    // -------------------------------------------------------------------------
    // CONSTRUCTOR — Called once when ASP.NET Core creates this service.
    //
    // IOptions<MongoDbSettings> is injected automatically by the DI container.
    // It wraps our MongoDbSettings object with the values from appsettings.json.
    // We access the actual settings via options.Value.
    //
    // Flow:
    //   1. Read settings from IOptions wrapper
    //   2. Create a MongoClient (the connection to MongoDB)
    //   3. Get the specific database from the client
    //   4. Get the specific collection from the database
    //
    // Why is MongoClient creation here OK for a singleton service?
    //   MongoClient is designed to be a singleton — it manages its own
    //   connection pool internally. Creating one MongoClient per application
    //   is the recommended approach. Since our service is registered as a
    //   singleton too, this constructor runs exactly once.
    // -------------------------------------------------------------------------
    public ApplicationService(IOptions<MongoDbSettings> options)
    {
        // Step 1: Unwrap the settings from the IOptions wrapper
        var settings = options.Value;

        // Step 2: Create the MongoDB client — this is our connection to the database server.
        //   MongoClient manages a pool of connections internally, so one client
        //   can efficiently handle many concurrent requests.
        var client = new MongoClient(settings.ConnectionString);

        // Step 3: Get a reference to our specific database.
        //   If the database doesn't exist yet, MongoDB creates it when you
        //   first insert data. No need for "CREATE DATABASE" like in SQL!
        var database = client.GetDatabase(settings.DatabaseName);

        // Step 4: Get a reference to our collection within that database.
        //   The generic type <Application> tells the driver what C# class
        //   to use when reading/writing documents. It handles all the
        //   serialization (C# object ↔ BSON document) automatically.
        _collection = database.GetCollection<Application>(settings.CollectionName);
    }

    // =========================================================================
    //                          CRUD OPERATIONS
    // =========================================================================

    // -------------------------------------------------------------------------
    // READ ALL — Returns every application in the collection.
    //
    // How it works:
    //   Find(app => true) — The lambda "app => true" is a filter that matches
    //   ALL documents (since 'true' is always true). It's equivalent to
    //   "SELECT * FROM Applications" in SQL.
    //
    //   .ToListAsync() — Executes the query and converts the results to a
    //   List<Application>. The 'Async' suffix means this runs without
    //   blocking the thread.
    // -------------------------------------------------------------------------
    // If userId is provided, return only applications that belong to that user.
    public async Task<List<Application>> GetAllAsync(string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return await _collection.Find(app => true).ToListAsync();
        }

        return await _collection.Find(app => app.UserId == userId).ToListAsync();
    }

    // -------------------------------------------------------------------------
    // READ ONE — Finds a single application by its MongoDB _id.
    //
    // How it works:
    //   Find(app => app.Id == id) — Filter that matches only the document
    //   whose Id field equals the provided id string.
    //
    //   .FirstOrDefaultAsync() — Returns the first matching document, or
    //   null if no document matches. This is safer than .FirstAsync()
    //   which throws an exception if nothing is found.
    //
    // The return type Application? (nullable) tells callers "this might
    // return null if the id doesn't exist" — they should check for null!
    // -------------------------------------------------------------------------
    public async Task<Application?> GetByIdAsync(string id)
    {
        return await _collection.Find(app => app.Id == id).FirstOrDefaultAsync();
    }

    // -------------------------------------------------------------------------
    // CREATE — Inserts a new application document into the collection.
    //
    // How it works:
    //   InsertOneAsync(application) — Takes our C# object, serializes it to
    //   BSON, and inserts it into MongoDB. After insertion, MongoDB
    //   automatically generates an _id (ObjectId) and the driver sets
    //   it on our application.Id property. Magic! ✨
    //
    // We return the application object so the caller gets back the
    // newly-assigned Id (useful for the API to return the created resource).
    // -------------------------------------------------------------------------
    public async Task<Application> CreateAsync(Application application)
    {
        // Ensure CreatedAt is set to the current UTC time
        application.CreatedAt = DateTime.UtcNow;

        // Insert into MongoDB — the driver handles serialization
        await _collection.InsertOneAsync(application);

        // Return the application with its new Id populated by MongoDB
        return application;
    }

    // -------------------------------------------------------------------------
    // UPDATE — Replaces an entire document with updated data.
    //
    // How it works:
    //   ReplaceOneAsync(filter, replacement) — Finds the document matching
    //   the filter and replaces its ENTIRE contents with the new object.
    //
    // Why ReplaceOneAsync instead of UpdateOneAsync?
    //   UpdateOneAsync requires building Update definitions (like
    //   Builders<Application>.Update.Set(x => x.Company, "Google")),
    //   which is more complex. ReplaceOneAsync is simpler — just pass
    //   the whole updated object. For a learning project, this is cleaner!
    //
    //   Trade-off: ReplaceOneAsync sends the full document over the network
    //   even if only one field changed. For small documents like ours,
    //   this is perfectly fine. For huge documents, you might prefer
    //   UpdateOneAsync to send only the changed fields.
    //
    // IMPORTANT: We set application.Id = id to make sure the _id field
    //   in the replacement document matches the original. MongoDB doesn't
    //   allow changing the _id of a document!
    // -------------------------------------------------------------------------
    public async Task UpdateAsync(string id, Application application)
    {
        // Ensure the Id on the replacement object matches the URL parameter.
        // This prevents accidental Id mismatches between the URL and the body.
        application.Id = id;

        // Replace the entire document that has this _id
        await _collection.ReplaceOneAsync(app => app.Id == id, application);
    }

    // -------------------------------------------------------------------------
    // DELETE — Removes a single document from the collection.
    //
    // How it works:
    //   DeleteOneAsync(filter) — Finds the FIRST document matching the
    //   filter and permanently removes it. There's no "undo" or "trash"
    //   in MongoDB — the document is gone forever!
    //
    //   In a production app, you might implement "soft delete" by setting
    //   a "Deleted" flag instead of actually removing the document.
    //   But for learning, real deletion keeps things simple.
    // -------------------------------------------------------------------------
    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(app => app.Id == id);
    }
}
