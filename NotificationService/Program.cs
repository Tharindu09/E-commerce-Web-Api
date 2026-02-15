using dotenv.net;
using NotificationService.Service;
using NotificationService.Services;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// -------------------- Kafka --------------------
// Bind Kafka settings from appsettings.json
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

// Add Kafka consumer as hosted service
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<INotificationService, SmtpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();

