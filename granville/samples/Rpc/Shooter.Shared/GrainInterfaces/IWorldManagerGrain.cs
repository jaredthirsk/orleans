using Orleans;
using Shooter.Shared.Models;

namespace Shooter.Shared.GrainInterfaces;

public interface IWorldManagerGrain : Orleans.IGrainWithIntegerKey
{
    Task<ActionServerInfo> RegisterActionServer(string serverId, string ipAddress, int udpPort, string httpEndpoint, int rpcPort = 0);
    Task UnregisterActionServer(string serverId);
    Task<ActionServerInfo?> GetActionServerForPosition(Vector2 position);
    Task<List<ActionServerInfo>> GetAllActionServers();
    Task<PlayerInfo> RegisterPlayer(string playerId, string name);
    Task<Vector2> GetPlayerStartPosition(string playerId);
    Task ResetAllServerAssignments();
    
    // Zone transition support
    Task<PlayerTransferInfo?> InitiatePlayerTransfer(string playerId, Vector2 currentPosition);
    Task UpdatePlayerPosition(string playerId, Vector2 position);
    Task UpdatePlayerPositionAndVelocity(string playerId, Vector2 position, Vector2 velocity);
    
    // Chat message broadcasting
    Task BroadcastChatMessage(ChatMessage message);
    
    // Game round tracking
    Task NotifyGameOver();
    
    // Zone statistics streaming
    Task ReportZoneStats(GridSquare zone, ZoneStats stats);
    IAsyncEnumerable<GlobalZoneStats> StreamZoneStatistics(TimeSpan updateInterval);
}