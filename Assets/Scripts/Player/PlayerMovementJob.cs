using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerMovementJob : IJobEntity
{
    void Execute(
        ref PlayerInputComponent playerInput, 
        ref PlayerComponent playerComponent, 
        ref PhysicsVelocity physicsVelocity, 
        ref PhysicsMass physicsMass, 
        ref LocalTransform transform)
    {
        if (playerInput.IsGrounded)
        {
            if (playerInput.MovementDirection.y != 0.0f)
            {
                physicsVelocity.Linear.y = playerInput.MovementDirection.y * playerComponent.JumpForce;
            }
        }

        physicsVelocity.Linear.x = playerInput.MovementDirection.x * playerComponent.MovementSpeed;
        physicsVelocity.Linear.z = playerInput.MovementDirection.z * playerComponent.MovementSpeed;

        if(playerInput.MovementDirection.x != 0.0f || playerInput.MovementDirection.z != 0.0f)
        {
            float y = math.atan2(playerInput.MovementDirection.x, playerInput.MovementDirection.z);
            transform.Rotation = quaternion.EulerXYZ(new float3(0.0f, y, 0.0f));
        }

        physicsMass.InverseInertia = float3.zero;
    }
}
