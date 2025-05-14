using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    private GameManager gameManager;

    [Header("血量")]
    [SerializeField] public float maxHealth; // 初始生命值，並可以在Inspector中調整
    [SerializeField] public float currentHealth { get; private set; } // 儲存當前生命值 

    [Header("UI")]
    public UnityEvent<float ,float> OnHealthUpdate;
 

    private Animator anim; 
    private bool isDead = false; // 是否死亡
    public PlayerController playerController;
    public bool isInvincible = false; // 是否無敵（用於遠程和近戰）

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // 無敵時間
    [SerializeField] private int numberOfFlashes; // 閃爍次數
    private SpriteRenderer spriteRend; // 獲取Sprite Renderer


     
    private void Awake()
    {
        // 在開始時設置當前生命值為初始值
        currentHealth = maxHealth;
        OnHealthUpdate?.Invoke(maxHealth,currentHealth);//初始化
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        gameManager = FindFirstObjectByType<GameManager>();
    }
     
    void Update()
    {
        
    }

    // 玩家受傷
    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return; // 死亡或無敵時不處理傷害

        // 減少生命值
        currentHealth -= damage;
        Debug.Log($"玩家受到 {damage} 點傷害，當前剩餘生命值: {currentHealth}/{maxHealth}");

        if (currentHealth > 0)
        { 
            StartCoroutine(Invincibility());
        }
        else if (currentHealth <= 0)
        {
            Die();
        } 
        OnHealthUpdate?.Invoke(maxHealth,currentHealth);//更新玩家血量UI
    }


    //恢復血量
    public virtual void RestoreHealth(float value)
    {
        if(currentHealth ==maxHealth ) return;//滿血就返回

        if(currentHealth + value > maxHealth)//回血不超過最大血量
        {
            currentHealth = maxHealth;
        }else
        {
            currentHealth += value;
        }
        OnHealthUpdate?.Invoke(maxHealth,currentHealth);//更新血量UI
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
    }

    // 玩家無敵
    private IEnumerator Invincibility()
    {
        // 設置無敵狀態，這個標誌在 MonsterAttackManager 中會檢查
        isInvincible = true;
        
        // 設置物理層碰撞忽略（用於子彈等物理碰撞）
        Physics2D.IgnoreLayerCollision(10, 11, true);
        
        // 閃爍視覺效果
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f); // 紅色閃爍
            yield return new WaitForSeconds(iFramesDuration/(numberOfFlashes*2));
            spriteRend.color = Color.white; // 恢復原色
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        
        // 無敵結束
        isInvincible = false;
        Physics2D.IgnoreLayerCollision(10, 11, false); 
    }
    
    // 獲取玩家是否處於無敵狀態
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
