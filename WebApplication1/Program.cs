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

#region models

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedPositionalProperty.Global
public record FaceReply(
    int sequence_no,
    string cap_time,
    GatewayControl? gateway_ctrl = null, //door control
    Data? data = null,
    TextDisplay? text_display = null, //display single-line text on screen
    TextDisplay[]? text_displays = null, //multiline text display
    string cmd = "face",
    string reply = "ACK",
    int code = 0
);

public record Data(
    bool is_output_on_device, //if show popup window on device
    bool match_success,
    string personName,
    string personId,
    string profileImage,
    string remarks
);

public record GatewayControl(
    string device_type = "gpio",
    string ctrl_type = "on",
    int device_no = 1, //1 or 2
    string ctrl_mode = "force"
);

public record TextDisplay(
    Position position,
    int alive_time, //active time in millisecond
    int font_size, //100
    int font_spacing, //1
    string font_color, //ARGB, e.g. "0xffffffff"
    string text
);

public record Position(
    int x,
    int y
);


public record GeneralRequest(string cmd);


public record Face(
    string addr_name,
    string addr_no,
    string cap_time,
    Closeup_pic closeup_pic,
    bool closeup_pic_flag,
    string cmd,
    string device_no,
    string device_sn,
    int is_realtime,
    Match match,
    int match_failed_reson,
    int match_result,
    bool overall_pic_flag,
    Person person,
    int sequence_no,
    string version,
    bool video_flag,
    string timezone //newly added 
);

public record Closeup_pic(
    string data,
    int face_height,
    int face_width,
    int face_x,
    int face_y,
    string format
);

public record Match(
    string customer_text,
    string format,
    string image,
    bool is_encryption,
    object match_type,
    string origin,
    string person_attr,
    string person_id,
    string person_name,
    int person_role,
    int wg_card_id
);

public record Person(
    int age,
    int face_quality,
    bool has_mask,
    string hat,
    int rotate_angle,
    string sex,
    float temperatur,
    int turn_angle,
    int wg_card_id
);



// ReSharper restore InconsistentNaming
// ReSharper restore NotAccessedPositionalProperty.Global

#endregion



