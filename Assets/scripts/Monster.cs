using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    // 基本屬性
    public float moveSpeed = 1f;
    [Tooltip("怪物檢測玩家的範圍，超出此範圍怪物將回到閒置狀態")]
    public float detectionRange = 5f; // 檢測範圍
    [Tooltip("怪物攻擊玩家的範圍，進入此範圍怪物將停止移動並攻擊")]
    public float attackRange = 1.5f; // 攻擊範圍
    [Tooltip("怪物的初始生命值")]
    public float startingHealth = 30f; // 初始生命值 
    public float currentHealth; // 當前生命值
    
    // 組件引用
    protected Transform target;
    protected Rigidbody2D rb2d;
    protected Animator animator;
    protected SpriteRenderer spriteRend;
    protected AttackManager attackManager;
    protected GameManager gameManager;
    
    // 狀態機
    protected MonsterState currentState;
    
    // 無敵相關
    [Header("傷害效果")]
    [Tooltip("受傷後閃爍的次數")]
    [SerializeField] protected int numberOfFlashes = 1; // 閃爍次數
    [SerializeField] protected float flashDuration = 0.5f; // 閃爍持續時間
    
    public GameObject detectionArea; // 檢測範圍空物件
    public GameObject attackArea; // 攻擊範圍空物件
    
    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        attackManager = GetComponent<AttackManager>();
        
        // 設置剛體屬性，防止被玩家撞開
        if (rb2d != null)
        {
            rb2d.freezeRotation = true; // 凍結旋轉
            rb2d.mass = 10000f; // 設置極大質量
            rb2d.linearDamping = 10f; // 高阻力
            rb2d.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation; // 凍結位置和旋轉
        }
        
        // 在Awake中初始化當前血量，確保怪物一開始就有正確的血量
        currentHealth = startingHealth;
    }
    
    protected virtual void Start()
    {
        // 初始化其他組件
        gameManager = FindFirstObjectByType<GameManager>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 設置初始狀態為閒置
        SetCurrentState(GetIdleState());
        
        
    }
    
    protected virtual void Update()
    {
        // 更新當前狀態
        if (currentState != null)
        {
            currentState.Update();
        }
    }
    
    protected virtual void FixedUpdate()
    {
        // 更新物理
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState != null)
        {
            currentState.OnTriggerEnter2D(collision);
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentState != null)
        {
            currentState.OnTriggerStay2D(collision);
        }
    }
    
    // 設置當前狀態
    public void SetCurrentState(MonsterState state)
    {
        currentState = state;
    }
    
    // 各種子類必須實現的抽象方法
    protected abstract MonsterState GetIdleState();
    protected abstract MonsterState GetHurtState();
    protected abstract MonsterState GetDeadState();
    
    // 移動方法
    public virtual Vector2 MoveTowardsPlayer()
    {
        Vector2 direction = Vector2.zero; // 初始化方向
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
            // 只有在距離超過攻擊範圍時才會移動
            if (Vector2.Distance(target.position, transform.position) > attackRange)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        return direction; // 返回方向
    }
    
    // 停止移動
    public virtual void Stop()
    {
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
        }
    }
    
    // 攻擊方法
    public virtual void Attack()
    {
        if (attackManager != null && target != null)
        {
            attackManager.PerformAttack(target);
        }
    }
    
    // 檢查是否在範圍內
    public virtual bool IsPlayerInRange(float range)
    {
        if (target == null) return false;
        
        Vector3 checkPosition = transform.position;
        if (detectionArea != null && range == detectionRange)
        {
            checkPosition = detectionArea.transform.position;
        }
        else if (attackArea != null && range == attackRange)
        {
            checkPosition = attackArea.transform.position;
        }
        
        float distanceToPlayer = Vector2.Distance(target.position, checkPosition);
        
        return distanceToPlayer <= range;
    }
    
    // 檢查是否在攻擊範圍內
    public virtual bool IsPlayerInAttackRange()
    {
        Vector3 checkPosition = transform.position;
        if (attackArea != null)
        {
            checkPosition = attackArea.transform.position;
        }
        
        if (target == null) return false;
        
        float distanceToPlayer = Vector2.Distance(target.position, checkPosition);
        
        return distanceToPlayer <= attackRange;
    }
    
    // 檢查是否在檢測範圍內但在攻擊範圍外
    public virtual bool IsPlayerInDetectionRange()
    {
        Vector3 checkPosition = transform.position;
        Vector3 attackCheckPosition = transform.position;
        
        if (detectionArea != null)
        {
            checkPosition = detectionArea.transform.position;
        }
        
        if (attackArea != null)
        {
            attackCheckPosition = attackArea.transform.position;
        }
        
        if (target == null) return false;
        
        float distanceToPlayer = Vector2.Distance(target.position, checkPosition);
        float attackDistanceToPlayer = Vector2.Distance(target.position, attackCheckPosition);
        
        return distanceToPlayer <= detectionRange && attackDistanceToPlayer > attackRange;
    }
    
    // 獲取到玩家的距離
    public virtual float GetDistanceToPlayer()
    {
        if (target == null) return float.MaxValue;
        
        return Vector2.Distance(target.position, transform.position);
    }
    
    // 播放動畫
    public virtual void PlayAnimation(string clipName)
    {
        if (animator != null)
        {
            animator.Play(clipName);
        }
    }
    
    // 檢查動畫是否播放完畢
    public virtual bool IsAnimationDone(string clipName)
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return (stateInfo.IsName(clipName) && stateInfo.normalizedTime >= 1.0);
        }
        return false;
    }
    
    // 面向方向
    public virtual void Face(Vector2 direction)
    {
        if (spriteRend != null)
        {
            bool flipped = spriteRend.flipX;
            if (direction.x < 0 && !flipped)
            {
                spriteRend.flipX = true; // 面向左
            }
            else if (direction.x > 0 && flipped)
            {
                spriteRend.flipX = false; // 面向右
            }
        }
    }
    
    // 受傷方法
    public virtual void TakeDamage(float damage)
    {
        // 減少生命值
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 受到傷害，目前剩餘生命值: " + currentHealth + "/" + startingHealth);
        
        if (currentHealth > 0)
        {
            // 轉換到受傷狀態
            SetCurrentState(GetHurtState());
            StartCoroutine(FlashOnDamage()); // 受傷閃爍效果
        }
        else if (currentHealth <= 0)
        {
            // 轉換到死亡狀態
            SetCurrentState(GetDeadState());
        }
    }
    
    // 受傷閃爍效果
    protected virtual IEnumerator FlashOnDamage()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f); // 紅色
            yield return new WaitForSeconds(flashDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white; // 恢復原色
            yield return new WaitForSeconds(flashDuration / (numberOfFlashes * 2));
        }
    }
    
    // 死亡方法
    public virtual void Die()
    {
        Debug.Log(gameObject.name + " 死亡！");
        if (gameManager != null)
        {
            gameManager.PlayerScored(100);
        }
        Destroy(gameObject);
    }
    
    // 在編輯器中繪製範圍
    protected virtual void OnDrawGizmosSelected()
    {
        if (detectionArea != null)
        {
            // 繪製檢測範圍 - 綠色
            Gizmos.color = new Color(0, 1, 0, 0.2f); // 半透明綠色
            Gizmos.DrawWireSphere(detectionArea.transform.position, detectionRange);
        }
        
        if (attackArea != null)
        {
            // 繪製攻擊範圍 - 紅色
            Gizmos.color = new Color(1, 0, 0, 0.3f); // 半透明紅色
            Gizmos.DrawWireSphere(attackArea.transform.position, attackRange);
        }
    }
    
    // 獲取當前血量
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // 獲取最大血量
    public float GetMaxHealth()
    {
        return startingHealth;
    }
} 