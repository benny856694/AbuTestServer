using Scalar.AspNetCore;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseHttpsRedirection();

var profileImageBase64 = Convert.ToBase64String(File.ReadAllBytes("profile.jpg"));

app.MapPost("/upload/record", (ILogger<Program> logger, Face req, HttpRequest request) =>
    {

        var sb = new StringBuilder();
        sb.AppendLine($"Request received ({request.HttpContext.Connection.RemoteIpAddress})");
        sb.AppendLine("headers:");
        foreach (var header in request.Headers)
        {
            sb.AppendLine($"\t{header.Key}: {header.Value}");
        }

        object rep = "unknown request";
        switch (req.cmd)
        {
            case "heart beat":
                rep = "any text is ok";
                sb.AppendLine($"heart beat");
                break;
            case "face":
                {
                    var faceReply = new FaceReply
                    (
                        req.sequence_no,
                        req.cap_time,
                        new GatewayControl(),
                        new Data(
                            is_output_on_device: Random.Shared.Next(0, 2) is 1, // if popup windows will be shown
                            match_success: Random.Shared.Next(0, 2) is 1,
                            personName: "Jon Done",
                            personId: "123",
                            profileImage: profileImageBase64,
                            remarks: "Some Remarks"),
                        new TextDisplay(
                            new Position(0, 100),
                            1000,
                            100,
                            1,
                            "0xffffffff",
                            "WELCOME!"
                            )
                        );

                    rep = faceReply;
                    
                    sb.AppendLine(@$"timezone: ""{req.timezone}"""); //newly added
                    sb.AppendLine(
                        $"face upload, replay: {nameof(faceReply.data.is_output_on_device)}: {faceReply.data?.is_output_on_device}");
                }
                break;
            default:
                sb.AppendLine(@$"Unknown request: cmd: '{req.cmd}'");
                rep = Results.Problem($"Unknown cmd: '{req.cmd}'", statusCode: 400);
                break;

        }

        logger.LogInformation(sb.ToString());

        return rep;
    })
    .WithName("UploadRecord");

app.Run();

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





