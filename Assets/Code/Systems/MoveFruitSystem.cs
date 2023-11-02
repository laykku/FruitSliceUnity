using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class MoveFruitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _fruitFilter;
        private EcsPool<Fruit> _fruitPool;
        
        private const float RotationSpeed = 60f;
        
        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _fruitFilter = world.Filter<Fruit>().End();
            _fruitPool = world.GetPool<Fruit>();
        }
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _fruitFilter)
            {
                ref Fruit fruit = ref _fruitPool.Get(entity);
                fruit.position -= Vector3.up * Time.deltaTime * 5f;
                fruit.obj.transform.position = fruit.position;
                fruit.obj.transform.Rotate(fruit.spinAngle * Time.deltaTime * RotationSpeed);
            }
        }
    }
}