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

    private bool hasStarted = false;

    public void StartSpawning()
    {
        if (hasStarted) return;
        hasStarted = true;
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found!");
            return;
        }
        _currentWaveIndex = 0;
        _allWavesCompleted = false;
        StartCoroutine(NextWaveCoroutine());
    }
    
    private IEnumerator NextWaveCoroutine()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex - 1 < waves.Length)
        {
            _currentWave = waves[_currentWaveIndex - 1];
            
            // 處理當前波次中的每個敵人元素
            foreach (WaveElement element in _currentWave.elements)
            {
                // 生成指定數量的特定敵人類型
                for (int i = 0; i < element.count; i++)
            {
                int spawnIndex = Random.Range(0, spawnPoints.Length);
                    Enemy spawnedEnemy = Instantiate(element.enemy, spawnPoints[spawnIndex].position, Quaternion.identity);
                    spawnedEnemy.playerTarget = this.enemy.transform;
                
                // 確保新生成的怪物使用正確的時間縮放
                    if (spawnedEnemy.GetComponent<Animator>() != null)
                {
                        spawnedEnemy.GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
                    }
                    
                    yield return new WaitForSeconds(element.timeBetweenSpawn);
                }
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

  