using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(GoInGameRequest))]
public partial struct ProcessNewConnectionJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
    public NativeArray<Entity> Players;
    public NativeArray<Entity> AutoPlayers;
    [ReadOnly]
    public ComponentLookup<NetworkId> NetworkIdLookup;
    public Entity PlayerPrefab;

    public void Execute(Entity reqEntity, in ReceiveRpcCommandRequest reqSrc)
    {
        if (Players.Length == 0)
        {
            Ecb.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);

            Entity firstPlayer = Ecb.Instantiate(PlayerPrefab);
            Entity secondPlayer = Ecb.Instantiate(PlayerPrefab);

            Ecb.SetComponent(firstPlayer, LocalTransform.FromPosition(new float3(-1.0f, 1.0f, 0.0f)));
            Ecb.SetComponent(secondPlayer, LocalTransform.FromPosition(new float3(1.0f, 1.0f, 0.0f)));

            Ecb.AddComponent(secondPlayer, new AutoPlayerComponent());

            LinkNetworkIdToPlayer(Ecb, reqSrc.SourceConnection, firstPlayer, NetworkIdLookup[reqSrc.SourceConnection]);
        }
        else
        {
            if (AutoPlayers.Length != 0)
            {
                Ecb.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);

                Ecb.RemoveComponent<AutoPlayerComponent>(AutoPlayers[0]);

                LinkNetworkIdToPlayer(Ecb, reqSrc.SourceConnection, AutoPlayers[0], NetworkIdLookup[reqSrc.SourceConnection]);
            }
        }

        Ecb.DestroyEntity(reqEntity);
    }

    private void LinkNetworkIdToPlayer(EntityCommandBuffer ecb, Entity reqSrc, Entity player, NetworkId networkId)
    {
        ecb.SetComponent(player, new GhostOwner
        {
            NetworkId = networkId.Value
        });

        ecb.AppendToBuffer(reqSrc, new LinkedEntityGroup
        {
            Value = player
        });
    }
}