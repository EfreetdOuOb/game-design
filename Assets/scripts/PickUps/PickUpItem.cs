using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{  
    //定義PickUp的類型
    public enum PickUpType
    {
        Coin,
        HealingPotion
        
    }
    [SerializeField] private PickUpType pickUpType;
    [SerializeField] private int value;

    private Health health;

    private void Awake()
    {
        health = FindFirstObjectByType<Health>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>())
        {
            CollectPickUp();
        }
    }
    //根據類型來執行不同邏輯
    private void CollectPickUp()
    {
        switch(pickUpType)
        {
            case PickUpType.Coin:
                HandleCoinPickUp();
                break;

            case PickUpType.HealingPotion:
                HandleHealingPotionPickUp();
                break;
                
        }
        //銷毀道具
        Destroy(gameObject); 
    }
    //拾取金幣後的邏輯
    private void HandleCoinPickUp()
    {
        //增加玩家的金幣數量

    }

    private void HandleHealingPotionPickUp()
    {
        //增加玩家的生命值
        health.RestoreHealth(value);

    }


}
