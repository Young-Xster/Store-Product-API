using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineShopApi.Data;
using DotNetEnv;

// LOAD THE ENV FILE FIRST - This is the missing piece!
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Now the environment variable should be available
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Check if connection string was loaded
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found in environment variables!");
}

// Rest of your code
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use the connection string directly here
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add CORS for React app running on port 5173
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Use CORS before routing
app.UseCors("ReactAppPolicy");

app.UseAuthorization();

app.MapControllers();

// Optional: Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Uncomment when you have migrations or use EnsureCreated for development
    // context.Database.Migrate();
    // OR for development/testing:
    context.Database.EnsureCreated();
}

app.Run();