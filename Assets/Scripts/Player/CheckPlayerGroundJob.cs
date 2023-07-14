using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(GhostOwnerIsLocal))]
public partial struct CheckPlayerGroundJob : IJobEntity
{
    [ReadOnly]
    public PhysicsWorldSingleton PhysicsWorld;

    public void Execute(RefRW<PlayerInputComponent> playerInput, RefRO<LocalTransform> transform)
    {
        RaycastInput input = new RaycastInput()
        {
            Start = transform.ValueRO.Position + new float3(0.0f, 0.5f, 0.0f),
            End = transform.ValueRO.Position + new float3(0.0f, -0.6f, 0.0f),
            Filter = new CollisionFilter()
            {
                BelongsTo = 1 << 6,
                CollidesWith = 1 << 7,
                GroupIndex = 0
            }
        };

        if (PhysicsWorld.CastRay(input, out RaycastHit _))
        {
            playerInput.ValueRW.IsGrounded = true;
        }
        else
        {
            playerInput.ValueRW.IsGrounded = false;
        }
    }
}