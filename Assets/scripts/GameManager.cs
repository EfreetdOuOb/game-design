using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;    
    public EnemySpawner enemySpawner;
    public bool isInCombat = false; // 戰鬥狀態標誌
    public bool isPaused = false;
    
    private float checkInterval = 1f; // 檢查間隔
    private float timer = 0f;

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        Time.timeScale = 1;
        StartCoroutine(CheckEnemiesRoutine());
    }

    void Update()
    {
        // 倒計時取消，使用 UIManager 中的正常計時
        
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            if (isPaused == false)
            { 
                PauseGame();
            }
            else 
            { 
                ResumeGame();
            }
        }
        
        // 檢查戰鬥狀態
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            CheckCombatStatus();
            timer = 0f;
        }
    }
    
    // 檢查戰鬥狀態的方法
    private void CheckCombatStatus()
    {
        // 如果遊戲暫停或玩家已死亡，則不處於戰鬥狀態
        if (isPaused)
        {
            isInCombat = false;
            return;
        }
        
        // 檢查玩家是否死亡
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && playerHealth.currentHealth <= 0)
            {
                isInCombat = false;
                return;
            }
        }
        else
        {
            // 找不到玩家，可能已死亡或場景未載入完成
            isInCombat = false;
            return;
        }
        
        // 檢查場景中是否還有敵人
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // 檢查是否還有波次可以生成
        bool hasMoreWaves = enemySpawner != null && enemySpawner.HasRemainingWaves();
        
        // 確認是否有敵人或還有波次，表示處於戰鬥狀態
        isInCombat = (enemies.Length > 0 || hasMoreWaves);
    }
    
    // 定期檢查敵人的協程
    private IEnumerator CheckEnemiesRoutine()
    {
        while (true)
        {
            CheckCombatStatus();
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    public void EndGame()
    { 
        isInCombat = false; // 遊戲結束時關閉戰鬥狀態
        uiManager.ShowGameOverMenu(); 
        Time.timeScale = 0; 
    }

    public void PauseGame()
    { 
        Time.timeScale = 0;
        uiManager.ShowGamePauseMenu();
        isPaused = true;
        isInCombat = false; // 遊戲暫停時關閉戰鬥狀態
    }

    public void PlayerScored(int score)
    {
        uiManager.IncreaseScore(score);
    }
    
    public void RestartGame()
    {
        // 載入當前場景以重新開始 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ResumeGame()
    { 
        Time.timeScale = 1;
        uiManager.gameOverMenu.SetActive(false);
        uiManager.gamePauseMenu.SetActive(false);
        isPaused = false;
        
        // 恢復時重新檢查戰鬥狀態
        CheckCombatStatus();
    }

    public void BackToMenu()//返回
    {
        SceneManager.LoadScene("MainMenu");
    }
}

