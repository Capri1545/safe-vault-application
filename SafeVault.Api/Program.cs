var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure for minimal API development

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add development-specific middleware or endpoints here if needed
}

app.UseHttpsRedirection();

// Add authentication and authorization middleware for robust access control
app.UseAuthentication();
app.UseAuthorization();

app.Run();
