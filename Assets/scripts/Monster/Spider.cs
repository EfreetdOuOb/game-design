using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Spider : Monster
{ 
     
     
    
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
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.isPaused)
            return;
        base.Update();
        if (currentState is SlimeChase)
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
} 