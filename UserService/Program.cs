using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Services;


var builder = WebApplication.CreateBuilder(args);

//Grpc server
builder.Services.AddGrpc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine)
);

// User service
builder.Services.AddScoped<IUserService, CUserService>();
// JWT service
builder.Services.AddScoped<JwtService>();

// Controllers (REST)
builder.Services.AddControllers();

var jwtsettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication().AddJwtBearer(Option =>
{
    Option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtsettings["Issuer"],
        ValidAudience = jwtsettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtsettings["Key"]))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// gRPC
app.MapGrpcService<UserGrpcService>();

// REST controllers
app.MapControllers();

app.Run();
