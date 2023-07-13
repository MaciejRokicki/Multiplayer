using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ConnectionStateSystem : ISystem
{
    private EntityQuery _networkStreamConnectionQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder networkStreamConnectionQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkStreamConnection>()
            .WithNone<ConnectionState>();

        _networkStreamConnectionQuery = state.GetEntityQuery(networkStreamConnectionQueryBuilder);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        ecb.AddComponent(_networkStreamConnectionQuery, new ConnectionState());

        new ConnectionStateJob
        {
            Ecb = ecb,
            PlayerComponentLookup = SystemAPI.GetComponentLookup<PlayerComponent>(true)
        }.Schedule();
    }
}