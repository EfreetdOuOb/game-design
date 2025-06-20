using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 怪物基礎狀態抽象類
public abstract class MonsterState
{
    protected Monster monster; // 引用怪物實例
    
    
    public MonsterState(Monster _monster)
    {
        monster = _monster;
    }
    
    // 必須由子類實現的方法
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void OnTriggerEnter2D(Collider2D collision);
    public abstract void OnTriggerStay2D(Collider2D collision);
}

// 史萊姆的閒置狀態
public class SlimeIdle : MonsterState
{
    private float idleTimer = 0f;
    private float nextStateTime = 3f; // 閒置一段時間後可能會隨機移動
    
    public SlimeIdle(Monster _monster) : base(_monster)
    {
        // 播放閒置動畫
        monster.PlayAnimation("idle");
        monster.Stop(); // 停止移動
        
        // 隨機設置閒置時間
        nextStateTime = Random.Range(2f, 5f);
    }
    
    public override void Update()
    { 
        
        // 更新閒置計時器
        idleTimer += Time.deltaTime;
        
        // 檢測玩家是否在追蹤範圍內
        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            // 轉換到追蹤狀態
            monster.SetCurrentState(new SlimeChase(monster)); 
        }
        
    }
    
    public override void FixedUpdate()
    {
        // 閒置狀態下不需要物理更新
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 不再處理碰撞造成的玩家傷害
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 不再處理碰撞造成的玩家傷害
    }
}



// 史萊姆的追蹤狀態
public class SlimeChase : MonsterState
{
    public SlimeChase(Monster _monster) : base(_monster)
    {
        // 播放移動動畫
        monster.PlayAnimation("run");
    }
    
    public override void Update()
    {
         
        
        // 檢測玩家是否在攻擊範圍內
        if (monster.IsPlayerInAttackRange())
        {
            // 轉換到攻擊狀態
            monster.SetCurrentState(new SlimeAttack(monster)); 
        }
        // 檢測玩家是否超出追蹤範圍
        else if (!monster.IsPlayerInRange(monster.detectionRange))
        {
            // 回到閒置狀態
            monster.SetCurrentState(new SlimeIdle(monster)); 
        }
    }
    
    public override void FixedUpdate()
    { 
        
         
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 不再處理碰撞造成的玩家傷害
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 不再處理碰撞造成的玩家傷害
    }
}

// 史萊姆的攻擊狀態
public class SlimeAttack : MonsterState
{
    private bool hasAttacked = false;
    
    public SlimeAttack(Monster _monster) : base(_monster)
    {
        // 啟動攻擊，動畫和傷害判定由 MonsterAttackManager 處理
        monster.Attack();
        monster.Stop(); // 攻擊時停止移動
    }
    
    public override void Update()
    { 
        // 檢測動畫是否播放完畢
        if (!hasAttacked && monster.IsAnimationDone("attack"))
        {
            hasAttacked = true;
            monster.attackManager.StopAttacking();
            
            // 注意：冷卻計時已經由 MonsterAttackManager 在動畫結束時觸發
        }
        
        // 攻擊完成後並且可以再次攻擊時，檢查是否需要繼續攻擊
        if (hasAttacked && monster.CanAttack())
        {
            // 檢測玩家是否仍在攻擊範圍內
            if (monster.IsPlayerInAttackRange())
            {
                // 立即開始新的攻擊循環
                hasAttacked = false;
                monster.Attack(); 
            }
            else
            {
                // 如果玩家在檢測範圍內但超出攻擊範圍
                if (monster.IsPlayerInDetectionRange())
                {
                    // 轉換到追蹤狀態
                    monster.SetCurrentState(new SlimeChase(monster)); 
                }
                else
                {
                    // 玩家離開檢測範圍，回到閒置狀態
                    monster.SetCurrentState(new SlimeIdle(monster)); 
                }
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // 攻擊狀態下不移動
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 不處理碰撞
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 不處理碰撞
    }
}

// 史萊姆的受傷狀態
public class SlimeHurt : MonsterState
{
    private float hurtTimer = 0f;
    private float hurtDuration = 0.5f; // 受傷狀態持續時間
    
    public SlimeHurt(Monster _monster) : base(_monster)
    {
        // 播放受傷動畫
        monster.PlayAnimation("hurt");
        monster.Stop(); // 受傷時停止移動
    }
    
    public override void Update()
    { 
        
        // 更新受傷計時器
        hurtTimer += Time.deltaTime;
        
        // 檢測受傷狀態是否結束
        if (hurtTimer >= hurtDuration || monster.IsAnimationDone("hurt"))
        {
            // 檢測玩家位置並轉到適當狀態
            if (monster.IsPlayerInRange(monster.detectionRange))
            {
                // 如果玩家在攻擊範圍內，轉換到攻擊狀態
                if (monster.IsPlayerInAttackRange())
                {
                    monster.SetCurrentState(new SlimeAttack(monster));
                }
                // 如果玩家在追蹤範圍內但不在攻擊範圍內，轉換到追蹤狀態
                else
                {
                    monster.SetCurrentState(new SlimeChase(monster));
                }
            }
            else
            {
                // 如果玩家不在追蹤範圍內，回到閒置狀態
                monster.SetCurrentState(new SlimeIdle(monster));
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // 受傷狀態下不移動
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 受傷狀態下不處理碰撞
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 受傷狀態下不處理碰撞
    }
}

// 史萊姆的死亡狀態
public class SlimeDead : MonsterState
{
    
    public SlimeDead(Monster _monster) : base(_monster)
    {
        // 播放死亡動畫
        monster.PlayAnimation("dead");
        monster.Stop(); // 停止移動
    }
    
    public override void Update()
    {
        // 檢查死亡動畫是否播放完畢
        if (monster.IsAnimationDone("dead"))
        { 
            // 立即摧毀怪物
            monster.Die();
        }
    }
    
    public override void FixedUpdate()
    {
        // 死亡狀態下不移動
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 死亡狀態下不處理碰撞
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 死亡狀態下不處理碰撞
    }
}



// 蜘蛛的閒置狀態
public class SpiderIdle : MonsterState
{
    private float idleTimer = 0f;
    private float nextStateTime = 3f;

    public SpiderIdle(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("idle");
        monster.Stop();
        nextStateTime = Random.Range(2f, 5f);
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new SpiderChase(monster));
        }
        
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}



// 蜘蛛的追蹤狀態
public class SpiderChase : MonsterState
{
    public SpiderChase(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("run");
    }

    public override void Update()
    {
        if (monster.IsPlayerInAttackRange())
        {
            monster.SetCurrentState(new SpiderAttack(monster));
        }
        else if (!monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new SpiderIdle(monster));
        }
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 蜘蛛的攻擊狀態
public class SpiderAttack : MonsterState
{
    private bool hasAttacked = false;

    public SpiderAttack(Monster _monster) : base(_monster)
    {
        // 隨機選擇攻擊方式（毒彈或蛛絲）
        if (monster is Spider spider)
        {
            spider.Attack();
        }
        monster.Stop();
    }

    public override void Update()
    {
        if (!hasAttacked && monster.IsAnimationDone("attack"))
        {
            hasAttacked = true;
            monster.attackManager?.StopAttacking();
        }
        if (hasAttacked && monster.CanAttack())
        {
            if (monster.IsPlayerInAttackRange())
            {
                hasAttacked = false;
                if (monster is Spider spider)
                {
                    spider.Attack();
                }
            }
            else if (monster.IsPlayerInDetectionRange())
            {
                monster.SetCurrentState(new SpiderChase(monster));
            }
            else
            {
                monster.SetCurrentState(new SpiderIdle(monster));
            }
        }
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 蜘蛛的受傷狀態
public class SpiderHurt : MonsterState
{
    private float hurtTimer = 0f;
    private float hurtDuration = 0.5f;

    public SpiderHurt(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("hurt");
        monster.Stop();
    }

    public override void Update()
    {
        // 更新受傷計時器
        hurtTimer += Time.deltaTime;

        // 如果受傷時間結束或受傷動畫播放完畢
        if (hurtTimer >= hurtDuration || monster.IsAnimationDone("hurt"))
        {
            // 檢查是否還有玩家在追蹤範圍內
            if (monster.IsPlayerInRange(monster.detectionRange))
            {
                // 如果玩家在攻擊範圍內，轉換到攻擊狀態
                if (monster.IsPlayerInAttackRange())
                {
                    monster.SetCurrentState(new SpiderAttack(monster));
                }
                // 如果玩家在追蹤範圍內但不在攻擊範圍內，轉換到追蹤狀態
                else
                {
                    monster.SetCurrentState(new SpiderChase(monster));
                }
            }
            else
            {
                // 如果玩家不在追蹤範圍內，回到閒置狀態
                monster.SetCurrentState(new SpiderIdle(monster));
            }
        }
    }
    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 蜘蛛的死亡狀態
public class SpiderDead : MonsterState
{
    private bool deathProcessed = false;
    public SpiderDead(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("dead");
        monster.Stop();
    }
    public override void Update()
    {
        if (!deathProcessed && monster.IsAnimationDone("dead"))
        {
            deathProcessed = true;
            monster.Die();
        }
    }
    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的閒置狀態
public class SkullIdle : MonsterState
{
    private float idleTimer = 0f;
    private float nextStateTime = 3f;

    public SkullIdle(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("idle");
        monster.Stop();
        nextStateTime = Random.Range(2f, 5f);
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;

        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new SkullChase(monster));
        }
        
    }

    public override void FixedUpdate() { }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}



// 骷髏的追蹤狀態
public class SkullChase : MonsterState
{
    public SkullChase(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("run");
    }

    public override void Update()
    {
        // 檢測玩家是否在攻擊範圍內
        if (monster.IsPlayerInAttackRange())
        {
            // 轉換到攻擊狀態
            monster.SetCurrentState(new SkullAttack(monster)); 
        }
        // 檢測玩家是否超出追蹤範圍
        else if (!monster.IsPlayerInRange(monster.detectionRange))
        {
            // 回到閒置狀態
            monster.SetCurrentState(new SkullIdle(monster)); 
        }
    }

    public override void FixedUpdate()
    {
        
    }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的攻擊狀態
public class SkullAttack : MonsterState
{
    private bool hasAttacked = false;
    private bool isAttackAnimationDone = false;

    public SkullAttack(Monster _monster) : base(_monster)
    {
        monster.Attack();
        monster.Stop();
        isAttackAnimationDone = false;
    }

    public override void Update()
    {
        

        // 檢查攻擊動畫是否結束
        if (!isAttackAnimationDone && monster.IsAnimationDone("attack"))
        {
            isAttackAnimationDone = true;
            hasAttacked = true;
            monster.StartAttackCooldown();
        }

        // 只有在攻擊動畫結束且冷卻時間結束後才能進行下一次攻擊
        if (isAttackAnimationDone && hasAttacked)
        {
            if (!monster.IsPlayerInAttackRange())
            {
                monster.SetCurrentState(new SkullChase(monster));
            }
            else if (monster.CanAttack())
            {
                monster.SetCurrentState(new SkullAttack(monster));
            }
        }
    }

    public override void FixedUpdate() { }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的受傷狀態
public class SkullHurt : MonsterState
{
    private float hurtTimer = 0f;
    private float hurtDuration = 0.4f;

    public SkullHurt(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("hurt");
        monster.Stop();
    }

    public override void Update()
    {
        hurtTimer += Time.deltaTime;

        if (hurtTimer >= hurtDuration)
        {
            if (monster.IsPlayerInAttackRange())
            {
                monster.SetCurrentState(new SkullAttack(monster));
            }
            else if (monster.IsPlayerInRange(monster.detectionRange))
            {
                monster.SetCurrentState(new SkullChase(monster));
            }
            else
            {
                monster.SetCurrentState(new SkullIdle(monster));
            }
        }
    }

    public override void FixedUpdate() { }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的第一次死亡狀態
public class SkullFirstDead : MonsterState
{ 

    public SkullFirstDead(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("dead");
        monster.Stop();
    }

    public override void Update()
    {
        if (monster.IsAnimationDone("dead"))
        {
            monster.SetCurrentState(new SkullRevive(monster));
        }
    }

    public override void FixedUpdate() { }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的復活狀態
public class SkullRevive : MonsterState
{
    private float reviveTimer = 0f;
    private float reviveDuration = 1.5f;

    public SkullRevive(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("dead");
        monster.Stop();
    }

    public override void Update()
    {
        reviveTimer += Time.deltaTime;

        if (reviveTimer >= reviveDuration)
        {
            if (monster.IsPlayerInAttackRange())
            {
                monster.SetCurrentState(new SkullAttack(monster));
            }
            else if (monster.IsPlayerInRange(monster.detectionRange))
            {
                monster.SetCurrentState(new SkullChase(monster));
            }
            else
            {
                monster.SetCurrentState(new SkullIdle(monster));
            }
        }
    }

    public override void FixedUpdate() { }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 骷髏的第二次死亡狀態
public class SkullSecondDead : MonsterState
{ 
    public SkullSecondDead(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("dead2");
        monster.Stop();
    }
    public override void Update()
    {
        if (monster.IsAnimationDone("dead2"))
        { 
            monster.Die(); // 真正銷毀
        }
    }
    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 鬼魂的閒置狀態
public class GhostIdle : MonsterState
{
    private float idleTimer = 0f;
    private float nextStateTime = 3f;

    public GhostIdle(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("idle");
        monster.Stop();
        nextStateTime = Random.Range(2f, 5f);
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;

        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new GhostChase(monster));
        }
        
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}



// 鬼魂的追蹤狀態
public class GhostChase : MonsterState
{
    public GhostChase(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("run");
    }

    public override void Update()
    {
        if (monster.IsPlayerInAttackRange())
        {
            monster.SetCurrentState(new GhostDead(monster));
        }
        else if (!monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new GhostIdle(monster));
        }
    }

    public override void FixedUpdate()
    {
        
    }

    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}



// 鬼魂的受傷狀態
public class GhostHurt : MonsterState
{
    private float hurtTimer = 0f;
    private float hurtDuration = 0.4f;

    public GhostHurt(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("hurt");
        monster.Stop();
    }

    public override void Update()
    {
        // 如果血量<=0，立即切到死亡狀態
        if (monster.currentHealth <= 0)
        {
            monster.SetCurrentState(new GhostDead(monster)); // 鬼魂直接死掉，沒有復活
            return;
        }

        hurtTimer += Time.deltaTime;

        if (hurtTimer >= hurtDuration)
        {
            if (monster.IsPlayerInAttackRange())
            {
                monster.SetCurrentState(new GhostDead(monster));
            }
            else if (monster.IsPlayerInRange(monster.detectionRange))
            {
                monster.SetCurrentState(new GhostChase(monster));
            }
            else
            {
                monster.SetCurrentState(new GhostIdle(monster));
            }
        }
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 鬼魂的死亡狀態 (自爆後)
public class GhostDead : MonsterState
{
    

    public GhostDead(Monster _monster) : base(_monster)
    {
        monster.Attack();
        monster.Stop();
        monster.PlayAnimation("attack"); // 鬼魂死亡動畫 (自爆後通常直接消失或有簡單死亡動畫) 
    }

    public override void Update()
    {
        if (monster.IsAnimationDone("attack"))
        {
            
            monster.Die(); // 真正銷毀鬼魂
        }
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision)
    { 
        
    }
    public override void OnTriggerStay2D(Collider2D collision)
    {

    }
    
} 