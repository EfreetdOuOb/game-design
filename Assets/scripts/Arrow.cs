using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 10f; // 箭矢造成的傷害值
    public float lifeTime = 5f; // 箭矢存在時間
    public float knockbackForce = 1.5f; // 箭矢擊中怪物的擊退力度
    private int objLayerNumber; // 用於存儲 "obj" 圖層的編號
    private Vector2 lastFramePosition; // 上一幀的位置

    private void Start()
    {
        // 設置一個定時器，讓箭矢在一段時間後自動銷毀
        Destroy(gameObject, lifeTime);
        
        // 獲取 "obj" 圖層的編號
        objLayerNumber = LayerMask.NameToLayer("obj");
        
        // 初始化位置記錄
        lastFramePosition = transform.position;
    }
    
    private void Update()
    {
        // 在每一幀更新位置記錄
        lastFramePosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 檢查是否擊中敵人
        if (collision.CompareTag("Enemy"))
        {
            bool damageDealt = false;
            
            // 檢查是否是Monster類型（新的敵人系統）
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                // 獲取箭矢飛行方向作為擊退方向的參考
                Vector2 arrowDirection = (transform.position - (Vector3)lastFramePosition).normalized;
                
                // 對怪物造成傷害並擊退
                monster.TakeDamage(damage);
                
                // 如果怪物沒有被擊退（可能在TakeDamage中已經處理），則直接調用擊退方法
                if (!monster.IsBeingKnockedBack())
                {
                    // 使用箭矢位置作為擊退源
                    monster.Knockback(lastFramePosition, knockbackForce);
                }
                
                Debug.Log("箭矢擊中怪物，造成 " + damage + " 點傷害！怪物當前生命值：" + monster.currentHealth);
                damageDealt = true;
            }
            
            // 如果不是Monster，檢查是否是舊的Enemy類型
            if (!damageDealt)
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log("箭矢擊中敵人(舊系統)，造成 " + damage + " 點傷害！");
                    damageDealt = true;
                }
            }
            
            // 箭矢擊中敵人後銷毀
            Destroy(gameObject);
        }
        // 檢查是否擊中 "obj" 圖層的物體
        else if (collision.gameObject.layer == objLayerNumber)
        {
            Debug.Log("箭矢擊中 obj 圖層物體，箭矢被摧毀");
            // 箭矢擊中 "obj" 圖層物體後銷毀
            Destroy(gameObject);
        }
    }
}
