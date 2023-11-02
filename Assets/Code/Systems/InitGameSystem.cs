using FruitSlicer.Code.Components;
using Leopotam.EcsLite;

namespace FruitSlicer.Code.Systems
{
    public class InitGameSystem : IEcsInitSystem
    {
        public void Init(IEcsSystems systems)
        {
            var config = systems.GetShared<GameConfig>();
            
            var world = systems.GetWorld();
            var spawnerEntity = world.NewEntity();
            var pool = world.GetPool<FruitSpawner>();
            ref var spawnerComp = ref pool.Add(spawnerEntity);
            spawnerComp.prefabs = config.fruitPrefabs;
            spawnerComp.spawnRate = 1f;
        }
    }
}