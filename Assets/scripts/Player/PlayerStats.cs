using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // 添加UI命名空間

[Serializable]
public class Stat
{
    public float baseValue;     // 基礎值
    public float equipBonus;    // 裝備加成
    public float upgradeBonus;  // 升級加成

    public float Value => baseValue + equipBonus + upgradeBonus;
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("玩家屬性")]
    public Stat attackPower;    // 攻擊力
    public Stat defense;        // 防禦力
    public Stat critRate;       // 暴擊率（0-1之間的值）
    public Stat moveSpeed;      // 移動速度
    public Stat maxHealth;      // 最大生命值

    [Header("等級系統")]
    public int playerLevel = 1;     // 玩家等級
    public float currentExp = 0;    // 當前經驗值
    public float maxExp = 100;      // 升級所需經驗值

    [Header("UI引用")]
    public Text attackText;     // 攻擊力顯示
    public Text defenseText;    // 防禦力顯示
    public Text critRateText;   // 暴擊率顯示
    public Text moveSpeedText;  // 移動速度顯示
    public Text maxHealthText;  // 最大生命值顯示

    [Header("事件")]
    public UnityEvent OnStatsChanged;
    public UnityEvent<int, float, float> OnExpChanged; // 等級, 當前經驗, 最大經驗

    private PlayerController playerController;
    private Health playerHealth;
    private PlayerAttackManager playerAttackManager; // 添加玩家攻擊管理器引用

    private void Awake()
    {
        Instance = this;
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<Health>();
        playerAttackManager = GetComponent<PlayerAttackManager>(); // 獲取玩家攻擊管理器

        // 初始化屬性
        InitializeStats();
    }

    private void Start()
    {
        // 初始化時更新一次UI
        UpdateUI();
        
        // 註冊事件監聽
        OnStatsChanged.AddListener(UpdateUI);
        
        // 應用移動速度到玩家控制器
        ApplyMoveSpeed();
    }

    // 初始化屬性
    private void InitializeStats()
    {
        // 從PlayerAttackManager獲取基礎攻擊力
        if (playerAttackManager != null)
        {
            attackPower.baseValue = playerAttackManager.GetBaseAttackDamage();
            Debug.Log($"從PlayerAttackManager獲取基礎攻擊力: {attackPower.baseValue}");
        }
        else
        {
            // 如果沒有找到PlayerAttackManager，使用預設值
            attackPower.baseValue = 10f;
            Debug.LogWarning("無法找到PlayerAttackManager，使用預設攻擊力: 10");
        }
        
        defense.baseValue = 5f;
        critRate.baseValue = 0.05f;  // 5%基礎暴擊率
        moveSpeed.baseValue = 5f;    // 基礎移動速度
        maxHealth.baseValue = 100f;  // 基礎最大生命值
    }

    // 獲取屬性文字描述
    public string GetStatsDescription()
    {
        return $"攻擊力: {attackPower.Value:F1}\n" +
               $"防禦力: {defense.Value:F1}\n" +
               $"暴擊率: {critRate.Value * 100:F1}%\n" +
               $"移動速度: {moveSpeed.Value:F1}\n" +
               $"最大生命值: {maxHealth.Value:F1}";
    }
    
    // 更新UI顯示
    private void UpdateUI()
    {
        if(attackText) attackText.text = $"攻擊力: {attackPower.Value:F1}";
        if(defenseText) defenseText.text = $"防禦力: {defense.Value:F1}";
        if(critRateText) critRateText.text = $"暴擊率: {critRate.Value*100:F1}%";
        if(moveSpeedText) moveSpeedText.text = $"移動速度: {moveSpeed.Value:F1}";
        if(maxHealthText) maxHealthText.text = $"最大生命值: {maxHealth.Value:F1}";
    }

    // 更新裝備加成
    public void UpdateEquipmentBonus(float attack, float def, float crit, float speed, float health = 0f)
    {
        attackPower.equipBonus = attack;
        defense.equipBonus = def;
        critRate.equipBonus = crit;
        moveSpeed.equipBonus = speed;
        maxHealth.equipBonus = health;

        ApplyStats();
    }

    // 增加升級加成
    public void AddUpgradeBonus(StatType statType, float amount)
    {
        switch (statType)
        {
            case StatType.AttackPower:
                attackPower.upgradeBonus += amount;
                break;
            case StatType.Defense:
                defense.upgradeBonus += amount;
                break;
            case StatType.CritRate:
                critRate.upgradeBonus += amount;
                break;
            case StatType.MoveSpeed:
                moveSpeed.upgradeBonus += amount;
                break;
            case StatType.MaxHealth:
                maxHealth.upgradeBonus += amount;
                // 更新玩家當前生命值
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(amount);
                }
                break;
        }

        ApplyStats();
    }

    // 應用屬性效果到玩家
    private void ApplyStats()
    {
        // 應用移動速度
        ApplyMoveSpeed();
        
        // 更新UI顯示
        UpdateUI();

        // 通知UI更新
        OnStatsChanged?.Invoke();
    }

    // 應用移動速度
    private void ApplyMoveSpeed()
    {
        if (playerController != null)
        {
            playerController.moveSpeed = moveSpeed.Value;
        }
    }

    // 計算是否暴擊
    public bool RollForCritical()
    {
        return UnityEngine.Random.value <= critRate.Value;
    }

    // 計算實際傷害 (攻擊者攻擊力與被攻擊者防禦力的計算)
    public float CalculateDamage(float baseDamage, float targetDefense)
    {
        float attackValue = attackPower.Value;
        
        // 計算防禦減免，防禦越高，減免越多，但有上限
        float defenseReduction = targetDefense / (targetDefense + 100f);
        
        // 計算基礎傷害
        float damage = baseDamage * attackValue * (1f - defenseReduction);
        
        // 暴擊檢定 - 移除暴擊傷害計算，暴擊傷害現在由PlayerAttackManager計算
        // 如果這裡需要暴擊判定，使用RollForCritical方法
        
        return Mathf.Max(1f, damage);  // 至少造成1點傷害
    }

    // 計算承受傷害 (考慮自身防禦力)
    public float CalculateDamageTaken(float incomingDamage)
    {
        // 計算防禦減免，防禦越高，減免越多，但有上限
        float defenseReduction = defense.Value / (defense.Value + 100f);
        
        // 計算最終受到的傷害
        float finalDamage = incomingDamage * (1f - defenseReduction);
        
        return Mathf.Max(1f, finalDamage);  // 至少受到1點傷害
    }

    // 接收攻擊力更新
    public void UpdateAttackFromManager(int newBaseAttack)
    {
        // 更新基礎攻擊力
        attackPower.baseValue = newBaseAttack;
        
        // 應用屬性變化
        ApplyStats();
        
        Debug.Log($"攻擊力已從PlayerAttackManager更新: {newBaseAttack}");
    }
    
    // 讓其他腳本可以獲取當前攻擊力
    public float GetCurrentAttackPower()
    {
        return attackPower.Value;
    }

    // 設置等級
    public void SetLevel(int level)
    {
        playerLevel = level;
        OnStatsChanged?.Invoke();
    }

    // 設置經驗值
    public void SetExp(float exp, float maxExp)
    {
        currentExp = exp;
        this.maxExp = maxExp;
        OnExpChanged?.Invoke(playerLevel, currentExp, maxExp);
    }

    // 增加經驗值
    public void AddExp(float exp)
    {
        currentExp += exp;
        while (currentExp >= maxExp)
        {
            LevelUp();
        }
        OnExpChanged?.Invoke(playerLevel, currentExp, maxExp);
    }

    // 升級
    private void LevelUp()
    {
        playerLevel++;
        currentExp -= maxExp;
        maxExp *= 1.2f; // 每次升級所需經驗值增加20%

        // 升級獎勵
        AddUpgradeBonus(StatType.AttackPower, 2);
        AddUpgradeBonus(StatType.Defense, 1);
        AddUpgradeBonus(StatType.MaxHealth, 10);

        OnStatsChanged?.Invoke();
        OnExpChanged?.Invoke(playerLevel, currentExp, maxExp);
    }

    // 設置所有屬性
    public void SetAllStats(int level, float exp, float maxExp, Dictionary<StatType, float> stats)
    {
        playerLevel = level;
        currentExp = exp;
        this.maxExp = maxExp;
        if (stats != null)
        {
            if (stats.ContainsKey(StatType.AttackPower)) attackPower.baseValue = stats[StatType.AttackPower];
            if (stats.ContainsKey(StatType.Defense)) defense.baseValue = stats[StatType.Defense];
            if (stats.ContainsKey(StatType.CritRate)) critRate.baseValue = stats[StatType.CritRate];
            if (stats.ContainsKey(StatType.MoveSpeed)) moveSpeed.baseValue = stats[StatType.MoveSpeed];
            if (stats.ContainsKey(StatType.MaxHealth)) maxHealth.baseValue = stats[StatType.MaxHealth];
        }
        ApplyStats();
        OnStatsChanged?.Invoke();
        OnExpChanged?.Invoke(playerLevel, currentExp, this.maxExp);
    }
}

// 屬性類型枚舉
public enum StatType
{
    AttackPower,
    Defense,
    CritRate,
    MoveSpeed,
    MaxHealth    // 添加最大生命值選項
} 