using Microsoft.AspNetCore.SignalR;

namespace AbsurdCourt.Api.RealTime;

public sealed class InvocationRateLimitFilter(HubInvocationRateLimiter limiter) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken = default)
    {
        var ip = invocationContext.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()
            ?? invocationContext.Context.ConnectionId;
        var isRoomEntry = invocationContext.HubMethodName is "CreateRoom" or "JoinRoom" or "Rejoin";
        var limit = isRoomEntry ? 20 : 120;
        var key = $"{ip}:{(isRoomEntry ? "entry" : "game")}";

        if (!limiter.TryConsume(key, limit, TimeSpan.FromMinutes(1)))
            throw new HubException("Muitas solicitações. Tente novamente em instantes.");

        return await next(invocationContext, cancellationToken);
    }
}
