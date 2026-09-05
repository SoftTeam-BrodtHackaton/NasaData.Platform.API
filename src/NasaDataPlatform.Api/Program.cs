using NasaData.Platform.API.Shared.Domain.Repositories;
using NasaData.Platform.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using NasaData.Platform.API.Missions.Infrastructure;
using NasaData.Platform.API.Students.Infrastructure;
using NasaData.Platform.API.Missions.Application;
using NasaData.Platform.API.Missions.Domain.Model;


var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables().AddCommandLine(args);


builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMissions();
builder.Services.AddStudents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

if (connectionString == null) throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableDetailedErrors();
    else
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "NASA Data Platform API",
            Version = "v1",
            Description = "MVP educativo: misiones y progreso STEM",
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }

        });
});


var app = builder.Build();

// Verify if the database exists and create it if it doesn't
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
    if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Missions:EnableFixtures"))
    {
        var missions = services.GetRequiredService<MissionService>();
        foreach (var fixture in app.Configuration.GetSection("Missions:Fixtures").GetChildren())
            await missions.GenerateAsync(fixture["EventId"]!, Enum.Parse<MissionType>(fixture["Type"]!), Enum.Parse<Difficulty>(fixture["Difficulty"]!));
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/" && HttpMethods.IsGet(context.Request.Method))
        {
            context.Response.Redirect("swagger/index.html");
            return;
        }
        await next(context);
    });
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler();
app.UseCors("AllowAllPolicy");
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
