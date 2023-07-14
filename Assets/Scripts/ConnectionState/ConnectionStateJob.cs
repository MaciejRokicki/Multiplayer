using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[BurstCompile]
public partial struct ConnectionStateJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
    [ReadOnly]
    public ComponentLookup<PlayerComponent> PlayerComponentLookup;

    public void Execute(Entity reqEntity, ConnectionState connectionState, ref DynamicBuffer<LinkedEntityGroup> linkedEntityGroup)
    {
        if (connectionState.CurrentState == ConnectionState.State.Disconnected)
        {
            if(linkedEntityGroup.Length == 2)
            {
                Entity playerEntity = linkedEntityGroup[1].Value;

                Ecb.AddComponent<AutoPlayerComponent>(playerEntity);

                for (int i = 0; i < linkedEntityGroup.Length; i++)
                {
                    if (PlayerComponentLookup.HasComponent(linkedEntityGroup[i].Value))
                    {
                        linkedEntityGroup.RemoveAt(i);
                        break;
                    }
                }

                Ecb.RemoveComponent<ConnectionState>(reqEntity);
            }
        }
    }
}