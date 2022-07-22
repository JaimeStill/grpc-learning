using Grpc.Core;

namespace GrpcServer.Services;

public class AppService : App.AppBase
{
    private readonly ILogger<AppService> _logger;
    public AppService(ILogger<AppService> logger)
    {
        _logger = logger;
    }

    static string ResolveRequest(PingRequest ping) => $"Resolved request from {ping.Id} {ping.Name}";

    public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingReply
        {
            Message = ResolveRequest(request)
        });
    }

    public override async Task ServerPingStream(PingRequestPayload request, IServerStreamWriter<PingReply> responseStream, ServerCallContext context)
    {
        foreach (var ping in request.Requests)
        {
            await responseStream.WriteAsync(new PingReply{ Message = ResolveRequest(ping) });
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }

    public override async Task<PingReplyPayload> ClientPingStream(IAsyncStreamReader<PingRequest> requestStream, ServerCallContext context)
    {
        PingReplyPayload reply = new();

        await foreach (var ping in requestStream.ReadAllAsync())
        {
            reply.Replies.Add(new PingReply { Message = ResolveRequest(ping) });
        }

        return reply;
    }

    public override async Task PingStream(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PingReply> responseStream, ServerCallContext context)
    {
        await foreach (var ping in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new PingReply { Message = ResolveRequest(ping) });
        }        
    }
}