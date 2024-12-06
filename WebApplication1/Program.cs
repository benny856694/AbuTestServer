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
                    rep = new FaceReply(
                        "ACK",
                        "face",
                        0,
                        req.cap_time,
                        req.sequence_no,
                        new Data(
                            is_output_on_device: Random.Shared.Next(0, 2) is 1, // if popup windows will be shown
                            match_success: Random.Shared.Next(0, 2) is 1,
                            personName: "Jon Done",
                            personId: "123",
                            profileImage: profileImageBase64,
                            remarks: "Some Remarks"));
                    sb.AppendLine(@$"timezone: ""{req.timezone}"""); //newly added
                    sb.AppendLine(
                        $"face upload, replay: is_output_on_device: {(rep as FaceReply)!.data.is_output_on_device}");
                }



                break;
            default:
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
    string reply,
    string cmd,
    int code,
    string cap_time,
    int sequence_no,
    Data data
);

public record Data(
    bool is_output_on_device, //if show popup window on device
    bool match_success,
    string personName,
    string personId,
    string profileImage,
    string remarks
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





