using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    // -----------------------------------------------------
    // Current authenticated user
    // GET /api/users/me
    // -----------------------------------------------------

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        ApplicationUser? user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        IList<string> roles =
            await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Age,
            roles
        });
    }


    // -----------------------------------------------------
    // Get all users - ADMIN ONLY
    // GET /api/users
    // -----------------------------------------------------

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users =
            await _userManager
                .Users
                .Select(user => new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Age
                })
                .ToListAsync();

        return Ok(users);
    }


    // -----------------------------------------------------
    // Search user safely by email - ADMIN ONLY
    // GET /api/users/search?email=...
    // -----------------------------------------------------

    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchByEmail(
        [FromQuery] string email)
    {
        var user =
            await _userManager
                .Users
                .Where(user => user.Email == email)
                .Select(user => new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Age
                })
                .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
