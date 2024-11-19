using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Data;
using WebSocketServer.Models;
namespace WebSocketServer.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;

    public AuthController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest(new { message = "All fields are required" });
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        user.ConnectionTime = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Registration successful" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        Console.WriteLine($"Logging in: Email={loginUser.Email}");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginUser.Email && u.Password == loginUser.Password);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid email or password" });
        }

        return Ok(new
        {
            message = "Login successful",
            username = user.Username,
            roomname = user.roomname
        });
    }
}