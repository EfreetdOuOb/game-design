using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private PlayerController enemy;
    public Wave[] waves;
    private Wave _currentWave ;
    private int _currentWaveIndex;
    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return;
        }
        StartCoroutine(NextWaveCoroutine());
    }
    
    private IEnumerator NextWaveCoroutine()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex -1 < waves.Length)
        {
            _currentWave = waves[_currentWaveIndex - 1];
            for (int i = 0; i < _currentWave.count; i++)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Length);
                Enemy enemy = Instantiate(_currentWave.enemy, spawnPoints[spawnIndex].position, Quaternion.identity);
                enemy.target = this.enemy.transform;
                yield return new WaitForSeconds(_currentWave.timeBetweenSpawn);
            }

        }
    }

}

  