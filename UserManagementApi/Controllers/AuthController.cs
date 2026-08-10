using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.DTOs;
using UserManagementApi.Models;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly SignInManager<ApplicationUser>
        _signInManager;

    private readonly JwtService
        _jwtService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        string email =
            request.Email.Trim();

        var existingUser =
            await _userManager
                .FindByEmailAsync(email);

        if (existingUser != null)
        {
            return Conflict(
                new
                {
                    message =
                        "Email already registered."
                });
        }

        var user =
            new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = request.Name.Trim(),
                Age = request.Age
            };

        IdentityResult result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(
                result.Errors);
        }

        await _userManager.AddToRoleAsync(
            user,
            "User");

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Age
            });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(
                request.Email.Trim());

        if (user == null)
        {
            return Unauthorized(
                new
                {
                    message =
                        "Invalid credentials."
                });
        }

var result =
    await _signInManager
        .CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                new
                {
                    message =
                        "Account temporarily locked."
                });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(
                new
                {
                    message =
                        "Invalid credentials."
                });
        }

        string token =
            await _jwtService
                .CreateTokenAsync(user);

        return Ok(
            new
            {
                accessToken = token
            });
    }
}
