using UnityEngine;

public class ArrowHit : MonoBehaviour
{ 
    public GameObject arrowPrefab;
    public float shootCooldown = 2f; // 射擊冷卻時間
    public float detectionRadius = 5f; // 自動檢測敵人的範圍
    
    private GameManager gameManager;
    private float shootTimer = 0f;
    private PlayerController playerController;
    private Health playerHealth;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<PlayerController>();
            playerHealth = playerTransform.GetComponent<Health>();
        }
        else
        {
            Debug.LogError("無法找到玩家物件！");
        }
    }
 
    void Update()
    {
        // 檢查是否處於戰鬥狀態
        if (gameManager != null && gameManager.isInCombat)
        {
            // 更新射擊冷卻時間
            shootTimer -= Time.deltaTime;
            
            // 如果冷卻時間已過，且玩家仍然存活，嘗試發射箭矢
            if (shootTimer <= 0 && playerController != null && playerHealth != null && playerHealth.currentHealth > 0)
            {
                Shoot();
                shootTimer = shootCooldown; // 重置射擊冷卻時間
            }
        }
    }

    void Shoot()
    {
        // 如果有敵人在範圍內，才發射箭矢
        if (AreEnemiesNearby())
        {
            Instantiate(arrowPrefab, transform.position, transform.rotation);
            Debug.Log("發射箭矢！");
        }
    }
    
    bool AreEnemiesNearby()
    {
        // 檢查是否有敵人在指定範圍內
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 用於在編輯器中顯示檢測範圍
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
