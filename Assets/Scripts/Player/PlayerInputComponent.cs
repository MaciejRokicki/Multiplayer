using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInputComponent : IInputComponentData
{
    public float3 MovementDirection;
    public bool IsGrounded;
}
