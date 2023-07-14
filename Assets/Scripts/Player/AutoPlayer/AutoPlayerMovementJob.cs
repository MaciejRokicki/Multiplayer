using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(AutoPlayerComponent))]
public partial struct AutoPlayerMovementJob : IJobEntity
{
    [ReadOnly]
    public LocalTransform Player;

    public void Execute(RefRW<LocalTransform> transform, RefRO<PlayerComponent> playerComponent, RefRW<PhysicsVelocity> physicsVelocity)
    {
        if (math.distance(Player.Position, transform.ValueRO.Position) > 1.5f)
        {
            float3 dir = math.normalize(Player.Position - transform.ValueRO.Position);

            physicsVelocity.ValueRW.Linear.x = dir.x * playerComponent.ValueRO.MovementSpeed;
            physicsVelocity.ValueRW.Linear.z = dir.z * playerComponent.ValueRO.MovementSpeed;

            float y = math.atan2(dir.x, dir.z);
            transform.ValueRW.Rotation = quaternion.EulerXYZ(new float3(0.0f, y, 0.0f));
        }
    }
}