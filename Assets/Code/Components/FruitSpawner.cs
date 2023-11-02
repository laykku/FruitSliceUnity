using UnityEngine;

namespace FruitSlicer.Code.Components
{
    struct FruitSpawner
    {
        public GameObject[] prefabs;
        public float spawnRate;
        public float nextSpawnTime;
    }
}