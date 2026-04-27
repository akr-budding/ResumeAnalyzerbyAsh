using ResumeAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Switch between real AI and mock via appsettings OpenAI:UseMock
var useMock = builder.Configuration.GetValue<bool>("OpenAI:UseMock");
if (useMock)
    builder.Services.AddScoped<IAIService, MockAIService>();
else
    builder.Services.AddHttpClient<IAIService, AIService>();

// CORS — allow Angular dev server (adjust origin for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
                  "http://localhost:4200",
                  "https://ashwiniresumeanalyzer.vercel.app"
              )
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── App pipeline ──────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
