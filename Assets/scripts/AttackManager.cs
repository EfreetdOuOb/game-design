using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public float attackRange ; // 攻擊範圍
    public LayerMask enemyLayers;     // 檢測敵人層
    public int attackDamage ;     // 攻擊傷害
    


    void Start()
    {
        
    }

    void Update()
    {
        
    }

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

     
}
