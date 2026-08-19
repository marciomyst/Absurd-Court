using AbsurdCourt.Domain.Common;

namespace AbsurdCourt.Domain.Events;

public sealed record PlayerJoined(Guid RoomId, Guid PlayerId, string PlayerName, string Initials, bool IsHost) : IDomainEvent;

public sealed record PlayerReconnected(Guid RoomId, Guid PlayerId) : IDomainEvent;

public sealed record PlayerDisconnected(Guid RoomId, Guid PlayerId) : IDomainEvent;

public sealed record PlayerLeft(Guid RoomId, Guid PlayerId) : IDomainEvent;

public sealed record RoomSettingsChanged(Guid RoomId, int CaseCount, int RoundDurationSeconds) : IDomainEvent;

public sealed record RematchStatusChanged(Guid RoomId, int ReadyCount, int TotalConnectedPlayers, bool AllReady) : IDomainEvent;

public sealed record RoomClosed(Guid RoomId, string Reason) : IDomainEvent;
