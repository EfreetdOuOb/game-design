using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public float attackRange ; // 攻擊範圍
    public LayerMask enemyLayers;     // 檢測敵人層
    public int attackDamage ;     // 攻擊傷害
    
    // 是否正在執行攻擊動畫
    private bool isAttacking = false;
    // 動畫事件判定是否已造成傷害
    private bool hasDamaged = false;
    // 目標
    private Transform currentTarget;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // 傳統的攻擊方法，通過範圍檢測
    public void PerformAttack(Transform attacker)
    {
        // 使用 OverlapCircle 檢查攻擊範圍內的敵人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attacker.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyHealth = enemy.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage); // 對敵人造成傷害
                Debug.Log(enemy.name + " 被攻擊了");
            }
        } 
    }
    
    // 攻擊開始方法 - 被動畫調用
    public void StartAttacking(Transform target)
    {
        isAttacking = true;
        hasDamaged = false;
        currentTarget = target;
    }
    
    // 攻擊判定方法 - 在動畫中的特定幀觸發（通過Animation Event）
    public void AttackTrigger()
    {
        if (!isAttacking || hasDamaged || currentTarget == null) return;
        
        // 檢查攻擊範圍內是否有玩家
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            Health playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.isInvincible)
            {
                playerHealth.TakeDamage(attackDamage);
                hasDamaged = true;
                Debug.Log("怪物攻擊命中玩家");
            }
        }
    }
    
    // 攻擊結束方法 - 被動畫調用
    public void StopAttacking()
    {
        isAttacking = false;
        hasDamaged = false;
        currentTarget = null;
    }
     
    // 在Unity編輯器中繪製攻擊範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
