using UnityEngine;
using System.Collections;

public class SpiderWebBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 5f;
    private Vector2 direction;
    private static float currentSlowAmount = 1f; // 當前減速倍率
    private static float baseSlowAmount = 0.85f; // 基礎減速倍率
    private static float slowDuration = 3f; // 減速持續時間

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
                // 如果當前減速效果還在，先取消之前的減速
                if (currentSlowAmount < 1f)
                {
                    controller.ResetSpeed();
                }
                
                // 應用新的減速效果
                currentSlowAmount = baseSlowAmount;
                controller.ApplySlow(currentSlowAmount, slowDuration);
                
                // 設置定時器，在減速結束時重置速度
                StartCoroutine(ResetSlowAfterDelay(controller));
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("obj"))
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ResetSlowAfterDelay(PlayerController controller)
    {
        yield return new WaitForSeconds(slowDuration);
        currentSlowAmount = 1f;
        controller.ResetSpeed();
    }

    private void SetRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
} 