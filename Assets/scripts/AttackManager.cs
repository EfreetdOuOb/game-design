using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public float attackRange ; // �����d��
    public LayerMask enemyLayers;     // �˴����ĤH�h
    public int attackDamage ;     // �����ˮ`
    


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PerformAttack(Transform attacker)
    {
        // �ϥ� OverlapCircle �ˬd�����d�򤺪��ĤH
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attacker.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyHealth = enemy.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage); // ��ĤH�y���ˮ`
                Debug.Log(enemy.name + " �Q�����F");
            }

        } 
         
    }

     
}
