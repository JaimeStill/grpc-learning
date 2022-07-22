using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient;

using var channel = GrpcChannel.ForAddress("https://localhost:7234");
var client = new App.AppClient(channel);
var request = new PingRequest { Id = 1, Name = Environment.MachineName };

var reply = client.Ping(request);

Console.WriteLine($"Ping: {reply.Message}");
await Task.Delay(TimeSpan.FromMilliseconds(500));

PingRequestPayload pings = new();
pings.Requests.AddRange(new List<PingRequest>
{
    new PingRequest { Id = 1, Name = "PingPC-01" },
    new PingRequest { Id = 2, Name = "PingPC-02" },
    new PingRequest { Id = 3, Name = "PingPC-03" }
});

using var serverCall = client.ServerPingStream(pings);

await foreach (var serverReply in serverCall.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"ServerStreamPing {serverReply.Message}");
}

using var clientCall = client.ClientPingStream();

foreach (var ping in pings.Requests)
{
    await clientCall.RequestStream.WriteAsync(ping);
}

await clientCall.RequestStream.CompleteAsync();

var clientResult = await clientCall;

foreach (var clientReply in clientResult.Replies)
{
    Console.WriteLine($"ClientStreamPing {clientReply.Message}");
    await Task.Delay(TimeSpan.FromMilliseconds(500));
}

using var streamCall = client.PingStream();

foreach (var ping in pings.Requests)
{
    await streamCall.RequestStream.WriteAsync(ping);
}

await streamCall.RequestStream.CompleteAsync();

await foreach (var streamReply in streamCall.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"PingStream {streamReply.Message}");
    await Task.Delay(TimeSpan.FromMilliseconds(500));
}