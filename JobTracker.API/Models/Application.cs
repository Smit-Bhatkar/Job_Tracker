// =============================================================================
// FILE: Models/Application.cs
// PURPOSE: Defines the data model (also called an "entity" or "document")
//          for a job application. This class maps directly to a document
//          in our MongoDB "Applications" collection.
//
// KEY CONCEPT — What is a Model?
//   A model is a C# class that represents the shape of your data.
//   Each property becomes a field in the MongoDB document (like a column
//   in a SQL table, but more flexible since MongoDB is document-based).
//
// KEY CONCEPT — MongoDB Attributes
//   We use special attributes (the things in [brackets] above properties)
//   to tell the MongoDB Driver how to map C# properties to BSON fields.
//   BSON = Binary JSON, the format MongoDB uses internally.
// =============================================================================

// These 'using' statements import the MongoDB libraries we need for attributes.
// MongoDB.Bson         → gives us BsonId, BsonRepresentation, BsonType
// MongoDB.Bson.Serialization.Attributes → gives us BsonElement
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// The namespace organizes our code. Convention: ProjectName.FolderName
namespace JobTracker.API.Models;

/// <summary>
/// Represents a single job application document stored in MongoDB.
/// Each instance of this class = one document in the "Applications" collection.
/// </summary>
public class Application
{
    // -------------------------------------------------------------------------
    // [BsonId] — Tells MongoDB "this property is the unique identifier (_id)"
    //   Every MongoDB document MUST have an _id field. This attribute maps
    //   our C# "Id" property to MongoDB's "_id" field automatically.
    //
    // [BsonRepresentation(BsonType.ObjectId)] — Tells the driver to convert
    //   between C# string ↔ MongoDB ObjectId automatically.
    //   ObjectId is MongoDB's default 12-byte unique ID format (like "507f1f77bcf86cd799439011").
    //   We store it as a simple string in C# for convenience, but MongoDB
    //   stores it as a native ObjectId type for performance.
    // -------------------------------------------------------------------------
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // -------------------------------------------------------------------------
    // [BsonElement("company")] — Maps this C# property to a specific field
    //   name in MongoDB. Without this, MongoDB would use "Company" (PascalCase).
    //   Using camelCase field names is a common convention in MongoDB/JSON.
    //
    // 'required' keyword (C# 11+) — means this property MUST be set when
    //   creating a new Application object. The compiler enforces this!
    // -------------------------------------------------------------------------
    [BsonElement("company")]
    public required string Company { get; set; }

    // The job role/title the user applied for (e.g., "Software Engineer")
    [BsonElement("role")]
    public required string Role { get; set; }

    // -------------------------------------------------------------------------
    // Type of position: either "Internship" or "Job"
    // We use a simple string here for flexibility. In a production app, you
    // might use an enum, but strings keep things simple for learning and
    // are easier to work with in JSON/MongoDB.
    // -------------------------------------------------------------------------
    [BsonElement("type")]
    public required string Type { get; set; }

    // -------------------------------------------------------------------------
    // Status tracks where the application is in the pipeline:
    //   "Wishlist" → Haven't applied yet, just interested
    //   "Applied"  → Application submitted
    //   "Interview"→ Got an interview
    //   "Offer"    → Received an offer 🎉
    //   "Rejected" → Didn't work out 😢
    //
    // This acts like a Kanban board column for job applications.
    // -------------------------------------------------------------------------
    [BsonElement("status")]
    public required string Status { get; set; }

    // -------------------------------------------------------------------------
    // DateTime? — The '?' makes this nullable, meaning it can be null.
    //   Why nullable? Because if the status is "Wishlist", the user hasn't
    //   applied yet, so there's no application date to record.
    //
    // [BsonElement] with camelCase ensures consistent field naming in MongoDB.
    // -------------------------------------------------------------------------
    [BsonElement("dateApplied")]
    public DateTime? DateApplied { get; set; }

    // -------------------------------------------------------------------------
    // Optional fields — Link to the job posting and any personal notes.
    // The '?' on string makes these nullable (can be null or missing).
    // Not every application needs a link or notes, so we keep them optional.
    // -------------------------------------------------------------------------
    [BsonElement("link")]
    public string? Link { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    // -------------------------------------------------------------------------
    // CreatedAt — Automatically set when a new application is created.
    //   We use DateTime.UtcNow (not DateTime.Now) because UTC is timezone-
    //   independent. This avoids bugs when your server and database are in
    //   different timezones. Always store dates in UTC!
    //
    // The default value ensures CreatedAt is set even if the caller forgets.
    // -------------------------------------------------------------------------
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional: the id of the user who owns this application. Stored as
    // a string representation of the MongoDB ObjectId for the user.
    [BsonElement("userId")]
    public string? UserId { get; set; }
}
