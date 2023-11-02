using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class DestroyFruitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _fruitFilter;
        private EcsPool<Fruit> _fruitPool;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _fruitFilter = _world.Filter<Fruit>().End();
            _fruitPool = _world.GetPool<Fruit>();
        }
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _fruitFilter)
            {
                ref Fruit fruit = ref _fruitPool.Get(entity);
                if (fruit.position.y < -10f)
                {
                    GameObject.Destroy(fruit.obj);
                    _fruitPool.Del(entity);
                }
            }
        }
    }
}