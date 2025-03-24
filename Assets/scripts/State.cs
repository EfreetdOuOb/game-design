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
    //構造函數，class在被建立時，會呼叫此函數，並傳入PlayerController物件
    public Idle(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //播放動畫
        playerController.PlayAnimation("idle");
        playerController.Stop();
    }

    public override void Update()
    {
        if (playerController.PressAttackKey())
        {
            //切換到attack狀態
            playerController.SetCurrentState(new Attack(playerController));
            Debug.Log("切換至攻擊狀態");
        }
        else if (playerController.PressArrowKey())
        {
            //切換到run狀態
            playerController.SetCurrentState(new Run(playerController));
        }
    }

    public override void FixedUpdate()
    { 

    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("檢測到碰撞: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();  
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 修改：新增 OnTriggerStay2D 方法
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
    //構造函數，class在被建立時，會呼叫此函數，並傳入PlayerController物件
    public Run(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //播放動畫
        playerController.PlayAnimation("run");
    }
     

    public override void Update()
    {
        if (playerController.PressAttackKey())
        {
            // 
            playerController.SetCurrentState(new Attack(playerController));
            Debug.Log("切換至攻擊狀態");
        }
        else if (!playerController.PressArrowKey())
        {
            //切換到idle狀態
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
        Debug.Log("檢測到碰撞: " + collision.name);
        if (collision.CompareTag("Enemy"))
        { 
            if (health.currentHealth >0)
            {
                health.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 修改：新增 OnTriggerStay2D 方法
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
    //構造函數，class在被建立時，會呼叫此函數，並傳入PlayerController物件
    public Attack(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        attackManager = playerController.GetComponent<AttackManager>();

        // 根據角色的移動狀態來播放相應的動畫
        if (playerController.PressArrowKey())
        {
            playerController.PlayAnimation("running_atk"); // 播放運行中的攻擊動畫
        }
        else
        {
            playerController.PlayAnimation("atk"); // 播放靜止攻擊動畫
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
            Debug.Log("切換至 Run 狀態");
        }
        else
        {
            playerController.SetCurrentState(new Idle(playerController));
            Debug.Log("切換至 Idle 狀態");
        }
    }
    public override void Update()
    {
        //當atk動畫播放結束，如果按下方向鍵，切換至run狀態;如果!按下方向鍵，切換至idle狀態 
         
        if (playerController.IsAnimationDone("running_atk") || playerController.IsAnimationDone("atk"))
        {
            if (playerController.PressArrowKey())
            {
                playerController.SetCurrentState(new Run(playerController));
                Debug.Log("切換至Run");
            }
            else 
            { 
                playerController.SetCurrentState(new Idle(playerController));
                Debug.Log("切換至Idle");
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
        Debug.Log("檢測到碰撞: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // 修改：新增 OnTriggerStay2D 方法
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

