using FruitSlicer.Code.Components;
using FruitSlicer.Code.Views;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class SliceFruitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _fruitFilter;
        private EcsPool<Fruit> _fruitPool;
        private EcsPool<FruitPart> _fruitPartPool;
        private EcsPool<SliceData> _sliceDataPool;
        
        private const float InitialRigidbodyForce = 5f;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _fruitFilter = _world.Filter<Fruit>().Inc<SliceData>().End();
            _fruitPool = _world.GetPool<Fruit>();
            _fruitPartPool = _world.GetPool<FruitPart>();
            _sliceDataPool = _world.GetPool<SliceData>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _fruitFilter)
            {
                ref var fruit = ref _fruitPool.Get(entity);
                var fruitView = fruit.obj.GetComponent<FruitView>();

                ref var sliceData = ref _sliceDataPool.Get(entity);

                var parts = MeshSlice.MeshSlicer.Slice(fruit.obj.gameObject, sliceData.slicePlane, fruitView.material,
                    fruitView.sliceMaterial);

                if (parts != null)
                {
                    foreach (var partObj in parts)
                    {
                        var partEntity = _world.NewEntity();
                        ref var part = ref _fruitPartPool.Add(partEntity);
                        part.obj = partObj;
                            
                        var rb = partObj.AddComponent<Rigidbody>();
                        rb.AddForceAtPosition(Random.insideUnitSphere * InitialRigidbodyForce, Random.insideUnitSphere, ForceMode.Impulse);
                    }
                        
                    GameObject.Destroy(fruit.obj);
                    _fruitPool.Del(entity);
                }
            }
        }
    }
}