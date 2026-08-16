using AbsurdCourt.Application.Common;
using AbsurdCourt.Application.Features.Matches.NextRound;
using AbsurdCourt.Application.Features.Matches.RequestRematch;
using AbsurdCourt.Application.Features.Matches.StartMatch;
using AbsurdCourt.Application.Features.Matches.SubmitDefense;
using AbsurdCourt.Application.Features.Rooms.CreateRoom;
using AbsurdCourt.Application.Features.Rooms.Disconnect;
using AbsurdCourt.Application.Features.Rooms.JoinRoom;
using AbsurdCourt.Application.Features.Rooms.Rejoin;
using AbsurdCourt.Application.Features.Rooms.UpdateSettings;
using AbsurdCourt.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AbsurdCourt.Api.RealTime;

/// <summary>
/// Deliberately thin: every method just maps a client call onto a Command and lets
/// MediatR run it. The one thing only the Hub can do — SignalR group membership and
/// per-connection identity — lives here; everything else is Application/Domain.
/// </summary>
public sealed class CourtHub(ISender sender, PlayerSessionStore sessions, ILogger<CourtHub> logger) : Hub
{
    private const string RoomIdKey = "RoomId";
    private const string PlayerIdKey = "PlayerId";

    public Task<CreateRoomResult> CreateRoom(string hostName) => Guarded(async () =>
    {
        EnsureNotBound();
        var result = await sender.Send(new CreateRoomCommand(hostName, Context.ConnectionId));
        sessions.Set(GetSessionId(), result.Room.RoomCode, result.ReconnectToken);
        await RememberIdentityAsync(result.Room.RoomId, result.YourPlayerId);
        return result;
    });

    public Task<JoinRoomResult> JoinRoom(string roomCode, string playerName) => Guarded(async () =>
    {
        EnsureNotBound();
        var result = await sender.Send(new JoinRoomCommand(roomCode, playerName, Context.ConnectionId));
        sessions.Set(GetSessionId(), result.Room.RoomCode, result.ReconnectToken);
        await RememberIdentityAsync(result.Room.RoomId, result.YourPlayerId);
        return result;
    });

    public Task<RejoinResult> Rejoin(string roomCode) => Guarded(async () =>
    {
        if (!sessions.TryGet(GetSessionId(), out var session) ||
            !string.Equals(session.RoomCode, roomCode, StringComparison.OrdinalIgnoreCase))
            throw new HubException("A sessão de reconexão não foi encontrada.");

        var result = await sender.Send(new RejoinCommand(roomCode, session.ReconnectToken, Context.ConnectionId));
        sessions.Set(GetSessionId(), result.Room.RoomCode, result.ReconnectToken);
        if (result.PreviousConnectionId is not null && result.PreviousConnectionId != Context.ConnectionId)
            await Groups.RemoveFromGroupAsync(result.PreviousConnectionId, GroupNames.ForRoom(result.Room.RoomId));
        await RememberIdentityAsync(result.Room.RoomId, result.YourPlayerId);
        return result;
    });

    public Task UpdateSettings(int caseCount, int roundDurationSeconds) => Guarded(() =>
        sender.Send(new UpdateSettingsCommand(GetRoomId(), GetPlayerId(), caseCount, roundDurationSeconds, Context.ConnectionId)));

    public Task StartMatch() => Guarded(() =>
        sender.Send(new StartMatchCommand(GetRoomId(), GetPlayerId(), Context.ConnectionId)));

    public Task SubmitDefense(string text) => Guarded(() =>
        sender.Send(new SubmitDefenseCommand(GetRoomId(), GetPlayerId(), text, Context.ConnectionId)));

    public Task NextRound() => Guarded(() =>
        sender.Send(new NextRoundCommand(GetRoomId(), GetPlayerId(), Context.ConnectionId)));

    public Task RequestRematch(bool wantsRematch) => Guarded(() =>
        sender.Send(new RequestRematchCommand(GetRoomId(), GetPlayerId(), wantsRematch, Context.ConnectionId)));

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(RoomIdKey, out var roomId) && Context.Items.TryGetValue(PlayerIdKey, out var playerId))
            await sender.Send(new DisconnectCommand((Guid)roomId!, (Guid)playerId!, Context.ConnectionId));

        await base.OnDisconnectedAsync(exception);
    }

    private async Task RememberIdentityAsync(Guid roomId, Guid playerId)
    {
        if (Context.Items.TryGetValue(RoomIdKey, out var previousRoom) && previousRoom is Guid previousRoomId && previousRoomId != roomId)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.ForRoom(previousRoomId));

        Context.Items[RoomIdKey] = roomId;
        Context.Items[PlayerIdKey] = playerId;
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.ForRoom(roomId));
    }

    private void EnsureNotBound()
    {
        if (Context.Items.ContainsKey(RoomIdKey) || Context.Items.ContainsKey(PlayerIdKey))
            throw new HubException("Esta conexÃ£o jÃ¡ estÃ¡ vinculada a uma sala.");
    }

    private string GetSessionId()
    {
        var request = Context.GetHttpContext()?.Request;
        if (Guid.TryParse(request?.Query[PlayerSessionCookieMiddleware.QueryParameterName], out var sessionId))
            return sessionId.ToString("N");

        return request?.Cookies[PlayerSessionCookieMiddleware.CookieName]
            ?? throw new HubException("A sessão do navegador não foi inicializada.");
    }

    private Guid GetRoomId() =>
        Context.Items.TryGetValue(RoomIdKey, out var v) ? (Guid)v! : throw new HubException("Você ainda não entrou em uma sala.");

    private Guid GetPlayerId() =>
        Context.Items.TryGetValue(PlayerIdKey, out var v) ? (Guid)v! : throw new HubException("Você ainda não entrou em uma sala.");

    /// <summary>
    /// Expected, user-facing failures (domain invariant violations, "room not found", "not
    /// the host", a lost CloseRound race) become a clean HubException message for the
    /// caller. Anything else is a real bug — let SignalR's default masking handle it rather
    /// than leaking internals.
    /// </summary>
    private async Task<T> Guarded<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (IsExpected(ex))
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error invoking hub method for connection {ConnectionId}", Context.ConnectionId);
            throw new HubException("O servidor não conseguiu concluir a operação.");
        }
    }

    private async Task Guarded(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex) when (IsExpected(ex))
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error invoking hub method for connection {ConnectionId}", Context.ConnectionId);
            throw new HubException("O servidor não conseguiu concluir a operação.");
        }
    }

    private static bool IsExpected(Exception ex) =>
        ex is DomainException or RoomNotFoundException or MatchNotFoundException or NotHostException or ConnectionNotAuthorizedException or ConcurrencyConflictException;
}
