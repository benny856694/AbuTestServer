using Scalar.AspNetCore;
using System.Diagnostics;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpLogging(cfg =>
{
    cfg.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.None;
    cfg.LoggingFields =
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestHeaders
    | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody
    | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;
    cfg.RequestHeaders.Add("x-api-key");
    cfg.CombineLogs = true;

});

builder.Services.AddSingleton<Stats>();
builder.Services.AddHostedService<UptimeCounterService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); //add test UI, open at url: "/scalar/v1"
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            var url = $"{app.Urls.First()}/scalar/v1";
            var info = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(info);
        });
    });
}


//app.UseHttpLogging();

var profileImageBase64 = Convert.ToBase64String(File.ReadAllBytes("profile.jpg"));

app.MapGet("/stats", async (Stats s, HttpContext c) =>
{
    var uptime = s.CurrentTime - s.StartTime;
    c.Response.Headers["Content-Type"] = "text/html";
    await c.Response.WriteAsync($"<p><b>Uptime:</b> {uptime.ToString(@"dd\.hh\:mm\:ss")}</p>");
});


app.Run();




