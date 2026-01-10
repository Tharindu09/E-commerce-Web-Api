using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Service;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddSingleton<KafkaProducerService>();
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
builder.Services.AddControllers();

var app = builder.Build();

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();


app.Run();

