using UnityEngine;
using System.Collections;

public class SpiderPoisonBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    private Vector2 direction;
    private Animator animator;
    private bool isBooming = false;

    public void SetDirection(Vector2 dir) { direction = dir.normalized; SetRotation(-dir); }

    void Start() {
        animator = GetComponent<Animator>();
        Destroy(gameObject, lifeTime);
    }

    void Update() {
        if (!isBooming)
            transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isBooming) return;
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                health.ApplyPoison(5, 3f); // 每秒5滴，持續3秒
            }
            StartCoroutine(BoomAndDestroy());
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("obj"))
        {
            StartCoroutine(BoomAndDestroy());
        }
    }

    private IEnumerator BoomAndDestroy()
    {
        isBooming = true;
        if (animator) animator.Play("boom");
        yield return new WaitUntil(() => animator == null || animator.GetCurrentAnimatorStateInfo(0).IsName("boom") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        Destroy(gameObject);
    }

    private void SetRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
} 