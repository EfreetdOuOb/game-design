using UnityEngine;

public abstract class BoxState
{
    public Box box;
    protected bool playerInRange = false;

    public BoxState(Box _box)
    {
        box = _box;
    }

    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void OnTriggerEnter2D(Collider2D collision);
    public abstract void OnTriggerStay2D(Collider2D collision);
    public abstract void OnTriggerExit2D(Collider2D collision); 
    
    public virtual void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }
}

public class Unopened : BoxState
{
    public Unopened(Box _box) : base(_box) 
    {
        //播放動畫 
        box.PlayAnimation("idle");
    }

    public override void Update()
    {
        // 只在玩家進入範圍時檢測按鍵
        if (playerInRange && box.PressInteractKey())
        {
            Debug.Log("玩家按下 F 鍵，開始打開箱子");
            box.SetCurrentState(new Opening(box));
        }
    }

    public override void FixedUpdate() 
    {
        // 箱子不需要移動
    }

    // 以下方法現在由 TouchArea 負責，但保留提供一致性接口
    public override void OnTriggerEnter2D(Collider2D collision) {}
    public override void OnTriggerStay2D(Collider2D collision) {}
    public override void OnTriggerExit2D(Collider2D collision) {}
}

public class Opening : BoxState
{
    private bool hasOpened = false;

    public Opening(Box _box) : base(_box)
    { 
        //播放開箱動畫  
        box.PlayAnimation("open");
        Debug.Log("【箱子】播放開箱動畫");
    }

    public override void Update()
    {
        if (box.IsAnimationDone("open") && !hasOpened)
        {
            // 調用Box的OpenBox方法，實際生成裝備
            box.OpenBox();
            hasOpened = true;
            Debug.Log("【箱子】箱子開啟動畫完成，已呼叫OpenBox生成裝備");
            
            // 延遲一點時間後再切換狀態，確保其他操作完成
            // 這裡不立即設置IsOpened，因為OpenBox方法中會設置
        }
    }

    public override void FixedUpdate() 
    {
        // 箱子不需要移動
    }

    // 以下方法現在由 TouchArea 負責，但保留提供一致性接口
    public override void OnTriggerEnter2D(Collider2D collision) {}
    public override void OnTriggerStay2D(Collider2D collision) {}
    public override void OnTriggerExit2D(Collider2D collision) {}
}