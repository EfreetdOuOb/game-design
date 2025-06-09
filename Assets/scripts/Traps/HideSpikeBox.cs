using UnityEngine;

public class HideSpikeBox : MonoBehaviour
{
    public int damage;
    public float destroyTime;

    private Health health;
    void Start()
    {
        health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        Destroy(gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
     {
        if(other.gameObject.CompareTag("Player")&& other.GetType().ToString() == "UnityEngine.CapsuleCollider2D")
        {
             health.TakeDamage(damage);
        }
     }
}
