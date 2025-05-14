using UnityEngine;

public class MonsterAttackManager : AttackManager
{
    [Header("怪物攻擊設定")]
    public LayerMask playerLayer;  // 玩家層級遮罩
    
    [Header("怪物傷害設定")]
    [SerializeField] private int additionalDamage = 0;    // 額外傷害（來自怪物等級、狀態等）
    [SerializeField] private float damageMultiplier = 1f; // 傷害倍率
    
    [Header("攻擊動畫設定")]
    [Tooltip("攻擊動畫中觸發傷害的時間點（0-1）")]
    [Range(0f, 1f)]
    public float attackDamageFrame = 0.5f; // 默認在動畫中間造成傷害
    
    private Animator animator;
    private Monster monster;
    private bool attackAnimationPlaying = false;
    private string attackAnimationName = "attack"; // 儲存攻擊動畫名稱，便於檢查

    private void Awake()
    {
        animator = GetComponent<Animator>();
        monster = GetComponent<Monster>();
        
        // 確保攻擊點存在
        if (attackPoint == null)
        {
            Debug.LogWarning(gameObject.name + " 沒有設置攻擊點！將使用怪物中心點作為攻擊點。");
            // 創建一個默認的攻擊點
            GameObject newAttackPoint = new GameObject("AttackPoint");
            newAttackPoint.transform.SetParent(transform);
            newAttackPoint.transform.localPosition = Vector3.zero; // 默認在怪物中心
            attackPoint = newAttackPoint.transform;
        }
    }

    private void Update()
    {
        // 如果正在攻擊並且還沒有造成傷害，檢查動畫進度
        if (isAttacking && !hasDamaged && attackAnimationPlaying)
        {
            CheckAttackAnimation();
        }
    }

    // 檢查攻擊動畫進度，在適當時機觸發傷害
    private void CheckAttackAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(attackAnimationName) && stateInfo.normalizedTime >= attackDamageFrame)
            {
                // 當動畫進度達到指定幀時觸發攻擊
                AttackTrigger();
            }
            
            // 如果動畫已經播放完成，結束這次攻擊並開始冷卻計時
            if (stateInfo.normalizedTime >= 1.0f)
            {
                attackAnimationPlaying = false;
                
                // 開始攻擊冷卻
                if (monster != null)
                {
                    monster.StartAttackCooldown();
                }
                
                // 注意：我們不在這裡調用StopAttacking，因為這應該由狀態機來控制
            }
        }
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
        
        // 在攻擊前讓怪物面向玩家
        if (target != null && monster != null && monster.spriteRend != null)
        {
            float x = target.position.x - transform.position.x;
            if (x > 0)
            {
                monster.spriteRend.flipX = false; // 假設精靈默認面向右側
            }
            else
            {
                monster.spriteRend.flipX = true; // 面向左側
            }
        }
        
        // 重置傷害標誌，允許新的攻擊造成傷害
        hasDamaged = false;
        attackAnimationPlaying = true;
        
        // 強制設置動畫的當前時間為0，確保從頭開始播放
        if (animator != null)
        {
            animator.Play(attackAnimationName, 0, 0f);
        }
        
        // 注意：動畫播放已經移到Monster.Attack()方法中
    }

    public override void StopAttacking()
    {
        attackAnimationPlaying = false;
        base.StopAttacking();
    }

    public override void AttackTrigger()
    {
        // 只有在攻擊狀態且未造成傷害時才進行判定
        if (!isAttacking || hasDamaged || currentTarget == null) return;

        // 使用攻擊點而不是攻擊區域來判定
        Vector2 attackPos = GetAttackPosition();
        
        // 檢測攻擊範圍內的玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, attackRange, playerLayer);
        
        foreach (Collider2D hit in hits)
        { 
            // 獲取玩家生命值組件
            Health playerHealth = hit.GetComponent<Health>();
            
            // 檢查玩家是否存在且不處於無敵狀態
            if (playerHealth != null && !playerHealth.isInvincible)
            {
                // 計算最終傷害值
                int finalDamage = GetAttackDamage();
                
                // 對玩家造成傷害
                playerHealth.TakeDamage(finalDamage);
                
                // 標記已造成傷害，防止一次攻擊多次判定
                hasDamaged = true;
                
                Debug.Log($"{gameObject.name} 對玩家造成 {finalDamage} 點傷害");
            }
            else if (playerHealth != null && playerHealth.isInvincible)
            {
                // 玩家處於無敵狀態，攻擊無效
                Debug.Log($"{gameObject.name} 攻擊了無敵狀態的玩家，無效！");
                
                // 標記已造成傷害，防止之後的無效判定
                hasDamaged = true;
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

    // 在Unity編輯器中繪製攻擊範圍
    protected override void OnDrawGizmosSelected()
    {
        // 使用基類的繪製功能
        base.OnDrawGizmosSelected();
        
        // 額外繪製攻擊點
        Vector2 attackPos = Application.isPlaying ? GetAttackPosition() : (attackPoint != null ? attackPoint.position : transform.position);
        
        // 繪製一個較小的實心球體表示攻擊點
        Gizmos.color = new Color(1, 0, 0, 0.5f); // 半透明紅色
        Gizmos.DrawSphere(attackPos, 0.1f);
        
        // 繪製攻擊範圍
        Gizmos.color = new Color(1, 0, 0, 0.2f); // 更透明的紅色
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
} 