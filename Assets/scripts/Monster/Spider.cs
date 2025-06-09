using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Spider : Monster
{ 
    public enum AttackType { Poison, Web }
    private AttackType lastAttackType;
     
     
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        // 設置初始屬性
        
        
        base.Start();
    }
    
    protected override void Update()
    {
        // 檢查遊戲是否暫停
        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null && gm.isPaused)
            return;
        base.Update();
        if (currentState is SpiderChase)
        {
            Vector2 direction = MoveTowardsPlayer();
             
            if (direction != Vector2.zero)
            {
                Face(direction);
                transform.Translate(direction * moveSpeed * Time.deltaTime);//裡面使用deltaTime而不是fixedDeltaTime，fixedDeltaTime會根據幀數改變怪物速度
            }
        }
         
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
 
    }
    
    // 覆寫面向方法
    public override void Face(Vector2 direction)
    {
        if (spriteRend != null)
        {
            bool flipped = spriteRend.flipX;
            if (direction.x < 0 && !flipped)
            {
                spriteRend.flipX = false; // 面向右
            }
            else if (direction.x > 0 && flipped)
            {
                spriteRend.flipX = true; // 面向左
            }
        }
    }
    
    // 覆寫計算經驗值的方法
    protected override int CalculateExpValue()
    {
        return 60; 
    }
    
    // 獲取各種狀態
    protected override MonsterState GetIdleState()
    {
        return new SpiderIdle(this);
    }
    
    protected override MonsterState GetHurtState()
    {
        return new SpiderHurt(this);
    }
    
    protected override MonsterState GetDeadState()
    {
        return new SpiderDead(this);
    }

    public override void Attack()
    {
        if (!canAttack) return;

        if (attackManager != null && target != null)
        {
            // 隨機決定攻擊方式
            lastAttackType = Random.value < 0.5f ? AttackType.Poison : AttackType.Web;
            string animName = lastAttackType == AttackType.Poison ? "attack" : "attack2";

            // 手動播放對應動畫
            PlayAnimation(animName);

            // 啟動攻擊
            attackManager.StartAttacking(target);

            Debug.Log($"{gameObject.name} 開始{lastAttackType}攻擊，播放{animName}動畫");
        }
    }
} 