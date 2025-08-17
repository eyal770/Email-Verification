using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleEmail;
using EmailVerificationSystem.Infrastructure;
using EmailVerificationSystem.Repositories;
using EmailVerificationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8673", "http://127.0.0.1:8673", "file://")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure AWS services
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonSimpleEmailService>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// Configure application services
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Ensure DynamoDB table exists
await DynamoDbTableManager.EnsureEmailVerificationsTableExistsAsync(app.Services);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");

app.MapControllers();

// Serve static files for the frontend
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();