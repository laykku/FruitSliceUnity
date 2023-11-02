using FruitSlicer.Code.Systems;
using Leopotam.EcsLite;
using UnityEngine;

namespace FruitSlicer.Code
{
    public class Startup : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        
        private EcsWorld _world;
        private EcsSystems _updateSystems;
        
        private void Start()
        {
            _world = new EcsWorld();
            _updateSystems = new EcsSystems(_world, gameConfig);
            
            _updateSystems
                .Add(new InitGameSystem())
                .Add(new SpawnFruitSystem())
                .Add(new MoveFruitSystem())
                .Add(new InputSystem())
                .Add(new DetectSliceSystem())
                .Add(new SliceFruitSystem())
                .Add(new DestroyFruitSystem())
                .Add(new DestroyFruitPartSystem())
                .Init();
        }

        private void Update()
        {
            _updateSystems?.Run();
        }

        private void OnDestroy()
        {
            _updateSystems?.Destroy();
            _world?.Destroy();
        }
    }
}