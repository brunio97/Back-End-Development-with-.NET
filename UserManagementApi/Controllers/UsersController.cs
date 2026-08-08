using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users = new()
    {
        new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30
        },
        new User
        {
            Id = 2,
            Name = "Jane Smith",
            Email = "jane@example.com",
            Age = 28
        }
    };

    private static int _nextId = 3;

    // GET: api/users
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        return Ok(Users);
    }

    // GET: api/users/1
    [HttpGet("{id}")]
    public ActionResult<User> GetUser(int id)
    {
        User? user = Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = $"User with ID {id} was not found."
            });
        }

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public ActionResult<User> CreateUser(User user)
    {
        bool emailExists = Users.Any(
            u => u.Email.Equals(
                user.Email,
                StringComparison.OrdinalIgnoreCase));

        if (emailExists)
        {
            return Conflict(new
            {
                message = "A user with this email already exists."
            });
        }

        user.Id = _nextId++;
        Users.Add(user);

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user);
    }

    // PUT: api/users/1
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        User? existingUser =
            Users.FirstOrDefault(u => u.Id == id);

        if (existingUser == null)
        {
            return NotFound(new
            {
                message = $"User with ID {id} was not found."
            });
        }

        bool emailExists = Users.Any(
            u => u.Id != id &&
                 u.Email.Equals(
                     updatedUser.Email,
                     StringComparison.OrdinalIgnoreCase));

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Another user already uses this email."
            });
        }

        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;
        existingUser.Age = updatedUser.Age;

        return Ok(existingUser);
    }

    // DELETE: api/users/1
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        User? user =
            Users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = $"User with ID {id} was not found."
            });
        }

        Users.Remove(user);

        return NoContent();
    }
}
