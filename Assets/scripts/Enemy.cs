using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine; 
public class Enemy : MonoBehaviour
{
    public float enemyMoveSpeed = 1f; // 重命名moveSpeed為enemyMoveSpeed
    public Transform playerTarget; // 重命名target為playerTarget
    public float startingHealth = 30f; // 初始生命值
    public float currentHealth; // 當前生命值
    public AttackManager attackManager;
    private GameManager gameManager;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // 無敵時間
    [SerializeField] private int numberOfFlashes; // 閃爍次數
    private SpriteRenderer spriteRend; // 獲取Sprite Renderer

    public bool isInvincible = false;

    void Start()
    {
        currentHealth = startingHealth; // 初始化當前生命值
        gameManager = FindAnyObjectByType<GameManager>();
        spriteRend = GetComponent<SpriteRenderer>();
        attackManager = GetComponent<AttackManager>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Vector2 direction = MoveTowardsPlayer();
        Face(direction); // 更新方向

        // 檢查是否在攻擊範圍
        if (playerTarget != null && Vector2.Distance(playerTarget.position, transform.position) <= attackManager.attackRange)
        {
            AttackPlayer();
        }
    }

    public Vector2 MoveTowardsPlayer()
    {
        Vector2 direction = Vector2.zero; // 初始化方向
        if (playerTarget != null)
        {
            direction = (playerTarget.position - transform.position).normalized;
            // 只有在距離超過範圍時才會移動
            if (Vector2.Distance(playerTarget.position, transform.position) > attackManager.attackRange)
            {
                transform.Translate(direction * enemyMoveSpeed * Time.deltaTime);
            }
        }
        return direction; // 返回方向
    }

    public void TakeDamage(float _damage)
    {
        if (isInvincible) return;
        // 減少生命值
        currentHealth -= _damage;
        Debug.Log("敵人受到傷害，目前剩餘生命值: " + currentHealth + "/" + startingHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(Invincibility());
        }
        else if (currentHealth <= 0)
        {
            EnemyDie(); // 重命名Die()方法為EnemyDie()
        }
    }

    // 敵人死亡
    private void EnemyDie() // 重命名Die()方法為EnemyDie()
    {
        Debug.Log("敵人死亡！");
        if (gameManager != null)
        {
            gameManager.PlayerScored(100);
        }
        Destroy(gameObject);
    }

    public void Face(Vector2 direction)
    {
        bool flipped = GetComponentInChildren<SpriteRenderer>().flipX;
        if (direction.x < 0 && !flipped)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = true; // 面向左
        }
        else if (direction.x > 0 && flipped)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = false; // 面向右
        }
    }

    private IEnumerator Invincibility()
    { 
        
        for (int i = 0; i < numberOfFlashes; i++)
        {
            this.spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            this.spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        } 
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackManager.attackDamage); // 對玩家造成傷害
            }
        }*/
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        /*if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.isInvincible) // 檢查是否處於無敵狀態
            {
                playerHealth.TakeDamage(attackManager.attackDamage); // 對玩家造成傷害
            }
        }*/
    }

    private void AttackPlayer()
    {
        // 觸發攻擊
       /* attackManager.PerformAttack(target);*/
    }
}
