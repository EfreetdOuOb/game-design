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
    private int objLayerNumber; // 用於存儲 "obj" 圖層的編號

    void Awake() // 將 gameManager 的獲取放在 Awake，確保更早
    {
        // 獲取 "obj" 圖層的編號
        objLayerNumber = LayerMask.NameToLayer("obj");
    }

    void OnEnable() // 在啟用時獲取 PlayerController 和 Health
    {
        // 確保在物件啟用時獲取 GameManager 和 PlayerController 的引用
        gameManager = FindAnyObjectByType<GameManager>();

        // 使用 PlayerController.Instance 更可靠，前提是 PlayerController 是單例
        if (PlayerController.Instance != null)
        {
            playerController = PlayerController.Instance;
            playerHealth = playerController.GetComponent<Health>();
        }
        else
        {
            // 如果 PlayerController 不是單例，則繼續使用 FindAnyObjectByType
            Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform != null)
            {
                playerController = playerTransform.GetComponent<PlayerController>();
                playerHealth = playerTransform.GetComponent<Health>();
            }
            else
            {
                Debug.LogError("ArrowHit: 無法找到玩家物件或 PlayerController！");
            }
        }
    }

    void Update()
    {
        // 戰鬥狀態的檢查已移除，現在只檢查冷卻時間和玩家狀態
        // 更新射擊冷卻時間
        shootTimer -= Time.deltaTime;
        
        // 如果冷卻時間已過，且玩家仍然存活，嘗試發射箭矢
        if (shootTimer <= 0 && playerController != null && playerHealth != null && playerHealth.currentHealth > 0)
        {
            Shoot();
            shootTimer = shootCooldown; // 重置射擊冷卻時間
        }
    }

    void Shoot()
    {
        // 如果有敵人在範圍內，才發射箭矢
        if (AreEnemiesNearby())
        {
            // 根據玩家的 localScale 判斷面向方向
            // 玩家面向左邊時 localScale.x 為正數，面向右邊時為負數
            bool isFacingRight = playerController.transform.localScale.x < 0;

            // 設置箭矢旋轉角度
            Quaternion arrowRotation = Quaternion.Euler(0, isFacingRight ? 180 : 0, 0); // 180度翻轉箭矢

            // 實例化箭矢並設定正確方向
            GameObject arrow = Instantiate(arrowPrefab, transform.position, arrowRotation);

            // 如果箭矢有Rigidbody2D，設置其初始速度
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                float direction = isFacingRight ? 1 : -1; // 根據面向方向設置速度
                arrowRb.linearVelocity = new Vector2(direction * 10f, 0); // 箭矢速度，可以調整
            }

            
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 檢查是否擊中 "obj" 圖層的物體
        if (collision.gameObject.layer == objLayerNumber)
        {
            Debug.Log("箭矢擊中 obj 圖層物體，箭矢被摧毀");
            // 箭矢擊中 "obj" 圖層物體後銷毀
            Destroy(gameObject);
        }
        
        // 其他碰撞處理...
    }
}
