using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 引入DOTween

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
    
    [Header("物理屬性")]
    [Tooltip("摩擦力係數")]
    public float friction = 0.2f; // 摩擦力係數
    [Tooltip("粘滯阻力係數")]
    public float dragRatio = 0.8f; // 粘滯阻力係數
    [Tooltip("速度倍增器")]
    public float speedMultiplier = 1.0f; // 速度倍增器
    
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
    public int numberOfFlashes = 1; // 閃爍次數
    public float flashDuration = 0.5f; // 閃爍持續時間
    
    [Header("擊退效果")]
    [Tooltip("怪物被擊退的距離")]
    public float knockbackDistance = 1.0f; // 擊退距離
    [Tooltip("怪物被擊退的時間")]
    public float knockbackDuration = 0.3f; // 擊退持續時間
    [Tooltip("怪物被擊退時產生的灰塵特效預製體")]
    public GameObject dustParticlePrefab; // 灰塵粒子特效預製體
    [Tooltip("特效生成位置（可選），不設置則默認在怪物腳下")]
    public Transform effectSpawnPoint; // 特效生成位置
    [Tooltip("怪物被擊退產生特效的速度閾值")]
    public float dustSpawnThreshold = 0.5f; // 特效產生的速度閾值
    
    public GameObject detectionArea; // 檢測範圍空物件
    public GameObject attackArea; // 攻擊範圍空物件
    
    // 擊退相關
    protected Vector2 knockbackDirection; // 擊退方向
    protected bool isBeingKnockedBack = false; // 是否正在被擊退
    protected ParticleSystem currentDustEffect; // 當前的灰塵特效
    
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
            rb2d.mass = 100000f;
            rb2d.linearDamping = 10f; // 高阻力
            rb2d.constraints = RigidbodyConstraints2D.FreezeRotation; // 凍結旋轉但允許位置移動，以便擊退效果
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
        if (currentState != null && !isBeingKnockedBack)
        {
            currentState.Update();
        }
    }
    
    protected virtual void FixedUpdate()
    {
        // 更新物理
        if (currentState != null && !isBeingKnockedBack)
        {
            currentState.FixedUpdate();
        }
        
        // 應用拖曳力和摩擦力
        Drag(Time.fixedDeltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState != null && !isBeingKnockedBack)
        {
            currentState.OnTriggerEnter2D(collision);
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (currentState != null && !isBeingKnockedBack)
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
        if (target != null && !isBeingKnockedBack)
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
        if (attackManager != null && target != null && !isBeingKnockedBack)
        {
            // 使用新的攻擊機制，啟動攻擊動畫
            attackManager.StartAttacking(target);
            PlayAnimation("bite"); // 播放攻擊動畫
            Debug.Log(gameObject.name + " 開始攻擊動畫");
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
    
    // 擊退方法
    public virtual void Knockback(Vector2 sourcePosition, float force = 1.0f)
    {
        if (isBeingKnockedBack) return; // 如果已經在被擊退，則忽略
        
        // 計算擊退方向（從攻擊源位置指向怪物）
        knockbackDirection = (Vector2)transform.position - sourcePosition;
        knockbackDirection.Normalize();
        
        // 使用DOTween實現擊退效果
        isBeingKnockedBack = true;
        
        // 計算擊退距離和時間
        float distance = knockbackDistance * force;
        
        // 使用DOTween移動怪物
        transform.DOMove(
            (Vector2)transform.position + knockbackDirection * distance, 
            knockbackDuration
        ).SetEase(Ease.OutQuad) // 緩出效果，開始快然後變慢
         .OnComplete(() => {
            isBeingKnockedBack = false; // 結束擊退狀態
         });
        
        // 立即產生灰塵特效
        SpawnDustEffect();
    }
    
    // 產生灰塵特效
    protected virtual void SpawnDustEffect()
    {
        if (dustParticlePrefab != null)
        {
            Transform spawnPoint;
            
            // 如果設置了特效生成點，使用它；否則使用怪物腳下位置
            if (effectSpawnPoint != null)
            {
                spawnPoint = effectSpawnPoint;
            }
            else
            {
                // 計算灰塵特效的生成位置（怪物腳下）
                Vector3 dustPosition = transform.position;
                dustPosition.y -= spriteRend.bounds.extents.y * 0.8f; // 將特效放在怪物腳下
                
                // 實例化一個臨時點
                GameObject tempPoint = new GameObject("TempEffectPoint");
                tempPoint.transform.position = dustPosition;
                tempPoint.transform.SetParent(transform);
                spawnPoint = tempPoint.transform;
            }
            
            // 實例化粒子系統並設為子物件
            GameObject dustObj = Instantiate(dustParticlePrefab, spawnPoint.position, Quaternion.identity, transform);
            
            // 獲取粒子系統組件
            ParticleSystem dustParticle = dustObj.GetComponent<ParticleSystem>();
            if (dustParticle != null)
            {
                // 保存引用以便後續管理
                currentDustEffect = dustParticle;
                
                // 開始粒子系統播放
                dustParticle.Play();
                
                // 獲取粒子系統的主模塊
                var main = dustParticle.main;
                // 設置停止行為：EmitAndDestroy，這樣粒子會自然完成生命週期後銷毀
                main.stopAction = ParticleSystemStopAction.Destroy;
                
                // 在擊退結束時停止發射新粒子
                StartCoroutine(StopEffectAfterKnockback(dustParticle));
            }
            else
            {
                Debug.LogWarning("灰塵預製體缺少粒子系統組件!");
            }
            
            // 如果使用了臨時點，在適當時候銷毀它
            if (effectSpawnPoint == null)
            {
                Destroy(spawnPoint.gameObject, knockbackDuration + 1.0f);
            }
        }
    }
    
    // 擊退結束後停止發射新粒子
    private IEnumerator StopEffectAfterKnockback(ParticleSystem ps)
    {
        if (ps == null) yield break;
        
        // 等待擊退結束
        yield return new WaitForSeconds(knockbackDuration);
        
        // 停止發射新粒子，但讓現有粒子完成生命週期
        ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }
    
    // 受傷方法
    public virtual void TakeDamage(float damage)
    {
        // 減少生命值
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 受到傷害，目前剩餘生命值: " + currentHealth + "/" + startingHealth);
        
        // 只有活著的怪物才會被擊退
        if (target != null && currentHealth > 0)
        {
            Knockback(target.position); // 從玩家方向擊退
        }
        
        if (currentHealth > 0)
        {
            // 轉換到受傷狀態
            SetCurrentState(GetHurtState());
            StartCoroutine(FlashOnDamage()); // 受傷閃爍效果
        }
        else if (currentHealth <= 0)
        {
            // 如果可以復活，則調用復活邏輯
            if (CanRevive())
            {
                OnRevive();
            }
            else
            {
                // 血量歸零且無法復活，立即摧毀
                Die();
            }
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
        
        // 在死亡時停止所有DOTween動畫
        DOTween.Kill(transform);
        
        // 加分
        if (gameManager != null)
        {
            gameManager.PlayerScored(100);
        }
        
        // 立即銷毀物件
        Destroy(gameObject);
    }
    
    // 檢查是否可以復活（默認為false，子類可以覆寫）
    protected virtual bool CanRevive()
    {
        return false;
    }
    
    // 復活邏輯（子類需要覆寫）
    protected virtual void OnRevive()
    {
        // 默認不執行任何操作
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
    
    // 獲取擊退狀態
    public bool IsBeingKnockedBack()
    {
        return isBeingKnockedBack;
    }
    
    // 獲取當前速度
    public virtual float Speed()
    {
        if (currentState != null && currentState.GetType().Name.Contains("Idle"))
            return 0;
        
        return moveSpeed * speedMultiplier;
    }
    
    // 添加摩擦力和粘滯阻力
    protected virtual void Drag(float dT)
    {
        if (rb2d == null) return;
        
        var v = rb2d.linearVelocity;
        
        // 添加摩擦力
        rb2d.AddForce(-friction * rb2d.mass * 9.8f * v.normalized);
        
        // 粘滯阻力
        rb2d.AddForce(-dragRatio * rb2d.mass * v);
    }
}