using Unity.Entities;

public struct PlayerComponent : IComponentData
{
    public float MovementSpeed;
    public float JumpForce;
}