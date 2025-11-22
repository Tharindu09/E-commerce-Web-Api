using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(option => option.UseNpgsql(builder.Configuration.
                                                                        GetConnectionString("DefaultConnection")).
                                                                        EnableSensitiveDataLogging().
                                                                        LogTo(Console.WriteLine));
                                                                    
// 2. Add the UserService as a scoped service
builder.Services.AddScoped<IUserService, CUserService>();

// 3. Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers(); // Map controller routes

app.Run();

