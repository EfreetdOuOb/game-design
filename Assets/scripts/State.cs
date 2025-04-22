using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class BaseState
{
    public PlayerController playerController;
    public Health health;
    public AttackManager attackManager; 
     
    public BaseState(PlayerController _playerController)
    {
        playerController=_playerController;
    }

    public BaseState(Health _health)
    {
        health = _health;
    }

    public BaseState(AttackManager _attackManager)
    {
        attackManager = _attackManager;
    }


    public abstract void Update();  
    public abstract void FixedUpdate();
    public abstract void OnTriggerEnter2D(Collider2D collision);
    public abstract void OnTriggerStay2D(Collider2D collision);
}



public class Idle : BaseState
{
    //構造函數，class在被創建時，會調用函數，並傳入PlayerController組件
    public Idle(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //播放動畫
        playerController.PlayAnimation("idle");
        playerController.Stop();
    }

    public override void Update()
    {
        if (playerController.PressDashKey() && playerController.CanDash())
        {
            // 轉換到閃避狀態
            playerController.Dash();
            Debug.Log("轉換到閃避狀態");
        }
        else if (playerController.PressArrowKey())
        {
            //轉換到run狀態
            playerController.SetCurrentState(new Run(playerController));
        }
    }

    public override void FixedUpdate()
    { 

    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("碰觸到物體: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();  
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 更改：新增 OnTriggerStay2D 方法
    {
        if (collision.CompareTag("Enemy"))
        {
            
            if ( health.currentHealth >0 && !health.isInvincible)
            {
                health.TakeDamage();
            }
        }
    }
    
}

public class Run : BaseState
{
    //構造函數，class在被創建時，會調用函數，並傳入PlayerController組件
    public Run(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //播放動畫
        playerController.PlayAnimation("run");
    }
     

    public override void Update()
    {
        if (playerController.PressDashKey() && playerController.CanDash())
        {
            // 轉換到閃避狀態
            playerController.Dash();
            Debug.Log("轉換到閃避狀態");
        }
        else if (!playerController.PressArrowKey())
        {
            //轉換到idle狀態
            playerController.SetCurrentState(new Idle(playerController));
        }
    }

    public override void FixedUpdate()
    {
        playerController.Move();
        playerController.Face();
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("碰觸到物體: " + collision.name);
        if (collision.CompareTag("Enemy"))
        { 
            if (health.currentHealth >0)
            {
                health.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 更改：新增 OnTriggerStay2D 方法
    {
        if (collision.CompareTag("Enemy"))
        {
            if (health.currentHealth > 0&& !health.isInvincible)
            {
                health.TakeDamage();
            }
        }
    }



}

public class Attack : BaseState
{
    //構造函數，class在被創建時，會調用函數，並傳入PlayerController組件
    public Attack(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        attackManager = playerController.GetComponent<AttackManager>();

        // 根據玩家的移動狀態來播放攻擊動畫
        if (playerController.PressArrowKey())
        {
            playerController.PlayAnimation("running_atk"); // 播放邊走邊攻擊的動畫
        }
        else
        {
            playerController.PlayAnimation("atk"); // 播放站立攻擊的動畫
        }
        //attackManager.PerformAttack(playerController.transform);
        playerController.StartCoroutine(HandleAttackDamage());

        playerController.Stop();
    }
    private IEnumerator HandleAttackDamage()
    { 
        while (!playerController.IsAnimationDone("running_atk") && !playerController.IsAnimationDone("atk"))
        {
            yield return null; 
        }
         
        attackManager.PerformAttack(playerController.transform);
         
        if (playerController.PressArrowKey())
        {
            playerController.SetCurrentState(new Run(playerController));
            Debug.Log("轉換到 Run 狀態");
        }
        else
        {
            playerController.SetCurrentState(new Idle(playerController));
            Debug.Log("轉換到 Idle 狀態");
        }
    }
    public override void Update()
    {
        //當atk動畫播放結束，如果按下方向鍵，轉換到run狀態;如果!按下方向鍵，轉換到idle狀態 
         
        if (playerController.IsAnimationDone("running_atk") || playerController.IsAnimationDone("atk"))
        {
            if (playerController.PressArrowKey())
            {
                playerController.SetCurrentState(new Run(playerController));
                Debug.Log("轉換到Run");
            }
            else 
            { 
                playerController.SetCurrentState(new Idle(playerController));
                Debug.Log("轉換到Idle");
            }
        }

    }

    public override void FixedUpdate()
    {
        // 攻擊時也可以移動
        playerController.Move();  
        playerController.Face();  
       
        
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("碰觸到物體: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 更改：新增 OnTriggerStay2D 方法
    {
        if (collision.CompareTag("Enemy"))
        {
            if (health.currentHealth > 0 && !health.isInvincible)
            {
                health.TakeDamage();
            }
        }
    }

}


public class Dead : BaseState
{
    private bool gameEnded = false;

    //構造函數，class在被創建時，會調用函數，並傳入PlayerController組件
    public Dead(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        
        //播放動畫
        playerController.PlayAnimation("dead");
        playerController.Stop();
    }

    public override void Update()
    {
        // 等待死亡動畫播放完畢
        if (!gameEnded && playerController.IsAnimationDone("dead"))
        {
            // 通知GameManager遊戲結束
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.EndGame();
                gameEnded = true;
                Debug.Log("死亡動畫播放完畢，遊戲結束");
            }
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

// 閃避狀態 (Dash不需要單獨的狀態，因為使用協程來處理)
public class Dash : BaseState
{
    public Dash(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        // 播放閃避動畫
        playerController.PlayAnimation("dash");
    }
    
    public override void Update()
    {
        // 檢查是否還在閃避中
        if (!playerController.IsDashing())
        {
            // 如果閃避結束了，根據是否按下方向鍵決定轉換到哪個狀態
            if (playerController.PressArrowKey())
            {
                playerController.SetCurrentState(new Run(playerController));
            }
            else
            {
                playerController.SetCurrentState(new Idle(playerController));
            }
        }
    }
    
    public override void FixedUpdate()
    {
        // 閃避的物理移動由 DashCoroutine 處理
    }
    
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 閃避時可以不處理碰撞
    }
    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 閃避時可以不處理碰撞
    }
}
