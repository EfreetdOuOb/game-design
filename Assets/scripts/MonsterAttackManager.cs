using UnityEngine;

public class MonsterAttackManager : AttackManager
{
    [Header("怪物攻擊設定")]
    public LayerMask playerLayer;  // 玩家層級遮罩
    
    [Header("怪物傷害設定")]
    [SerializeField] private int additionalDamage = 0;    // 額外傷害（來自怪物等級、狀態等）
    [SerializeField] private float damageMultiplier = 1f; // 傷害倍率
    
    private Animator animator;
    private Monster monster;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        monster = GetComponent<Monster>();
    }

    // 重寫獲取傷害值的方法
    public override int GetAttackDamage()
    {
        // 計算總傷害：(基礎傷害 + 額外傷害) * 倍率
        return Mathf.RoundToInt((baseAttackDamage + additionalDamage) * damageMultiplier);
    }

    public override void StartAttacking(Transform target)
    {
        base.StartAttacking(target);
        // 播放攻擊動畫
        monster.PlayAnimation("bite");
    }

    public override void StopAttacking()
    {
        base.StopAttacking();
    }

    public override void AttackTrigger()
    {
        // 只有在攻擊動畫播放中且未造成傷害時才進行判定
        if (!isAttacking || hasDamaged || currentTarget == null) return;
        
        // 檢測攻擊範圍內的玩家
        Collider2D hit = Physics2D.OverlapCircle(GetAttackPosition(), attackRange, playerLayer);
        if (hit != null)
        {
            Health playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.isInvincible)
            {
                int finalDamage = GetAttackDamage();
                playerHealth.TakeDamage(finalDamage);
                hasDamaged = true;
                Debug.Log($"{gameObject.name} 對玩家造成 {finalDamage} 點傷害（基礎:{baseAttackDamage} + 額外:{additionalDamage}) x {damageMultiplier}");
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