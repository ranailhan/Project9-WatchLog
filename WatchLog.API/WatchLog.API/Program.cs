using Microsoft.Data.SqlClient;
using QuestPDF.Infrastructure;
using System.Data;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// IDbConnection - Dapper için
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("WatchLogDB")));

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WatchLog API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WatchLog API v1"));

app.UseCors("AllowMVC");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
