using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
namespace backends.Controllers;

[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly DynamoDBContext _context;
    private readonly CustomJwtService _jwtService;
    private readonly CustomJwtService _s3Service;
    private readonly IConfiguration _config;
    private readonly IAmazonS3 _s3Client;
    public ValuesController(IAmazonS3 s3Client,IAmazonDynamoDB dynamoDb,
        CustomJwtService jwtService, IConfiguration config)
    {
        _context = new DynamoDBContext(dynamoDb);
        _jwtService = jwtService;
        //_s3Service = s3Service;
        _s3Client = s3Client;

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


    [HttpPost("upload2")]
    public async Task<IActionResult> UploadProfileImage2([FromForm] IFormFile file)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileContent = Convert.ToBase64String(memoryStream.ToArray());

        var fileName = file.FileName;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            fileContent = fileContent,
            fileName = fileName
        };

        var response = await httpClient.PostAsJsonAsync("YOUR_API_GATEWAY_URL", payload);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

        return Ok(new { ImageUrl = result.imageUrl.ToString() });
    }
    [Authorize]
    [HttpPost("upload")]
    
    public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile file)
    {
        dynamic user = "";
        dynamic bucketName = "";
        dynamic fileName = "";
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value ;  
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            

             user = await _context.LoadAsync<User>(email);
            if (user == null) return NotFound("User not found.");

             bucketName = "s32030";
             fileName = $"{email}_{file.FileName}";

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0; // Reset stream position

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream =  file.OpenReadStream(),
                Key = fileName,
                BucketName = bucketName
                //ContentType = file.ContentType
            };

            try
            {
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

              

                user.ProfileImageUrl = $"https://{bucketName}.s3.amazonaws.com/{fileName}";
                await _context.SaveAsync(user);
            }
            catch (AmazonS3Exception ex)
            {
             
                return StatusCode(500, "Internal server error: Unable to upload image.");
            }

            return Ok(new { ImageUrl = user.ProfileImageUrl + " content-Type=" + file.ContentType });
        
        
    }


[HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User model)
    {


        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.PasswordHash))
        {
            return BadRequest("Email and password are required.");
        }

        dynamic token = "";
        string userEmail = "";
        string password = "";


        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("YourVeryStrongKey123!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
        new Claim("sub", "test@example.com"),
        new Claim("email", "test@example.com")
    }),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = "Osama",
            Audience = "People",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
         token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);
        Console.WriteLine(jwt);




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