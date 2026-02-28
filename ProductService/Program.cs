using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Model;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductService, CProductService>(); 
builder.Services.AddGrpc();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate(); // ensure DB is created

    // DataSeeder.Seed(context); // seed initial data

}


// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapGrpcService<ProductGrpcService>();
app.MapControllers();

app.Run();
