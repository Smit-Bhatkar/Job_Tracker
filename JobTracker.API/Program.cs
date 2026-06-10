// =============================================================================
// FILE: Program.cs
// PURPOSE: The entry point of the ASP.NET Core application. This is where
//          we configure all services (DI), middleware, and the HTTP pipeline.
//
// KEY CONCEPT — Program.cs Structure
//   Modern ASP.NET Core uses "minimal hosting" — no Startup class needed!
//   The file has two main sections:
//     1. SERVICE REGISTRATION (before builder.Build())
//        → Configure dependencies, settings, CORS, etc.
//     2. MIDDLEWARE PIPELINE (after builder.Build())
//        → Configure how HTTP requests are processed
//
// Think of it like opening a restaurant:
//   Section 1 = Hiring staff, setting up the kitchen (preparation)
//   Section 2 = Defining the order of operations when a customer arrives
//               (greet → seat → take order → serve → bill)
// =============================================================================

using JobTracker.API.Settings;    // For MongoDbSettings
using JobTracker.API.Services;    // For ApplicationService

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
//                     1. SERVICE REGISTRATION
// =============================================================================

// ---- MongoDB Settings via Options Pattern ----
// Configure<T>() reads a section from appsettings.json and binds it to our class.
// "MongoDbSettings" must match the JSON section name exactly.
// Now anywhere we inject IOptions<MongoDbSettings>, we get these values!
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// ---- Register ApplicationService as Singleton ----
// AddSingleton means ONE instance is created and shared across ALL requests.
// This is perfect for our service because:
//   1. MongoClient (used internally) is designed to be a singleton
//   2. MongoClient manages its own connection pool
//   3. Creating multiple MongoClients wastes resources
//
// Other lifetimes you could use (but shouldn't here):
//   AddScoped    → New instance per HTTP request (good for DbContext in EF Core)
//   AddTransient → New instance every time it's injected (good for lightweight services)
builder.Services.AddSingleton<ApplicationService>();
builder.Services.AddSingleton<UserService>();

// ---- Add Controllers ----
// This tells ASP.NET Core to look for Controller classes and set up routing.
// Without this, our ApplicationsController wouldn't be discovered!
builder.Services.AddControllers();

// ---- CORS (Cross-Origin Resource Sharing) ----
// CORS is a browser security feature. By default, browsers BLOCK requests
// from one origin (e.g., http://localhost:4200) to a different origin
// (e.g., http://localhost:5227). Our Angular app needs to call our API,
// so we must explicitly allow it.
//
// Without CORS configuration, the Angular app would get this error:
//   "Access to XMLHttpRequest has been blocked by CORS policy"
//
// We create a named policy "AllowAngularDev" that:
//   - WithOrigins → allows requests from Angular's dev server
//   - AllowAnyHeader → allows any HTTP header (Content-Type, Authorization, etc.)
//   - AllowAnyMethod → allows any HTTP method (GET, POST, PUT, DELETE, etc.)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // Angular dev server URL
              .AllowAnyHeader()                       // Accept all headers
              .AllowAnyMethod();                      // Accept all HTTP methods
    });
});

// ---- OpenAPI / Swagger ----
// Enables the auto-generated API documentation at /openapi/v1.json.
// This lets you explore and test your API endpoints in a browser!
builder.Services.AddOpenApi();

// =============================================================================
//                     2. BUILD THE APP
// =============================================================================

var app = builder.Build();

// =============================================================================
//                     3. MIDDLEWARE PIPELINE
// =============================================================================
// Middleware runs in ORDER for every HTTP request. Think of it as a series
// of "filters" that each request passes through:
//
//   Request → [CORS] → [Authorization] → [Routing → Controller] → Response
//
// The ORDER MATTERS! CORS must come before Authorization, which must come
// before controller mapping. Getting the order wrong can cause subtle bugs.

// ---- OpenAPI (Development only) ----
// Only expose the API docs in development mode, not in production.
// app.Environment.IsDevelopment() checks the ASPNETCORE_ENVIRONMENT variable.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ---- Enable CORS ----
// Apply the "AllowAngularDev" policy we defined above.
// This MUST come before UseAuthorization and MapControllers!
app.UseCors("AllowAngularDev");

// ---- Authorization Middleware ----
// Even though we don't have authentication set up yet, this middleware is
// included by default. It's a no-op for now but will be important when
// we add authentication later (e.g., JWT tokens).
app.UseAuthorization();

// ---- Serve Angular Static Files ----
// Angular 19 outputs built files to a "browser" subfolder.
// We configure the static file middleware to serve from wwwroot/browser.
var staticFileOptions = new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "browser"))
};
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = staticFileOptions.FileProvider
});
app.UseStaticFiles(staticFileOptions);

// ---- Map Controller Routes ----
// This tells ASP.NET Core to route incoming requests to our controller
// action methods based on their [Route], [HttpGet], [HttpPost], etc. attributes.
// Without this line, no controller endpoints would work!
app.MapControllers();

// ---- SPA Fallback ----
// For any request that doesn't match an API route or a static file,
// serve index.html so Angular's client-side routing can handle it.
app.MapFallbackToFile("browser/index.html");

// ---- Start the Server ----
// This is a blocking call that starts listening for HTTP requests.
// The app runs until you stop it (Ctrl+C in the terminal).
// By default, it listens on http://localhost:5227 (configured in launchSettings.json).
app.Run();
