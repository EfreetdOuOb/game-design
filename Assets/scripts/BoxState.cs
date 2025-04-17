using UnityEngine;


public abstract class BoxState
{
    public Box box;


    public BoxState(Box _box)
    {
        box = _box;
    }

    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void OnTriggerEnter2D(Collider2D collision);
    public abstract void OnTriggerStay2D(Collider2D collision);
    public abstract void OnTriggerExit2D(Collider2D collision);
    public abstract void OnInteract(); // 玩家按F鍵互動時觸發
}


public class Unopened : BoxState
{
    public Unopened(Box _box) : base(_box)
    { 
        //播放動畫 
        /*
        box.PlayAnimation("idle"); */
    }

    public override void Update()
    {
        // 這裡只檢測是否互動，實際互動邏輯在OnInteract中
    }


    public override void FixedUpdate()
    {
        // 箱子不需要移動
    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 偵測玩家進入範圍，實際邏輯已在Box類處理
    }

    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 玩家在箱子範圍內停留
    }
    
    public override void OnTriggerExit2D(Collider2D collision)
    {
        // 玩家離開箱子範圍
    }
    
    public override void OnInteract()
    {
        // 玩家按F鍵打開箱子
        Debug.Log("正在打開箱子...");
        box.SetCurrentState(new Opening(box));
    }
}

public class Opening : BoxState
{
    public Opening(Box _box) : base(_box)
    { 
        //播放開箱動畫
        box.PlayAnimation("open");
    }

    public override void Update()
    {
        // 檢查開箱動畫是否播放完畢
        if (box.IsAnimationDone("open"))
        {
            // 轉換到已開啟狀態
            box.SetCurrentState(new Opened(box));
            Debug.Log("箱子已打開");
        }
    }


    public override void FixedUpdate()
    {
        // 箱子不需要移動
    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 開箱過程中不處理新的觸發事件
    }

    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 開箱過程中不處理觸發事件
    }
    
    public override void OnTriggerExit2D(Collider2D collision)
    {
        // 開箱過程中不處理觸發事件
    }
    
    public override void OnInteract()
    {
        // 正在開箱過程中，不重複處理互動
    }
}

public class Opened : BoxState
{
    public Opened(Box _box) : base(_box)
    { 
        // 可能會播放一個已開啟狀態的動畫或最後一幀
        // 或者什麼也不做，維持開箱動畫的最後狀態
    }

    public override void Update()
    {
        // 箱子已打開，不需要做任何事
    }


    public override void FixedUpdate()
    {
        // 箱子不需要移動
    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        // 箱子已打開，不再處理新的觸發事件
    }

    
    public override void OnTriggerStay2D(Collider2D collision)
    {
        // 箱子已打開，不再處理觸發事件
    }
    
    public override void OnTriggerExit2D(Collider2D collision)
    {
        // 箱子已打開，不再處理觸發事件
    }
    
    public override void OnInteract()
    {
        // 箱子已打開，不再響應互動
        Debug.Log("箱子已經打開了");
    }
}




