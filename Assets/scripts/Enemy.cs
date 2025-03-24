using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine; 
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 1f;
    public Transform target;
    public float startingHealth = 30f; // ��l�ͩR��
    public float currentHealth; // �ثe�ͩR��
    public AttackManager attackManager;
    private GameManager gameManager;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // �L�Įɶ�
    [SerializeField] private int numberOfFlashes; // �{�{����
    private SpriteRenderer spriteRend; // ����Sprite Renderer

    public bool isInvincible = false;

    void Start()
    {
        currentHealth = startingHealth; // ��l�Ʒ�e�ͩR��
        gameManager = FindObjectOfType<GameManager>();
        spriteRend = GetComponent<SpriteRenderer>();
        attackManager = GetComponent<AttackManager>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Vector2 direction = MoveTowardsPlayer();
        Face(direction); // ��s�¦V

        // �ˬd�O�_�b�����d��
        if (target != null && Vector2.Distance(target.position, transform.position) <= attackManager.attackRange)
        {
            AttackPlayer();
        }
    }

    public Vector2 MoveTowardsPlayer()
    {
        Vector2 direction = Vector2.zero; // ��l�Ƥ�V
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
            // �u���b�Z������󻷮ɤ~�|����
            if (Vector2.Distance(target.position, transform.position) > attackManager.attackRange)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);
            }
        }
        return direction; // ��^��V
    }

    public void TakeDamage(float _damage)
    {
        if (isInvincible) return;
        // ��֥ͩR��
        currentHealth -= _damage;
        Debug.Log("�Ǫ�����ˮ`�A��e�Ѿl�ͩR��: " + currentHealth + "/" + startingHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(Invincibility());
        }
        else if (currentHealth <= 0)
        {
            Die();
        }
    }

    // �Ǫ����`
    private void Die()
    {
        Debug.Log("�Ǫ����`�I");
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
            GetComponentInChildren<SpriteRenderer>().flipX = true; // ���¥�
        }
        else if (direction.x > 0 && flipped)
        {
            GetComponentInChildren<SpriteRenderer>().flipX = false; // ���¥k
        }
    }

    private IEnumerator Invincibility()
    {
        isInvincible = true;
        Physics2D.IgnoreLayerCollision(10, 11, true); // �����P���a���I��
        for (int i = 0; i < numberOfFlashes; i++)
        {
            this.spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            this.spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        isInvincible = false;
        Physics2D.IgnoreLayerCollision(10, 11, false); // ��_�P���a���I��
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackManager.attackDamage); // �缾�a�y���ˮ`
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.isInvincible) // �ˬd�O�_�B��L�Ī��A
            {
                playerHealth.TakeDamage(attackManager.attackDamage); // �缾�a�y���ˮ`
            }
        }
    }

    private void AttackPlayer()
    {
        // Ĳ�o����
        attackManager.PerformAttack(target);
    }
}
