using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
namespace backends.Controllers;

[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly DynamoDBContext _context;
    private readonly CustomJwtService _jwtService;
    public ValuesController(IAmazonDynamoDB dynamoDb, CustomJwtService jwtService)
    {
        _context = new DynamoDBContext(dynamoDb);
        _jwtService = jwtService;
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] User model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.PasswordHash))
        {
            return BadRequest("Email and password are required.");
        }
        try
        {
            var existingUser = await _context.LoadAsync<User>(model.Email);
            if (existingUser != null) return BadRequest("Email already exists.");

            model.PasswordHash = HashPassword(model.PasswordHash);
            await _context.SaveAsync(model);
        }

        catch (Exception ex)
        {

            return Ok(ex.Message);
        }

        return Ok("User registered.");
    }

    

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User model)
    {


        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.PasswordHash))
        {
            return BadRequest("Email and password are required.");
        }

        var token = "";
        string userEmail = "";
        string password = "";
        try
        {
            var user = await _context.LoadAsync<User>(model.Email);
            if (user == null || !VerifyPassword(model.PasswordHash, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }
            userEmail = user.Email;
            password = user.PasswordHash;
            token = _jwtService.GenerateToken(user.Email);
            
        }
        catch (Exception ex)
        {
            return Ok(new { Message = ex.Message + " email " + userEmail+ " password" + password });
        }

        return Ok(new { Token = token });
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private bool VerifyPassword(string inputPassword, string storedHash)
    {
        return HashPassword(inputPassword) == storedHash;
    }
}