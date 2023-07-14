using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class PlayerInputSystem : SystemBase, DefaultControlMap.IPlayerActions
{
    private DefaultControlMap _controlMap;

    protected override void OnStartRunning() => _controlMap.Enable();

    protected override void OnStopRunning() => _controlMap.Disable();

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerComponent>();

        _controlMap = new DefaultControlMap();
        _controlMap.Player.SetCallbacks(this);
    }

    protected override void OnUpdate()
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        new CheckPlayerGroundJob
        {
            PhysicsWorld = physicsWorld
        }.Schedule();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        foreach (RefRW<PlayerInputComponent> playerInput in SystemAPI.Query<RefRW<PlayerInputComponent>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.MovementDirection = context.ReadValue<Vector3>();
        }
    }
}
