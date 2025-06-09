using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideSpike : MonoBehaviour
{
     public GameObject hideSpikeBox;
     private Animator anim;
     public float time;
     private bool playerInRange = false;
     private float spikeTimer = 0f;
     public float spikeInterval = 1.5f; // 每隔多久自動觸發一次

     void Start()
     {
        anim = GetComponent<Animator>();
     }

     void Update()
     {
        if (playerInRange)
        {
            spikeTimer += Time.deltaTime;
            if (spikeTimer >= spikeInterval)
            {
                StartCoroutine(SpikeAttack());
                spikeTimer = 0f;
            }
        }
     }

     void OnTriggerEnter2D(Collider2D other)
     {
        if(other.gameObject.CompareTag("Player") && other.GetType().ToString() == "UnityEngine.CapsuleCollider2D")
        {
            playerInRange = true;
            
            StartCoroutine(SpikeAttack());
        }
     }

     void OnTriggerExit2D(Collider2D other)
     {
        if(other.gameObject.CompareTag("Player") && other.GetType().ToString() == "UnityEngine.CapsuleCollider2D")
        {
            playerInRange = false;
            spikeTimer = 0f;
        }
     }
     
     IEnumerator SpikeAttack()
     {
        yield return new WaitForSeconds(time);
        anim.SetTrigger("Attack");
        Instantiate(hideSpikeBox, transform.position, Quaternion.identity);
     }
}
