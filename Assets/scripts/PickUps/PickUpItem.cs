using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class PickUpItem : MonoBehaviour
{  
    //定義PickUp的類型
    public enum PickUpType
    {
        Coin,
        HealingPotion
        
    }

    [Header("道具類型")]
    [SerializeField] private PickUpType pickUpType;
    [SerializeField] private int value;

    [Header("上拋動畫")]
    [SerializeField]private float trowHeight = 1f;
    [SerializeField]private float trowDuration = 1f;

    [Header("拾取範圍")]
    [SerializeField]private float pickUpDistance = 3f;
    [SerializeField]private float moveSpeed = 5f;
    private bool canPickUp = false;


    private Health health;

    private void Awake()
    {
        health = FindFirstObjectByType<Health>();
    }

    private void Start()
    {
        ThrowItem();
    }

    private void Update()
    {
        if(canPickUp && Vector2.Distance(transform.position,health.transform.position)<pickUpDistance)
        {
            Vector2 dir = (health.transform.position-transform.position).normalized;
            transform.Translate(dir*moveSpeed*Time.deltaTime);
        }
    }

    //上拋動畫
    private void ThrowItem()
    {
        transform.DOJump(transform.position,trowHeight,1,trowDuration)
        .OnComplete(()=>{
            canPickUp = true;
        });
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if(canPickUp && collision.gameObject.GetComponent<PlayerController>())
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
        GameManager.Instance.AddCoins(value);

        //顯示拾取金幣數值
        GameManager.Instance.ShowText("+" + value, transform.position, Color.yellow);
    }

    private void HandleHealingPotionPickUp()
    {
        //增加玩家的生命值
        health.RestoreHealth(value);

        //顯示回血數值
        GameManager.Instance.ShowText("+" + value, transform.position, Color.green);

    }


}
