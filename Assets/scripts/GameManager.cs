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
    public float PlayerCurrentMaxHealth{ get; set; }

    [Header("玩家進度數據")]
    public int playerLevel { get; private set; }
    public float playerExp { get; private set; }
    public float playerMaxExp { get; private set; }
    public Dictionary<StatType, float> playerStats { get; private set; }
    public List<string> unlockedSkills { get; private set; }
    public Dictionary<string, float> skillEffects { get; private set; }  // 新增：保存技能效果
    public Dictionary<string, int> equippedItems { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerStats = new Dictionary<StatType, float>();
            unlockedSkills = new List<string>();
            skillEffects = new Dictionary<string, float>();
            equippedItems = new Dictionary<string, int>();
            if (SceneManager.GetActiveScene().name == "MainMenu")
                InitializePlayerData();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return; // 關鍵：馬上 return，避免執行 Start/Update
        }

        uiManager = FindAnyObjectByType<UIManager>();
        playerController = FindAnyObjectByType<PlayerController>();
        Time.timeScale = 1;
        StartCoroutine(CheckEnemiesRoutine());
    }

    private void InitializePlayerData()
    {
        playerLevel = 1;
        playerExp = 0;
        playerMaxExp = 100;
        playerStats[StatType.AttackPower] = 10;
        playerStats[StatType.Defense] = 5;
        playerStats[StatType.CritRate] = 0.05f;
        playerStats[StatType.MoveSpeed] = 5;
        playerStats[StatType.MaxHealth] = 100;
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
        isInCombat = false;
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null && uiManager.gameOverMenu != null)
            uiManager.ShowGameOverMenu();
        Time.timeScale = 0;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null && uiManager.gamePauseMenu != null)
            uiManager.ShowGamePauseMenu();
        isPaused = true;
        isInCombat = false;
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
        Time.timeScale = 1; // 確保重新開始時遊戲是正常狀態
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1;
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            if (uiManager.gameOverMenu != null)
                uiManager.gameOverMenu.SetActive(false);
            if (uiManager.gamePauseMenu != null)
                uiManager.gamePauseMenu.SetActive(false);
        }
        isPaused = false;
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
        var player = PlayerController.Instance;
        var stats = player.GetComponent<PlayerStats>();
        var exp = player.GetComponent<PlayerExperience>();
        var health = player.GetComponent<Health>();
        var equipment = player.GetComponent<Equipment>();
        
        PlayerCurrentHealth = health.currentHealth;
        PlayerCurrentMaxHealth = health.maxHealth;
        playerLevel = exp.currentLevel;
        playerExp = exp.currentExp;
        playerMaxExp = exp.maxExp;
        playerStats[StatType.AttackPower] = stats.attackPower.Value;
        playerStats[StatType.Defense] = stats.defense.Value;
        playerStats[StatType.CritRate] = stats.critRate.Value;
        playerStats[StatType.MoveSpeed] = stats.moveSpeed.Value;
        playerStats[StatType.MaxHealth] = stats.maxHealth.Value;
        
        // 保存裝備數據
        if (equipment != null)
        {
            var currentEquipment = equipment.GetCurrentEquipment();
            if (currentEquipment != null)
            {
                equippedItems.Clear();
                equippedItems[currentEquipment.itemName] = 1; // 使用裝備名稱作為鍵
                Debug.Log($"【GameManager】保存當前裝備：{currentEquipment.itemName}");
            }
        }
        
        // 技能
        var skillUpgrades = player.GetComponent<SkillUpgrades>();
        if (skillUpgrades != null)
            unlockedSkills = skillUpgrades.GetUnlockedSkills();
    }

    //載入數據
    public void LoadData()
    {
        var player = PlayerController.Instance;
        var stats = player.GetComponent<PlayerStats>();
        var health = player.GetComponent<Health>();
        var expSystem = player.GetComponent<PlayerExperience>();
        var equipment = player.GetComponent<Equipment>();
        
        // 同步最大生命值
        health.maxHealth = PlayerCurrentMaxHealth;
        // 同步當前生命值
        health.currentHealth = PlayerCurrentHealth;
        health.OnHealthUpdate?.Invoke(health.maxHealth, health.currentHealth);
        // 同步屬性
        stats.SetAllStats(playerLevel, playerExp, playerMaxExp, playerStats);
        // 同步經驗與等級
        if (expSystem != null)
            expSystem.SetExpAndLevel(playerLevel, playerExp, playerMaxExp);
            
        // 載入裝備
        if (equipment != null)
        {
            equipment.LoadEquippedItems(equippedItems);
            Debug.Log($"【GameManager】載入裝備數據，裝備數量：{equippedItems.Count}");
        }
        
        // 技能
        var skillUpgrades = player.GetComponent<SkillUpgrades>();
        if (skillUpgrades != null)
            skillUpgrades.LoadUnlockedSkills(unlockedSkills);
            
        UICoinCountText.UpdateText(coinCount);
        
    }
}

