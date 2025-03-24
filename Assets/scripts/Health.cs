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
    [SerializeField] private float startingHealth; // ��l�ͩR�ȡA�B���\�bInspector���վ�

    [SerializeField]private HpBar hpBar;
    public float currentHealth { get; private set; } // ����ثe�ͩR�� 
    private Animator anim; 
    private bool isDead = false; // �O�_���`
    public PlayerController playerController;
    public bool isInvincible = false;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // �L�Įɶ�
    [SerializeField] private int numberOfFlashes; // �{�{����
    private SpriteRenderer spriteRend; // ����Sprite Renderer


     
    private void Awake()
    {
        // �b�}�l�ɳ]�w��e�ͩR�Ȭ���l��
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        hpBar.UpdateBar(currentHealth,startingHealth);
    }
     
    void Update()
    {
        
    }

    // �������
    public void TakeDamage(int attackDamage)
    {
        if (isDead || isInvincible  ) return; // ���⦺�`�εL�Įɤ��B�z�ˮ`

        // ��֥ͩR��
        currentHealth -=attackManager.attackDamage;
        Debug.Log("�������ˮ`�A��e�Ѿl�ͩR��: " + currentHealth + "/" + startingHealth);

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

    // ���⦺�`
    private void Die()
    {
        isDead = true;
        Debug.Log("���⦺�`�I"); 
        playerController.Stop();
        gameManager.EndGame();
        GetComponent<PlayerController>().enabled = false;
    }

     

    // ����L��
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
