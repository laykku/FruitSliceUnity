using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class SpawnFruitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        
        private EcsFilter _fitler;
        private EcsPool<FruitSpawner> _spawnerPool;
        private EcsPool<Fruit> _fruitPool;
        
        public void Init(IEcsSystems systems)
        {
            
            _world = systems.GetWorld();
            _fitler = _world.Filter<FruitSpawner>().End();
            _spawnerPool = _world.GetPool<FruitSpawner>();
            _fruitPool = _world.GetPool<Fruit>();
        }
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _fitler)
            {
                ref FruitSpawner spawner = ref _spawnerPool.Get(entity);
                if (Time.time >= spawner.nextSpawnTime)
                {
                    var fruitEntity = _world.NewEntity();
                    ref var fruitComp = ref _fruitPool.Add(fruitEntity);
                    fruitComp.obj = GameObject.Instantiate(spawner.prefabs[Random.Range(0, spawner.prefabs.Length)]);
                    fruitComp.position = new Vector3(0f, 8f, 0f);
                    fruitComp.spinAngle = Random.insideUnitSphere;
                    spawner.nextSpawnTime = Time.time + spawner.spawnRate;
                }
            }
        }
    }
}