using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class DetectSliceSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _inputEventFilter;
        private EcsFilter _fruitFilter;
        private EcsPool<SliceInputEvent> _inputEventPool;
        private EcsPool<Fruit> _fruitPool;
        private EcsPool<SliceData> _sliceDataPool;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _inputEventFilter = _world.Filter<SliceInputEvent>().End();
            _inputEventPool = _world.GetPool<SliceInputEvent>();
            _fruitFilter = _world.Filter<Fruit>().Exc<SliceData>().End();
            _fruitPool = _world.GetPool<Fruit>();
            _sliceDataPool = _world.GetPool<SliceData>();
        }

        private const float SliceDistanceSquared = 9f;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _inputEventFilter)
            {
                ref var sliceEvt = ref _inputEventPool.Get(entity);
                var startWorld = Camera.main.ViewportToWorldPoint(sliceEvt.startPoint);
                startWorld.z = 0f;
                var endWorld = Camera.main.ViewportToWorldPoint(sliceEvt.endPoint);
                endWorld.z = 0f;

                Vector3 normal = Vector3.Cross((startWorld - endWorld).normalized, Vector3.forward);
                Vector3 pos = (startWorld + endWorld) / 2f;
                var plane = new Plane(normal, pos);
                
                foreach (var fruitEntity in _fruitFilter)
                {
                    ref var fruit = ref _fruitPool.Get(fruitEntity);
                    if (Vector3.SqrMagnitude(fruit.position - pos) > SliceDistanceSquared) continue;
                    ref var sliceData = ref _sliceDataPool.Add(fruitEntity);
                    sliceData.slicePlane = plane;
                }
                _inputEventPool.Del(entity);
            }
        }
    }
}