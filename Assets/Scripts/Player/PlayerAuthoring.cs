using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public float MovementSpeed;
    public float JumpForce;

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlayerInputComponent());
            AddComponent(entity, new PlayerComponent
            {
                MovementSpeed = authoring.MovementSpeed,
                JumpForce = authoring.JumpForce
            });
        }
    }
}