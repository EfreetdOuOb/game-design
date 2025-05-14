using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 骷髏怪物類，可以復活一次的特殊怪物
public class Skeleton : Monster
{
    [Header("骷髏特定屬性")]
    public float attackInterval = 1.2f;    // 攻擊間隔
    public bool canReviveOnce = true;      // 是否可以復活一次
    private bool hasRevived = false;       // 是否已經復活過
    private float attackTimer = 0f;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        // 設置初始屬性
        moveSpeed = 1.2f;       // 骷髏移動稍快
        attackRange = 1.5f;     // 骷髏攻擊範圍較大
        detectionRange = 6f;    // 骷髏檢測範圍較大
        
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 更新攻擊計時器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    
    // 覆寫面向方法
    public override void Face(Vector2 direction)
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
    
    // 覆寫攻擊方法，增加攻擊間隔
    public override void Attack()
    {
        if (attackTimer <= 0)
        {
            base.Attack();
            attackTimer = attackInterval;
        }
    }
    
    // 覆寫檢查是否可以復活的方法
    protected override bool CanRevive()
    {
        return canReviveOnce && !hasRevived;
    }
    
    // 覆寫復活邏輯
    protected override void OnRevive()
    {
        // 標記已經復活過
        hasRevived = true;
        
        // 恢復一部分生命值
        currentHealth = startingHealth * 0.5f;
        
        // 播放復活特效
        PlayReviveEffect();
        
        // 設置為復活狀態
        SetCurrentState(GetReviveState());
        
        Debug.Log(gameObject.name + " 復活了！當前生命值: " + currentHealth);
    }
    
    // 播放復活特效
    private void PlayReviveEffect()
    {
        // 可以在這裡添加復活特效，如閃光、粒子等
        StartCoroutine(FlashOnRevive());
    }
    
    // 復活閃爍效果
    private IEnumerator FlashOnRevive()
    {
        Color originalColor = spriteRend.color;
        
        // 閃爍幾次綠色光芒表示復活
        for (int i = 0; i < 3; i++)
        {
            spriteRend.color = new Color(0.5f, 1f, 0.5f, 0.8f); // 綠色
            yield return new WaitForSeconds(0.1f);
            spriteRend.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // 獲取各種狀態
    protected override MonsterState GetIdleState()
    {
        return new SkeletonIdle(this);
    }
    
    protected override MonsterState GetHurtState()
    {
        return new SkeletonHurt(this);
    }
    
    protected override MonsterState GetDeadState()
    {
        return new SkeletonDead(this);
    }
    
    // 獲取復活狀態
    protected MonsterState GetReviveState()
    {
        return new SkeletonRevive(this);
    }
} 