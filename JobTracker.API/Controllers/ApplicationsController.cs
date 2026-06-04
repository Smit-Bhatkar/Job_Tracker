// =============================================================================
// FILE: Controllers/ApplicationsController.cs
// PURPOSE: The API controller — handles incoming HTTP requests and returns
//          appropriate HTTP responses. This is the "front door" of our API.
//
// KEY CONCEPT — What is a Controller?
//   A controller is a class that groups related API endpoints together.
//   Each public method (called an "action") handles a specific HTTP request.
//   For example:
//     GET /api/applications      → GetAll()     → Returns all applications
//     GET /api/applications/123  → GetById(123) → Returns one application
//     POST /api/applications     → Create(...)  → Creates a new application
//     PUT /api/applications/123  → Update(...)  → Updates an application
//     DELETE /api/applications/123 → Delete(123)→ Deletes an application
//
// KEY CONCEPT — REST API Conventions
//   REST (Representational State Transfer) is a standard pattern for APIs:
//     - Use nouns for URLs (not verbs): /api/applications, NOT /api/getApplications
//     - Use HTTP methods to indicate the action: GET=read, POST=create, etc.
//     - Return appropriate status codes: 200=OK, 201=Created, 404=Not Found, etc.
//     - Resources are identified by URL: /api/applications/{id}
//
// KEY CONCEPT — Controller vs Service
//   The controller is THIN — it only handles HTTP concerns:
//     - Routing (which URL maps to which method)
//     - Model binding (parsing the request body/parameters)
//     - HTTP status codes (200, 201, 404, etc.)
//   The actual business logic and database operations live in the SERVICE.
// =============================================================================

using Microsoft.AspNetCore.Mvc;   // For ControllerBase, attributes, action results
using JobTracker.API.Models;       // For the Application model
using JobTracker.API.Services;     // For ApplicationService

namespace JobTracker.API.Controllers;

// =============================================================================
// ATTRIBUTES — These [brackets] above the class configure the controller.
//
// [ApiController] — Adds API-specific behaviors automatically:
//   1. Automatic model validation (returns 400 Bad Request if required fields are missing)
//   2. Automatic [FromBody] binding for complex types in POST/PUT
//   3. Problem details responses for errors (standardized error format)
//   Without this, you'd need to manually check ModelState.IsValid everywhere.
//
// [Route("api/[controller]")] — Defines the base URL for all endpoints.
//   [controller] is a magic token that gets replaced with the class name
//   minus "Controller". So "ApplicationsController" → "api/applications".
//   All action methods in this class will have URLs starting with /api/applications.
// =============================================================================
[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    // -------------------------------------------------------------------------
    // _service — Our injected ApplicationService instance.
    //   The controller delegates all database work to this service.
    //   'readonly' ensures it can only be set in the constructor.
    //
    // KEY CONCEPT — Constructor Injection
    //   ASP.NET Core's DI container sees that our constructor needs an
    //   ApplicationService, looks it up in the service registry (set up
    //   in Program.cs), and provides it automatically. We never write
    //   "new ApplicationService()" — the framework handles it!
    // -------------------------------------------------------------------------
    private readonly ApplicationService _service;

    // Constructor: ASP.NET Core injects the ApplicationService automatically
    public ApplicationsController(ApplicationService service)
    {
        _service = service;
    }

    // =========================================================================
    //                          API ENDPOINTS
    // =========================================================================

    // -------------------------------------------------------------------------
    // GET /api/applications — Returns ALL job applications.
    //
    // [HttpGet] — Maps this method to HTTP GET requests at the base route.
    //   When someone sends "GET /api/applications", this method runs.
    //
    // ActionResult<T> — A flexible return type that can return:
    //   - The data (with automatic 200 OK status)
    //   - Or an error status code (404, 500, etc.)
    //
    // Ok(result) — Returns HTTP 200 with the data serialized as JSON.
    //   ASP.NET Core automatically serializes the List<Application> to JSON.
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<List<Application>>> GetAll()
    {
        var applications = await _service.GetAllAsync();

        // Return 200 OK with the list of applications as JSON
        return Ok(applications);
    }

    // -------------------------------------------------------------------------
    // GET /api/applications/{id} — Returns ONE application by its ID.
    //
    // [HttpGet("{id}")] — The "{id}" adds a route parameter. So the full
    //   route becomes: GET /api/applications/507f1f77bcf86cd799439011
    //   The value after the last "/" is captured as the 'id' parameter.
    //
    // string id — ASP.NET Core automatically extracts the {id} from the URL
    //   and passes it to this parameter. This is called "model binding".
    //
    // NotFound() — Returns HTTP 404 if no application with that ID exists.
    //   This tells the client "the resource you asked for doesn't exist".
    // -------------------------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<ActionResult<Application>> GetById(string id)
    {
        var application = await _service.GetByIdAsync(id);

        // If the application wasn't found, return 404 Not Found
        if (application == null)
        {
            return NotFound();
        }

        // Return 200 OK with the found application
        return Ok(application);
    }

    // -------------------------------------------------------------------------
    // POST /api/applications — Creates a new job application.
    //
    // [HttpPost] — Maps to HTTP POST at the base route.
    //   POST is the standard HTTP method for creating new resources.
    //
    // [FromBody] — Not actually needed here because [ApiController] adds it
    //   automatically for complex types! But it's worth knowing: this tells
    //   ASP.NET Core to deserialize the JSON request body into an Application object.
    //
    // CreatedAtAction — Returns HTTP 201 Created with:
    //   1. A "Location" header pointing to the new resource's URL
    //      (e.g., Location: /api/applications/507f1f77bcf86cd799439011)
    //   2. The created object in the response body
    //
    //   Why 201 instead of 200?
    //   HTTP 201 specifically means "a new resource was created" — it's more
    //   descriptive than a generic 200 OK. REST APIs should use the most
    //   specific status code available.
    //
    //   The first parameter nameof(GetById) references our GET endpoint,
    //   so the Location header points to where you can fetch the new resource.
    // -------------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<Application>> Create(Application application)
    {
        var created = await _service.CreateAsync(application);

        // Return 201 Created with:
        //   - Location header → URL to fetch this new application
        //   - Body → the created application (including its new Id)
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // -------------------------------------------------------------------------
    // PUT /api/applications/{id} — Updates an existing application.
    //
    // [HttpPut("{id}")] — Maps to HTTP PUT with a route parameter.
    //   PUT means "replace the resource at this URL with the provided data".
    //
    //   PUT vs PATCH:
    //     PUT = Replace the ENTIRE resource (send all fields)
    //     PATCH = Update only SOME fields (send only what changed)
    //   We use PUT here for simplicity — the client sends the full object.
    //
    // NoContent() — Returns HTTP 204 No Content.
    //   This means "the operation succeeded, but there's nothing to return".
    //   It's the standard response for successful updates — the client
    //   already has the data (they just sent it!), so we don't need to
    //   echo it back.
    // -------------------------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Application application)
    {
        // First, check if the application exists
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(); // 404 — can't update something that doesn't exist!
        }

        // Perform the update (replace the document in MongoDB)
        await _service.UpdateAsync(id, application);

        // Return 204 No Content — update successful, no body needed
        return NoContent();
    }

    // -------------------------------------------------------------------------
    // DELETE /api/applications/{id} — Deletes an application.
    //
    // [HttpDelete("{id}")] — Maps to HTTP DELETE with a route parameter.
    //   DELETE means "remove the resource at this URL".
    //
    // NoContent() — Returns 204 No Content on success (same as PUT).
    //   Some APIs return 200 with the deleted object, but 204 is more
    //   common and simpler — the resource is gone, nothing to return.
    //
    // We check if the application exists before deleting it. This way,
    // we return 404 if someone tries to delete something that doesn't exist,
    // which is more informative than silently succeeding.
    // -------------------------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        // Check if the application exists before attempting deletion
        var application = await _service.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound(); // 404 — can't delete something that doesn't exist!
        }

        // Delete the document from MongoDB
        await _service.DeleteAsync(id);

        // Return 204 No Content — deletion successful
        return NoContent();
    }
}
