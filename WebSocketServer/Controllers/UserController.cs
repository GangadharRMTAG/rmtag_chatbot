using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // [HttpPost("join")]
    // public async Task<IActionResult> JoinRoom([FromBody] User user)
    // {
    //     Console.WriteLine($"Received join request: Username: {user.Username}, Room: {user.roomname}");
        
    //     if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.roomname))
    //     {
    //         return BadRequest("Invalid user data.");
    //     }
    //      Console.WriteLine($"Adding user: {user.Username} to room: {user.roomname}");

    //     _context.Users.Add(user);
    //     await _context.SaveChangesAsync();

    //     return Ok(user);
    // }
    [HttpPost("join")]
    public async Task<IActionResult> JoinRoom([FromBody] User user)
    {
        Console.WriteLine($"Received join request: Username: {user.Username}, Room: {user.roomname}");
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username && u.roomname == user.roomname);

        if (existingUser != null)
        {
            return Ok(new { message = "User already in the room", navigate = true });
        }

        user.ConnectionTime = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Adding user: {user.Username} to room: {user.roomname}");

        return Ok(new { message = "User added successfully", navigate = true });
    }


    [HttpGet("connect")]
    public async Task<IActionResult> JoinRoom([FromQuery] string Username,[FromQuery] string roomname)
    {
        Console.WriteLine($"POST: Received join request: Username: {Username}, Room: {roomname}");

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(roomname))
        {
            return BadRequest("Invalid user data.");
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == Username && u.roomname == roomname);

        if (existingUser != null)
        {
            return Ok(new { message = "User already in the room", navigate = true });
        }

        var user = new User
        {
            Username = Username,
            roomname = roomname,
            ConnectionTime = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User added successfully", navigate = true });
    }

    [HttpGet("GetAllUsersInRoom/{roomName}")]
    public IActionResult GetAllUsersInRoom(string roomName)
    {
        var usersInRoom = _context.Users
            .Where(u => u.roomname == roomName)
            .Select(u => u.Username)
            .ToList();

        return Ok(usersInRoom);
    }


}
