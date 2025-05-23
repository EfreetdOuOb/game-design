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
        // 隨機移動
        else if (idleTimer >= nextStateTime)
        {
            if (Random.value > 0.5f) // 50% 機率移動或保持閒置
            {
                monster.SetCurrentState(new SlimeWander(monster)); 
            }
            else
            {
                // 重置計時器，繼續閒置
                idleTimer = 0f;
                nextStateTime = Random.Range(2f, 5f);
            }
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

// 史萊姆的隨機移動狀態
public class SlimeWander : MonsterState
{
    private float wanderTimer = 0f;
    private float wanderDuration;
    private Vector2 wanderDirection;
    
    public SlimeWander(Monster _monster) : base(_monster)
    {
        // 播放移動動畫
        monster.PlayAnimation("run");
        
        // 隨機設置移動時間
        wanderDuration = Random.Range(1f, 3f);
        
        // 隨機選擇方向
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // 設置面向
        monster.Face(wanderDirection);
    }
    
    public override void Update()
    { 
        
        // 更新移動計時器
        wanderTimer += Time.deltaTime;
        
        // 檢測玩家是否在追蹤範圍內
        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            // 轉換到追蹤狀態
            monster.SetCurrentState(new SlimeChase(monster)); 
        }
        // 移動時間結束
        else if (wanderTimer >= wanderDuration)
        {
            // 回到閒置狀態
            monster.SetCurrentState(new SlimeIdle(monster)); 
        }
    }
    
    public override void FixedUpdate()
    { 
        
        // 在隨機方向上移動
        monster.transform.Translate(wanderDirection * monster.moveSpeed * 0.5f * Time.deltaTime);
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

// 骷髏復活狀態
public class SkeletonRevive : MonsterState
{
    private float reviveTimer = 0f;
    private float reviveDuration = 1.5f; // 復活動畫持續時間
    
    public SkeletonRevive(Monster _monster) : base(_monster)
    {
        // 播放復活動畫
        monster.PlayAnimation("revive");
        monster.Stop(); // 復活時停止移動
    }
    
    public override void Update()
    {
        // 更新復活計時器
        reviveTimer += Time.deltaTime;
        
        // 復活動畫結束後轉到閒置狀態
        if (reviveTimer >= reviveDuration || monster.IsAnimationDone("revive"))
        {
            // 直接創建新的SkeletonIdle狀態
            monster.SetCurrentState(new SkeletonIdle(monster));
            Debug.Log("骷髏復活完成，回到閒置狀態");
        }
    }
    
    public override void FixedUpdate()
    {
        // 復活狀態下不移動
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 復活狀態下不處理碰撞
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 復活狀態下不處理碰撞
    }
}

// 骷髏的閒置狀態
public class SkeletonIdle : MonsterState
{
    private float idleTimer = 0f;
    private float nextStateTime = 3f; // 閒置一段時間後可能會隨機移動
    
    public SkeletonIdle(Monster _monster) : base(_monster)
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
            monster.SetCurrentState(new SkeletonChase(monster));
            Debug.Log("骷髏發現玩家，開始追蹤");
        }
        // 隨機移動
        else if (idleTimer >= nextStateTime)
        {
            if (Random.value > 0.5f) // 50% 機率移動或保持閒置
            {
                monster.SetCurrentState(new SkeletonWander(monster));
                Debug.Log("骷髏開始隨機移動");
            }
            else
            {
                // 重置計時器，繼續閒置
                idleTimer = 0f;
                nextStateTime = Random.Range(2f, 5f);
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // 閒置狀態下不需要物理更新
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

// 骷髏的隨機移動狀態
public class SkeletonWander : MonsterState
{
    private float wanderTimer = 0f;
    private float wanderDuration;
    private Vector2 wanderDirection;
    
    public SkeletonWander(Monster _monster) : base(_monster)
    {
        // 播放移動動畫
        monster.PlayAnimation("run");
        
        // 隨機設置移動時間
        wanderDuration = Random.Range(1f, 3f);
        
        // 隨機選擇方向
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // 設置面向
        monster.Face(wanderDirection);
    }
    
    public override void Update()
    {
         
        
        // 更新移動計時器
        wanderTimer += Time.deltaTime;
        
        // 檢測玩家是否在追蹤範圍內
        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            // 轉換到追蹤狀態
            monster.SetCurrentState(new SkeletonChase(monster));
            Debug.Log("骷髏發現玩家，開始追蹤");
        }
        // 移動時間結束
        else if (wanderTimer >= wanderDuration)
        {
            // 回到閒置狀態
            monster.SetCurrentState(new SkeletonIdle(monster));
            Debug.Log("骷髏停止隨機移動，回到閒置狀態");
        }
    }
    
    public override void FixedUpdate()
    { 
        
        // 在隨機方向上移動
        monster.transform.Translate(wanderDirection * monster.moveSpeed * 0.5f * Time.deltaTime);
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

// 骷髏的追蹤狀態
public class SkeletonChase : MonsterState
{
    public SkeletonChase(Monster _monster) : base(_monster)
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
            monster.SetCurrentState(new SkeletonAttack(monster));
            Debug.Log("骷髏進入攻擊範圍");
        }
        // 檢測玩家是否超出追蹤範圍
        else if (!monster.IsPlayerInRange(monster.detectionRange))
        {
            // 回到閒置狀態
            monster.SetCurrentState(new SkeletonIdle(monster));
            Debug.Log("玩家離開，骷髏回到閒置狀態");
        }
    }
    
    public override void FixedUpdate()
    { 
        
        // 追蹤玩家
        monster.MoveTowardsPlayer();
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

// 骷髏的攻擊狀態
public class SkeletonAttack : MonsterState
{
    private bool hasAttacked = false;
    
    public SkeletonAttack(Monster _monster) : base(_monster)
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
                Debug.Log("玩家仍在攻擊範圍內，骷髏繼續攻擊");
            }
            else
            {
                // 如果玩家在檢測範圍內但超出攻擊範圍
                if (monster.IsPlayerInDetectionRange())
                {
                    // 轉換到追蹤狀態
                    monster.SetCurrentState(new SkeletonChase(monster));
                    Debug.Log("玩家離開攻擊範圍，骷髏開始追蹤");
                }
                else
                {
                    // 玩家離開檢測範圍，回到閒置狀態
                    monster.SetCurrentState(new SkeletonIdle(monster));
                    Debug.Log("玩家離開檢測範圍，骷髏回到閒置狀態");
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

// 骷髏的受傷狀態
public class SkeletonHurt : MonsterState
{
    private float hurtTimer = 0f;
    private float hurtDuration = 0.4f; // 受傷狀態持續時間
    
    public SkeletonHurt(Monster _monster) : base(_monster)
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
        if (hurtTimer >= hurtDuration)
        {
            // 檢測玩家位置並轉到適當狀態
            if (monster.IsPlayerInAttackRange())
            {
                // 玩家在攻擊範圍內
                monster.SetCurrentState(new SkeletonAttack(monster));
            }
            else if (monster.IsPlayerInDetectionRange())
            {
                // 玩家在檢測範圍內但不在攻擊範圍內
                monster.SetCurrentState(new SkeletonChase(monster));
            }
            else
            {
                // 玩家不在檢測範圍內
                monster.SetCurrentState(new SkeletonIdle(monster));
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

// 骷髏的死亡狀態
public class SkeletonDead : MonsterState
{
    private bool deathProcessed = false;
    
    public SkeletonDead(Monster _monster) : base(_monster)
    {
        // 播放死亡動畫
        monster.PlayAnimation("dead");
        monster.Stop(); // 停止移動
    }
    
    public override void Update()
    {
        // 檢查死亡動畫是否播放完畢
        if (!deathProcessed && monster.IsAnimationDone("dead"))
        {
            deathProcessed = true;
            // 調用怪物的死亡方法
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
        else if (idleTimer >= nextStateTime)
        {
            if (Random.value > 0.5f)
            {
                monster.SetCurrentState(new SpiderWander(monster));
            }
            else
            {
                idleTimer = 0f;
                nextStateTime = Random.Range(2f, 5f);
            }
        }
    }

    public override void FixedUpdate() { }
    public override void OnTriggerEnter2D(Collider2D collision) { }
    public override void OnTriggerStay2D(Collider2D collision) { }
}

// 蜘蛛的隨機移動狀態
public class SpiderWander : MonsterState
{
    private float wanderTimer = 0f;
    private float wanderDuration;
    private Vector2 wanderDirection;

    public SpiderWander(Monster _monster) : base(_monster)
    {
        monster.PlayAnimation("run");
        wanderDuration = Random.Range(1f, 3f);
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        monster.Face(wanderDirection);
    }

    public override void Update()
    {
        wanderTimer += Time.deltaTime;
        if (monster.IsPlayerInRange(monster.detectionRange))
        {
            monster.SetCurrentState(new SpiderChase(monster));
        }
        else if (wanderTimer >= wanderDuration)
        {
            monster.SetCurrentState(new SpiderIdle(monster));
        }
    }

    public override void FixedUpdate()
    {
        monster.transform.Translate(wanderDirection * monster.moveSpeed * 0.5f * Time.deltaTime);
    }
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