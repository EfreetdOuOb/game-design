using UnityEngine;

public class PlayerAttackManager : AttackManager
{
    [Header("玩家攻擊設定")]
    public LayerMask enemyLayer;  // 敵人層級遮罩
    
    [Header("玩家傷害設定")]
    [SerializeField] private int additionalDamage = 0;    // 額外傷害（來自武器、技能等）
    [SerializeField] private float damageMultiplier = 1f; // 傷害倍率
    
    // 重寫獲取傷害值的方法
    public override int GetAttackDamage()
    {
        // 計算總傷害：(基礎傷害 + 額外傷害) * 倍率
        return Mathf.RoundToInt((baseAttackDamage + additionalDamage) * damageMultiplier);
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
} 