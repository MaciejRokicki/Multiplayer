using Unity.Entities;
using UnityEngine;

public class PlayerSpawnerAuthoring : MonoBehaviour
{
    public GameObject PlayerPrefab;

    public class PlayerSpawnerBaker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerSpawnerComponent
            {
                PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
