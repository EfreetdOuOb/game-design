using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private Animator animator;
    private BoxState currentState;
    public TouchArea touchArea; // 使用 public 這樣可以在 Inspector 中設置
    public bool IsOpened { get;  set; } = false;
    
    [Header("Item Info")]
    public string itemName = "";
    public Sprite itemImage;
    [TextArea(3, 5)]
    public string itemDescription = "";
    
    private void Awake()
    { 
        animator = GetComponent<Animator>();
        // 如果沒有在 Inspector 中設置，嘗試在子物件中查找
        if (touchArea == null)
        {
            touchArea = GetComponentInChildren<TouchArea>();
        }
        
        // 確保 TouchArea 知道它所屬的 Box
        if (touchArea != null)
        {
            touchArea.SetBox(this);
        }
        else
        {
            Debug.LogError("TouchArea 未找到，請確保它已被正確設置。");
        }
    }
    
    void Start()
    {
        SetCurrentState(new Unopened(this));
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }
    
    // Box 不再需要觸發器方法，由 TouchArea 負責處理
    
    //檢測是否按下F鍵
    public bool PressInteractKey()
    {
        return Input.GetKeyDown(KeyCode.F);
    }
    
    //設置玩家是否在範圍內的狀態，由 TouchArea 調用
    public void SetPlayerInRange(bool inRange)
    {
        if (currentState != null)
        {
            currentState.SetPlayerInRange(inRange);
        }
    }
    
    public void PlayAnimation(string clip)
    {
        if (animator != null)
        {
            animator.Play(clip);
        }
        else
        {
            Debug.LogError("Animator 未找到，請確保它已被正確設置。");
        }
    }

    //判斷動畫是否播放完畢(只用於一次性動畫) 
    public bool IsAnimationDone(string _aniName)
    {
        if (animator == null) return false;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName) && stateInfo.normalizedTime >= 1.0);
    }

    //設置當前狀態
    public void SetCurrentState(BoxState state)
    {
        currentState = state;
    }

    public void OpenBox()
    {
        if (IsOpened) return;
        IsOpened = true;
        // 在這裡可以觸發寶箱打開的動畫或音效等
        Debug.Log("寶箱打開了！");
        // 這裡可以添加生成物品或將物品添加到玩家背包的邏輯
    }
}
