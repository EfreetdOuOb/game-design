using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Health : MonoBehaviour
{
    private GameManager gameManager;
    public AttackManager attackManager;

    [Header("Health")]
    [SerializeField] private float startingHealth; // 初始生命值，並可以在Inspector中調整

    [SerializeField]private HpBar hpBar;
    public float currentHealth { get; private set; } // 儲存當前生命值 
    private Animator anim; 
    private bool isDead = false; // 是否死亡
    public PlayerController playerController;
    public bool isInvincible = false;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // 無敵時間
    [SerializeField] private int numberOfFlashes; // 閃爍次數
    private SpriteRenderer spriteRend; // 獲取Sprite Renderer


     
    private void Awake()
    {
        // 在開始時設置當前生命值為初始值
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        gameManager = FindFirstObjectByType<GameManager>();
        hpBar.UpdateBar(currentHealth,startingHealth);
    }
     
    void Update()
    {
        
    }

    // 玩家受傷
    public void TakeDamage(int attackDamage)
    {
        if (isDead || isInvincible) return; // 死亡或無敵時不處理傷害

        // 減少生命值
        currentHealth -= attackManager.attackDamage;
        Debug.Log("玩家受到傷害，當前剩餘生命值: " + currentHealth + "/" + startingHealth);

        if (currentHealth > 0)
        { 
            StartCoroutine(Invincibility());
            hpBar.UpdateBar(currentHealth, startingHealth);
            
             
        }
        else if (currentHealth <= 0)
        {
            hpBar.UpdateBar(currentHealth, startingHealth);
            Die();
        } 
    }

    // 玩家死亡
    private void Die()
    {
        isDead = true;
        Debug.Log("玩家死亡！"); 
        
        if (playerController != null)
        {
            playerController.Stop();
            // 切換到死亡狀態
            playerController.SetCurrentState(new Dead(playerController));
        }
        
        // 不在這裡直接調用EndGame，讓Dead狀態處理這個邏輯
        // 也不禁用PlayerController，讓狀態機來管理
    }

     

    // 玩家無敵
    private IEnumerator Invincibility()
    {
        isInvincible = true;
        Physics2D.IgnoreLayerCollision(10, 11, true);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color( 1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration/(numberOfFlashes*2));
            spriteRend.color =  Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        isInvincible = false;
        Physics2D.IgnoreLayerCollision(10, 11, false); 
    }

    internal void TakeDamage()
    {
        TakeDamage(attackManager.attackDamage);
    }
}
