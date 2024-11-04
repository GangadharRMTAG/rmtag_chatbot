using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebSocketServer.Data;
using WebSocketServer.Models;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContext _context;

    public UserController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinRoom([FromBody] User user)
    {
        Console.WriteLine($"Received join request: Username: {user.Username}, Room: {user.roomname}");
        
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.roomname))
        {
            return BadRequest("Invalid user data.");
        }
         Console.WriteLine($"Adding user: {user.Username} to room: {user.roomname}");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }
}



