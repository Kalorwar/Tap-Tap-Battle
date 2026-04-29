using UnityEngine;

namespace _Project.Scripts.Core
{
    public class SeedService
    {
        private const string SeedSaveKey = "EnemySpawnSeed";
        private int _currentSeed;

        public SeedService()
        {
            Load();
        }

        public int CurrentSeed => _currentSeed;

        public void GenerateNewSeed()
        {
            _currentSeed = Random.Range(int.MinValue, int.MaxValue);
            Save();
        }

        public void SetSeed(int seed)
        {
            _currentSeed = seed;
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SeedSaveKey, _currentSeed);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            _currentSeed = PlayerPrefs.GetInt(SeedSaveKey, Random.Range(int.MinValue, int.MaxValue));
        }
    }
}