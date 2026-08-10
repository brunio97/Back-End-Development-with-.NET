using Microsoft.AspNetCore.Identity;

namespace UserManagementApi.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
}
