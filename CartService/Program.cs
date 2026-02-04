using CartService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ICartService, CCartService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProductService:BaseUrl"]);
});

//grpc 
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<ProductService.Grpc.ProductService.ProductServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["ProductService:GrpcUrl"]);
});
builder.Services.AddScoped<ProductGrpcClient>();

// Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"))
);

// DI
builder.Services.AddScoped<ICartService, CCartService>();

// JWT Authentication
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<CartGrpcService>();

app.MapControllers();
app.Run();
