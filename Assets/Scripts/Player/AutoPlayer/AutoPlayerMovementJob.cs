using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(AutoPlayerComponent))]
public partial struct AutoPlayerMovementJob : IJobEntity
{
    public LocalTransform Player;
    public void Execute( RefRO<LocalTransform> transform, RefRO<PlayerComponent> playerComponent, RefRW<PhysicsVelocity> physicsVelocity)
    {
        if (math.distance(Player.Position, transform.ValueRO.Position) > 1.5f)
        {
            float3 dir = math.normalize(Player.Position - transform.ValueRO.Position);

            physicsVelocity.ValueRW.Linear.x = dir.x * playerComponent.ValueRO.MovementSpeed;
            physicsVelocity.ValueRW.Linear.z = dir.z * playerComponent.ValueRO.MovementSpeed;
        }
    }
}