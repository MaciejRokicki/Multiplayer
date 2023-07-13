using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
public partial struct PlayerMovementJob : IJobEntity
{
    void Execute(ref PlayerInputComponent cubeInput, ref PlayerComponent playerComponent, ref PhysicsVelocity physicsVelocity, ref PhysicsMass physicsMass)
    {
        if (cubeInput.IsGrounded)
        {
            if (cubeInput.MovementDirection.y != 0.0f)
            {
                physicsVelocity.Linear.y = cubeInput.MovementDirection.y * playerComponent.JumpForce;
            }
        }

        physicsVelocity.Linear.x = cubeInput.MovementDirection.x * playerComponent.MovementSpeed;
        physicsVelocity.Linear.z = cubeInput.MovementDirection.z * playerComponent.MovementSpeed;

        physicsMass.InverseInertia = float3.zero;
    }
}
