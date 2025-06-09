using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public GameObject uiShowText;

    public static GameManager Instance {get;private set;}
    public UIManager uiManager;    
    public EnemySpawner enemySpawner;
    public PlayerController playerController;
    public bool isInCombat = false; // 戰鬥狀態標誌
    public bool isPaused = false;
    
    [Header("戰鬥相關事件")]
    public UnityEvent OnEnemyKilled; // 擊殺敵人事件
    
    private float checkInterval = 1f; // 檢查間隔
    private float timer = 0f;
    public int coinCount{get; private set;}
    public float PlayerCurrentHealth{ get; set; }

    private void Awake()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        playerController = GetComponent<PlayerController>();
        Time.timeScale = 1;
        StartCoroutine(CheckEnemiesRoutine());

        if(Instance == null)
        {
            Instance = this;
        }else if(Instance != null)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);//加載新場景時候告訴unity不要銷毀該物件
    }

    void Start()
    {
        // 每次進入新場景都重新抓 UIManager、PlayerController
        uiManager = FindAnyObjectByType<UIManager>();
        playerController = FindAnyObjectByType<PlayerController>();
        Time.timeScale = 1;
        StartCoroutine(CheckEnemiesRoutine());
    }

    void Update()
    {
        // 倒計時取消，使用 UIManager 中的正常計時
        
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            // 檢查是否有其他選單開啟
            bool hasOtherMenuOpen = CheckOtherMenusOpen();
            
            // 如果有其他選單開啟，則不處理ESC
            if (hasOtherMenuOpen)
            {
                // 不執行暫停/恢復邏輯
                return;
            }
            
            // 只有在沒有其他選單開啟時才執行暫停/恢復邏輯
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

    // 檢查是否有其他選單開啟
    private bool CheckOtherMenusOpen()
    {
        // 檢查各種可能的選單
        
        // 查找玩家的SkillUpgrades組件
        SkillUpgrades skillUpgrades = FindAnyObjectByType<SkillUpgrades>();
        if (skillUpgrades != null && skillUpgrades.IsAnyPanelOpen())
        {
            return true;
        }
        
        // 查找玩家的Equipment組件
        Equipment equipment = FindAnyObjectByType<Equipment>();
        if (equipment != null && equipment.IsEquipmentPanelOpen())
        {
            return true;
        }
        
        // 如果需要檢查其他類型的選單，可以在這裡添加
        
        // 如果沒有找到開啟的選單，返回false
        return false;
    }

    public void ChangeCoins(int amount)
    {
        coinCount += amount;
        if(coinCount < 0)
        {
            coinCount = 0;
        }
        UICoinCountText.UpdateText(coinCount);//更新金幣UI文本
    }

    //提示數值
    public void ShowText(string str, Vector2 pos, Color color)
    {
        Vector2 screenPostion = Camera.main.WorldToScreenPoint(pos);
        GameObject text = Instantiate(uiShowText, screenPostion,Quaternion.identity);
        text.transform.SetParent(GameObject.Find("HUD").transform);//UI元素需要Canvas才能顯示
        text.GetComponent<UIShowText>().SetText(str, color);
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
        var player = FindAnyObjectByType<PlayerController>();
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
        
        // 檢查是否從戰鬥狀態轉為非戰鬥狀態
        bool wasInCombat = isInCombat;
        
        // 確認是否有敵人或還有波次，表示處於戰鬥狀態
        isInCombat = (enemies.Length > 0 || hasMoreWaves);
        
        // 如果剛從戰鬥狀態轉為非戰鬥狀態，並且沒有更多波次，則顯示房間完成 UI
        if (wasInCombat && !isInCombat && !hasMoreWaves)
        {
            RoomComplete();
        }
    }
    
    // 房間完成方法
    public void RoomComplete()
    {
        if (uiManager != null)
        {
            uiManager.ShowRoomCompleteMenu();
        }
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

    // 原來的加分方法，現在同時增加經驗值
    public void PlayerScored(int score)
    {
        uiManager.IncreaseScore(score);
        
        // 同時增加經驗值，可以設置不同的轉換比例
        int expGained = score; // 假設1分=1經驗值，可以根據需要調整
        uiManager.IncreaseExp(expGained);
    }
    
    // 新增純經驗值獲取方法
    public void PlayerGainedExp(int exp)
    {
        uiManager.IncreaseExp(exp);
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
    
    // 敵人被擊殺
    public void EnemyKilled(Monster monster)
    {
        // 觸發敵人擊殺事件，用於通知其他系統（如能量護盾）
        OnEnemyKilled?.Invoke();
        Debug.Log("敵人被擊殺，已觸發OnEnemyKilled事件");
    }

    //保存數據
    public void SaveData()
    {
        PlayerCurrentHealth = PlayerController.Instance.GetComponent<Health>().currentHealth;
    }
    //載入數據
}

