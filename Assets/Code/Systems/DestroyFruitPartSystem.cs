using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class DestroyFruitPartSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _fruitPartFilter;
        private EcsPool<FruitPart> _fruitPartPool;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _fruitPartFilter = _world.Filter<FruitPart>().End();
            _fruitPartPool = _world.GetPool<FruitPart>();
        }
        
        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _fruitPartFilter)
            {
                ref FruitPart fruitPart = ref _fruitPartPool.Get(entity);
                if (fruitPart.obj.transform.position.y < -10f)
                {
                    GameObject.Destroy(fruitPart.obj);
                    _fruitPartPool.Del(entity);
                }
            }
        }
    }
}