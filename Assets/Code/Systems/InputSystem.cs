using FruitSlicer.Code.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code.Systems
{
    public class InputSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsPool<SliceInputEvent> _sliceInputEventPool;
        
        private bool _mousePressed;
        private Vector2 _pressPos;
        private float _pressTs;
        
        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _sliceInputEventPool = _world.GetPool<SliceInputEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mousePressed = true;
                _pressPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                _pressTs = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _mousePressed = false;
            }

            if (_mousePressed)
            {
                var viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                if (Vector2.Distance(viewportPos, _pressPos) > 0.25f && Time.time - _pressTs < 1f)
                {
                    _mousePressed = false;
                    var inputEventEntity = _world.NewEntity();
                    ref var sliceEvt = ref _sliceInputEventPool.Add(inputEventEntity);
                    sliceEvt.startPoint = new Vector3(_pressPos.x, _pressPos.y, -10f);
                    sliceEvt.endPoint = new Vector3(viewportPos.x, viewportPos.y, -10f);
                }
            }
        }
    }
}