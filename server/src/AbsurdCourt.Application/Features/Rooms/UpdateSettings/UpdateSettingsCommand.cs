using MediatR;

namespace AbsurdCourt.Application.Features.Rooms.UpdateSettings;

public sealed record UpdateSettingsCommand(Guid RoomId, Guid RequestedByPlayerId, int CaseCount, int RoundDurationSeconds, string? ConnectionId = null) : IRequest;
