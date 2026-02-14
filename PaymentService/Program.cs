using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Service;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddScoped<PaymentService.Service.PaymentService>();
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();

// -------------------- gRPC CLIENTS --------------------
builder.Services.AddGrpcClient<OrderService.Grpc.OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7143");

});

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
builder.Services.AddControllers();

var app = builder.Build();

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();

