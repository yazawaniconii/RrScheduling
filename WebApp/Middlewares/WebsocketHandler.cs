using System.Net.WebSockets;
using System.Text;
using RobinRound;
using WebApp.Models;

namespace WebApp.Middlewares;

public class WebsocketHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public WebsocketHandler(
        RequestDelegate next,
        ILoggerFactory loggerFactory
    )
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<WebsocketHandler>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                // TODO
                await Handle(webSocket);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }

    private static async Task Handle(WebSocket ws)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await ws.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!receiveResult.CloseStatus.HasValue)
        {
            var receiveString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            if (receiveString == "GetAllInfo")
            {
                var sentJson = await GetInfo.GetAllInfo();
                var sentBytes = Encoding.UTF8.GetBytes(sentJson);
                await ws.SendAsync(
                    new ArraySegment<byte>(sentBytes),
                    WebSocketMessageType.Text,
                    WebSocketMessageFlags.EndOfMessage,
                    CancellationToken.None);
            }
            else if (receiveString[..4] == "List")
            {
                var listString = receiveString[4..];
                var bytes = Encoding.UTF8.GetBytes(listString);
                var stream = new MemoryStream(bytes);
                var list = await Utils.Parser(new StreamReader(stream));
                // if (list != null) Scheduling.Instant.InitQueue(list);
                if (list != null) Scheduling.Instant.AddProcesses(list);
            }
            else if (receiveString == "Pause")
            {
                Scheduling.Instant.IsPause = true;
            }
            else if (receiveString == "Start")
            {
                Scheduling.Instant.IsPause = false;
            }
            else if (receiveString == "Cleanup")
            {
                Scheduling.Instant.Cleanup();
            }
            else if (receiveString.Substring(0, 9) == "TimeSlice")
            {
                if (Utils.IsNumeric(receiveString[9..]))
                {
                    Scheduling.Instant.TimeSlice = int.Parse(receiveString[9..]);
                    // Console.WriteLine(receiveString[9..]);
                }
            }

            receiveResult = await ws.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await ws.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}