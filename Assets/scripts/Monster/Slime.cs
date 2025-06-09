using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class Slime : Monster
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
        var gm = FindAnyObjectByType<GameManager>();
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
    
    
    
    // 覆寫計算經驗值的方法，史萊姆是弱小怪物，給的經驗值較少
    protected override int CalculateExpValue()
    {
        return 50; // 史萊姆給予50點經驗值
    }
    
    // 獲取各種狀態
    protected override MonsterState GetIdleState()
    {
        return new SlimeIdle(this);
    }
    
    protected override MonsterState GetHurtState()
    {
        return new SlimeHurt(this);
    }
    
    protected override MonsterState GetDeadState()
    {
        return new SlimeDead(this);
    }
} 