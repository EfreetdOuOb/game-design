using UnityEngine;

public class PlayerAttackManager : AttackManager
{
    [Header("玩家攻擊設定")]
    public LayerMask enemyLayer;  // 敵人層級遮罩
    
    [Header("玩家傷害設定")]
    [SerializeField] private int additionalDamage = 0;    // 額外傷害（來自武器、技能等）
    [SerializeField] private float damageMultiplier = 1f; // 傷害倍率
    [SerializeField] private float critDamageMultiplier = 1.5f; // 暴擊傷害倍率，設為150%
    
    private PlayerStats playerStats; // 玩家屬性引用
    
    private void Awake()
    {
        // 確保在 Awake 中獲取引用
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerAttackManager: 無法找到 PlayerStats 組件！");
        }
    }
    
    private void Start()
    {
        // 在啟動時同步基礎攻擊力到PlayerStats
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        SyncAttackPowerToStats();
    }
    
    // 同步基礎攻擊力到PlayerStats
    private void SyncAttackPowerToStats()
    {
        if (playerStats != null)
        {
            playerStats.UpdateAttackFromManager(baseAttackDamage);
            Debug.Log($"基礎攻擊力已同步到PlayerStats: {baseAttackDamage}");
        }
        else
        {
            Debug.LogError("PlayerAttackManager: 無法同步攻擊力，PlayerStats 為空！");
        }
    }
    
    // 當基礎攻擊力在Unity編輯器中改變時調用
    private void OnValidate()
    {
        // 僅在編輯器中運行時響應
        if (Application.isPlaying && playerStats != null)
        {
            SyncAttackPowerToStats();
        }
    }
    
    // 重寫獲取傷害值的方法
    public override int GetAttackDamage()
    {
        int baseDamage;
        
        // 直接使用 PlayerStats 中的攻擊力值
        if (playerStats != null)
        {
            baseDamage = Mathf.RoundToInt(playerStats.GetCurrentAttackPower());
        }
        else
        {
            // 如果無法獲取 PlayerStats，退回到原始計算方式
            baseDamage = baseAttackDamage;
        }
        
        // 應用傷害倍率
        int damage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        
        // 檢查是否暴擊
        if (playerStats != null && playerStats.RollForCritical())
        {
            damage = Mathf.RoundToInt(damage * critDamageMultiplier);
            Debug.Log($"暴擊！傷害從 {damage/critDamageMultiplier} 提升到 {damage}");
        }
        
        return damage;
    }

    public override void AttackTrigger()
    {
        // 只有在攻擊狀態且未造成傷害時才進行判定
        if (!isAttacking || hasDamaged || currentTarget == null) return;

        // 檢測攻擊範圍內的玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(GetAttackPosition(), attackRange,enemyLayer);
        foreach (Collider2D hit in hits)
        {
             
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null && !playerHealth.isInvincible)
                {
                    int finalDamage = GetAttackDamage();
                    playerHealth.TakeDamage(finalDamage);
                    hasDamaged = true;
                    Debug.Log($"{gameObject.name} 對玩家造成 {finalDamage} 點傷害");
                } 
        }
    }
    
    // 設置額外傷害
    public void SetAdditionalDamage(int damage)
    {
        additionalDamage = damage;
    }
    
    // 設置傷害倍率
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }
    
    // 增加額外傷害
    public void AddAdditionalDamage(int damage)
    {
        additionalDamage += damage;
    }
    
    // 增加傷害倍率（與現有倍率相乘）
    public void AddDamageMultiplier(float multiplier)
    {
        damageMultiplier *= multiplier;
    }

    // 設置基礎攻擊力
    public void SetBaseAttackDamage(int damage)
    {
        baseAttackDamage = damage;
        // 同步到 PlayerStats
        if (playerStats != null)
        {
            playerStats.SyncAttackPowerFromManager();
        }
    }
} 