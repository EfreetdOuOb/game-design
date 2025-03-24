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
    [SerializeField] private float startingHealth; // 初始生命值，且允許在Inspector中調整

    [SerializeField]private HpBar hpBar;
    public float currentHealth { get; private set; } // 角色目前生命值 
    private Animator anim; 
    private bool isDead = false; // 是否死亡
    public PlayerController playerController;
    public bool isInvincible = false;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // 無敵時間
    [SerializeField] private int numberOfFlashes; // 閃爍次數
    private SpriteRenderer spriteRend; // 角色Sprite Renderer


     
    private void Awake()
    {
        // 在開始時設定當前生命值為初始值
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        hpBar.UpdateBar(currentHealth,startingHealth);
    }
     
    void Update()
    {
        
    }

    // 角色受傷
    public void TakeDamage(int attackDamage)
    {
        if (isDead || isInvincible  ) return; // 角色死亡或無敵時不處理傷害

        // 減少生命值
        currentHealth -=attackManager.attackDamage;
        Debug.Log("角色受到傷害，當前剩餘生命值: " + currentHealth + "/" + startingHealth);

        if (currentHealth>0)
        { 
            StartCoroutine(Invincibility());
            hpBar.UpdateBar(currentHealth, startingHealth);
        }
        else if (currentHealth<=0)
        {
            hpBar.UpdateBar(currentHealth, startingHealth);
            Die();
        } 
    }

    // 角色死亡
    private void Die()
    {
        isDead = true;
        Debug.Log("角色死亡！"); 
        playerController.Stop();
        gameManager.EndGame();
        GetComponent<PlayerController>().enabled = false;
    }

     

    // 角色無敵
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
        throw new NotImplementedException();
    }
}
