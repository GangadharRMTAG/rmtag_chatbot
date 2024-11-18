using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Data;
using WebSocketServer.Models;
namespace WebSocketServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly DataContext _context;

    public LoginController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Login model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) ||
            string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
        {
            return BadRequest(new { message = "All fields are required." });
        }

        if (_context.Logins.Any(u => u.Email == model.Email))
            return BadRequest(new { message = "Email already registered!" });

        _context.Logins.Add(model);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Registration successful!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            return BadRequest(new { message = "Email and Password are required." });

        var user = await _context.Logins.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password!" });

        return Ok(new { message = "Login successful!" });
    }

}

