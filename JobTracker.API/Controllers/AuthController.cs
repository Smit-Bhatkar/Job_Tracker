using Microsoft.AspNetCore.Mvc;
using JobTracker.API.Models;
using JobTracker.API.Services;

namespace JobTracker.API.Controllers;

/// <summary>
/// Authentication endpoints for user registration and login.
/// Route: /api/auth
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// POST /api/auth/register
    /// Creates a new user account with BCrypt-hashed password.
    /// Returns 201 Created on success, 409 Conflict if username/email already exists.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = await _userService.RegisterAsync(
            request.Username.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.Password
        );

        if (user == null)
        {
            return Conflict(new { message = "Username or email already exists." });
        }

        var response = new AuthResponse
        {
            Id = user.Id!,
            Username = user.Username,
            Email = user.Email
        };

        return StatusCode(201, response);
    }

    /// <summary>
    /// POST /api/auth/login
    /// Validates credentials and returns user info on success.
    /// Returns 401 Unauthorized on failure.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Username.Trim(), request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var response = new AuthResponse
        {
            Id = user.Id!,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(response);
    }
}
