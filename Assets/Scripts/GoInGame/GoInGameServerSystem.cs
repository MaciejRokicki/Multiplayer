using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> _networkIdFromEntity;
    private int _playerCount;
    private EntityQuery _playerQuery;
    private EntityQuery _autoPlayerQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder goInGameRequestQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequest>()
            .WithAll<ReceiveRpcCommandRequest>();

        EntityQueryBuilder playersQueryBuilder = new EntityQueryBuilder(Allocator.TempJob)
            .WithAll<PlayerComponent>();

        EntityQueryBuilder autoPlayersQueryBuilder = new EntityQueryBuilder(Allocator.TempJob)
            .WithAll<PlayerComponent, AutoPlayerComponent>();

        state.RequireForUpdate(state.GetEntityQuery(goInGameRequestQueryBuilder));
        state.RequireForUpdate<PlayerSpawnerComponent>();

        _networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
        _playerQuery = state.GetEntityQuery(playersQueryBuilder);
        _autoPlayerQuery = state.GetEntityQuery(autoPlayersQueryBuilder);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerPrefab = SystemAPI.GetSingleton<PlayerSpawnerComponent>().PlayerPrefab;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        _networkIdFromEntity.Update(ref state);

        new ProcessNewConnectionJob
        {
            Ecb = ecb,
            Players = _playerQuery.ToEntityArray(Allocator.TempJob),
            AutoPlayers = _autoPlayerQuery.ToEntityArray(Allocator.TempJob),
            NetworkIdLookup = _networkIdFromEntity,
            PlayerPrefab = playerPrefab
        }.Schedule();

        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
    }
}