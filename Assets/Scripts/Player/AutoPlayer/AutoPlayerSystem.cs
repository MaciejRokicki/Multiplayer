using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(GoInGameServerSystem))]
public partial struct AutoPlayerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AutoPlayerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        new CheckPlayerGroundJob
        {
            PhysicsWorld = physicsWorld
        }.Schedule();

        if (SystemAPI.HasSingleton<AutoPlayerComponent>())
        {
            new AutoPlayerMovementJob
            {
                Player = SystemAPI.GetComponent<LocalTransform>(
                SystemAPI.QueryBuilder()
                .WithAll<PlayerComponent>()
                .WithNone<AutoPlayerComponent>()
                .Build()
                .GetSingletonEntity()
            )
            }.Schedule();
        }
    }
}