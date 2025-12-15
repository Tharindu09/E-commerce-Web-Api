using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<CartService.Grpc.CartService.CartServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7235"); // Cart service
});

builder.Services.AddGrpcClient<UserService.Grpc.UserProfileService.UserProfileServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7253"); // User service
});

builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();


