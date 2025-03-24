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
    //�c�y��ơAclass�b�Q�إ߮ɡA�|�I�s����ơA�öǤJPlayerController����
    public Idle(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //����ʵe
        playerController.PlayAnimation("idle");
        playerController.Stop();
    }

    public override void Update()
    {
        if (playerController.PressAttackKey())
        {
            //������attack���A
            playerController.SetCurrentState(new Attack(playerController));
            Debug.Log("�����ܧ������A");
        }
        else if (playerController.PressArrowKey())
        {
            //������run���A
            playerController.SetCurrentState(new Run(playerController));
        }
    }

    public override void FixedUpdate()
    { 

    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("�˴���I��: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();  
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // �ק�G�s�W OnTriggerStay2D ��k
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
    //�c�y��ơAclass�b�Q�إ߮ɡA�|�I�s����ơA�öǤJPlayerController����
    public Run(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        //����ʵe
        playerController.PlayAnimation("run");
    }
     

    public override void Update()
    {
        if (playerController.PressAttackKey())
        {
            // 
            playerController.SetCurrentState(new Attack(playerController));
            Debug.Log("�����ܧ������A");
        }
        else if (!playerController.PressArrowKey())
        {
            //������idle���A
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
        Debug.Log("�˴���I��: " + collision.name);
        if (collision.CompareTag("Enemy"))
        { 
            if (health.currentHealth >0)
            {
                health.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // �ק�G�s�W OnTriggerStay2D ��k
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
    //�c�y��ơAclass�b�Q�إ߮ɡA�|�I�s����ơA�öǤJPlayerController����
    public Attack(PlayerController _playerController) : base(_playerController)
    {
        health = playerController.GetComponent<Health>();
        attackManager = playerController.GetComponent<AttackManager>();

        // �ھڨ��⪺���ʪ��A�Ӽ���������ʵe
        if (playerController.PressArrowKey())
        {
            playerController.PlayAnimation("running_atk"); // ����B�椤�������ʵe
        }
        else
        {
            playerController.PlayAnimation("atk"); // �����R������ʵe
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
            Debug.Log("������ Run ���A");
        }
        else
        {
            playerController.SetCurrentState(new Idle(playerController));
            Debug.Log("������ Idle ���A");
        }
    }
    public override void Update()
    {
        //��atk�ʵe���񵲧��A�p�G���U��V��A������run���A;�p�G!���U��V��A������idle���A 
         
        if (playerController.IsAnimationDone("running_atk") || playerController.IsAnimationDone("atk"))
        {
            if (playerController.PressArrowKey())
            {
                playerController.SetCurrentState(new Run(playerController));
                Debug.Log("������Run");
            }
            else 
            { 
                playerController.SetCurrentState(new Idle(playerController));
                Debug.Log("������Idle");
            }
        }

    }

    public override void FixedUpdate()
    {
        // �����ɤ]�i�H����
        playerController.Move();  
        playerController.Face();  
       
        
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("�˴���I��: " + collision.name);
        if (collision.CompareTag("Enemy"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D collision) // �ק�G�s�W OnTriggerStay2D ��k
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

