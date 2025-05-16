using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for local development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Initialize database on API startup
var configuration = builder.Configuration;
var dbInitializer = new DatabaseInitializer(configuration);
dbInitializer.Initialize();
builder.Services.AddSingleton<DatabaseInitializer>(dbInitializer);

// Add services to the container.
builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SuperSecretKey12345SuperSecretKey12345")
            ),
        };
    });

// Add controllers and Razor Pages support
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add development-specific middleware or endpoints here if needed
}

app.UseHttpsRedirection();

app.UseCors(); // Moved before authentication/authorization

app.UseAuthentication();
app.UseAuthorization();

// Map controller and Razor Pages endpoints
app.MapControllers();
app.MapRazorPages();

app.Run();
