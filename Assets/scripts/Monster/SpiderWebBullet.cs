using UnityEngine;

public class SpiderWebBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    private Vector2 direction;

    public void SetDirection(Vector2 dir) { direction = dir.normalized; SetRotation(-dir); }

    void Start() { Destroy(gameObject, lifeTime); }

    void Update() { transform.position += (Vector3)direction * speed * Time.deltaTime; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ApplySlow(0.85f, 3f); // 速度降為85%，持續3秒
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("obj"))
        {
            Destroy(gameObject);
        }
    }

    private void SetRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
} 