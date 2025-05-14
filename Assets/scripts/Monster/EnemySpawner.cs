using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
public class EnemySpawner : MonoBehaviour
{
    [Header("怪物重生點")]
    [SerializeField] private Transform[] spawnPoints;
    [Header("敵人(玩家)")]
    [SerializeField] private PlayerController enemy;
    [Header("波次")]
    public Wave[] waves;
    private Wave _currentWave;
    private int _currentWaveIndex;
    public bool _allWavesCompleted = false;


     
    
    
    private void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return;
        }
        _currentWaveIndex = 0; // 初始化為0，在协程中会增加到1
        StartCoroutine(NextWaveCoroutine());
    }
    
    private IEnumerator NextWaveCoroutine()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex - 1 < waves.Length)
        {
            _currentWave = waves[_currentWaveIndex - 1];
            for (int i = 0; i < _currentWave.count; i++)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Length);
                Enemy enemy = Instantiate(_currentWave.enemy, spawnPoints[spawnIndex].position, Quaternion.identity);
                enemy.target = this.enemy.transform;
                yield return new WaitForSeconds(_currentWave.timeBetweenSpawn);
            }
            
            // 检查是否还有下一波敌人需要生成
            if (_currentWaveIndex < waves.Length)
            {
                StartCoroutine(NextWaveCoroutine());
            }
            else
            {
                _allWavesCompleted = true;
            }
        }
        else
        {
            _allWavesCompleted = true;
        }
    }
    
    // 檢查是否還有剩餘波次或者當前波次未完成
    public bool HasRemainingWaves()
    {
        // 如果所有波次都已完成
        if (_allWavesCompleted)
            return true;
        
        // 如果還有未觸發的波次
        if (_currentWaveIndex < waves.Length)
            return true;
        
        // 檢查當前波次是否全部生成完畢
        // 這裡可以添加一個標記或計數器來跟踪當前波次的敵人是否全部生成
        return false;
    }
}

  