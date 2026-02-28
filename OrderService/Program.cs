using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols =
            HttpProtocols.Http1AndHttp2;
    });
});

// -------------------- CORE ASP.NET --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

// -------------------- gRPC CLIENTS --------------------
builder.Services.AddGrpcClient<CartService.Grpc.CartService.CartServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:CartService"]);
});

builder.Services.AddGrpcClient<UserService.Grpc.UserProfileService.UserProfileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:UserService"]);
});

builder.Services.AddGrpcClient<ProductService.Grpc.ProductService.ProductServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcServices:ProductService"]);
});

// -------------------- Kafka --------------------
// Bind Kafka settings from appsettings.json
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

// Add Kafka consumer as hosted service
builder.Services.AddHostedService<KafkaConsumerService>();

// -------------------- DATABASE --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// -------------------- APPLICATION SERVICES --------------------
builder.Services.AddScoped<IOrderService, COrderService>();

// -------------------- AUTHENTICATION & AUTHORIZATION --------------------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication().AddJwtBearer(Option =>
{
    Option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt["Key"]))
    };
});
builder.Services.AddAuthorization();

// -------------------- LOGGING --------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// -------------------- MIDDLEWARE PIPELINE --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<OrderGrpcService>();
app.MapControllers();

app.Run();
