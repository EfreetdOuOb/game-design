using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 10f; // 箭矢造成的傷害值
    public float lifeTime = 5f; // 箭矢存在時間
    private int objLayerNumber; // 用於存儲 "obj" 圖層的編號

    private void Start()
    {
        // 設置一個定時器，讓箭矢在一段時間後自動銷毀
        Destroy(gameObject, lifeTime);
        
        // 獲取 "obj" 圖層的編號
        objLayerNumber = LayerMask.NameToLayer("obj");
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
                monster.TakeDamage(damage);
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
