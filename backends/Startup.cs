using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backends;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Add AWS services
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddAWSService<IAmazonS3>();
        services.AddSingleton<S3Service>();
        services.AddSingleton<CustomJwtService>();
        services.AddSingleton<IAmazonS3, AmazonS3Client>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Osama",
            ValidAudience = "People",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourVeryStrongKey123!"))
        };
    });
    //    // Add JWT Authentication
    //    services.AddAuthentication(options =>
    //    {
    //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //    })
    //    .AddJwtBearer(options =>
    //    {
    //        options.TokenValidationParameters = new TokenValidationParameters
    //        {
    //            ValidateIssuer = true,
    //            ValidateAudience = true,
    //            ValidateLifetime = true,
    //            ValidateIssuerSigningKey = true,
    //            ValidIssuer = "Osama", // Ensure these values are in appsettings.json
    //            ValidAudience = "people",
    //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourVeryStrongKey123!"))
    //        };
    //    });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        // Add authentication before authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}
