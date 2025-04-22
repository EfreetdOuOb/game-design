using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        base.Update();
        
         
    }
    
    public override Vector2 MoveTowardsPlayer()
    {
        Vector2 direction = Vector2.zero; // 初始化方向
        
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
            
            // 面向玩家
            Face(direction);
            
            // 只有在距離超過攻擊範圍時才會移動
            if (Vector2.Distance(target.position, transform.position) > attackRange)
            {
                // 正常移動
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        
        return direction; // 返回方向
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
         
            base.Attack();
             
         
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