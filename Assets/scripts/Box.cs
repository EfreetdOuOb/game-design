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
    
    [Header("裝備設置")]
    [Tooltip("寶箱生成裝備的機率 (0-1)")]
    [Range(0, 1)]
    public float equipmentDropChance = 0.3f; // 30% 機率獲得裝備
    
    [Tooltip("可能掉落的裝備圖標")]
    public Sprite[] equipmentIcons; // 設置不同類型裝備的圖標
    
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
        if (IsOpened) 
        {
            Debug.Log("【箱子】此箱子已經開啟過了");
            return;
        }
        
        // 寶箱打開動畫和音效
        Debug.Log("【箱子】寶箱開啟了！開始生成裝備...");
        
        // 先嘗試生成裝備
        TryGenerateEquipment();
        
        // 最後設置為已開啟狀態
        IsOpened = true;
    }
    
    // 嘗試生成裝備
    private void TryGenerateEquipment()
    {
        try
        {
            // 確保ItemInfo已設置
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError("【箱子錯誤】寶箱名稱為空，無法生成裝備");
                return;
            }
            
            if (itemImage == null)
            {
                Debug.LogError("【箱子錯誤】寶箱圖像為空，無法生成裝備");
                return;
            }
            
            Debug.Log($"【箱子】正在生成裝備: {itemName}，描述：{itemDescription}");
            
            // 創建裝備物品
            EquipmentItem newEquipment = CreateEquipmentFromBoxData();
            
            // 轉移給玩家
            TransferEquipmentToPlayer(newEquipment);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"【箱子錯誤】生成或轉移裝備時發生異常: {e.Message}\n{e.StackTrace}");
        }
    }

    // 創建裝備物品
    private EquipmentItem CreateEquipmentFromBoxData()
    {
        EquipmentItem newEquipment = new EquipmentItem();
        
        // 使用箱子的ItemInfo數據
        newEquipment.itemName = this.itemName;
        newEquipment.description = this.itemDescription;
        newEquipment.icon = this.itemImage;
        
        // 根據名稱判斷是否為能量護盾
        bool isShield = this.itemName.Contains("充能護盾") || this.itemName.Contains("能量護盾");
        newEquipment.isEnergyShield = isShield;
        
        if (isShield)
        {
            newEquipment.defenseBonus = 2f; // 給予適當的防禦加成
            Debug.Log("【箱子】已創建能量護盾裝備，isEnergyShield設為：" + newEquipment.isEnergyShield);
        }
        else
        {
            // 對於非護盾裝備，設置一些默認屬性（可以根據實際需要調整）
            newEquipment.attackBonus = 5f;
            newEquipment.defenseBonus = 3f;
            newEquipment.critRateBonus = 0.1f;
            newEquipment.moveSpeedBonus = 0.5f;
            Debug.Log("【箱子】已創建普通裝備: " + newEquipment.itemName);
        }
        
        return newEquipment;
    }

    // 轉移裝備給玩家
    private void TransferEquipmentToPlayer(EquipmentItem equipment)
    {
        if (equipment == null)
        {
            Debug.LogError("【箱子錯誤】無法轉移空裝備");
            return;
        }
        
        // 獲取玩家的裝備系統組件
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("【箱子錯誤】無法找到Player標籤的遊戲物體");
            return;
        }
        
        Equipment playerEquipment = playerObj.GetComponent<Equipment>();
        if (playerEquipment == null)
        {
            Debug.LogError("【箱子錯誤】無法找到玩家的Equipment組件");
            return;
        }
        
        Debug.Log($"【箱子】找到玩家的Equipment組件，準備添加裝備: {equipment.itemName}，isEnergyShield: {equipment.isEnergyShield}");
        
        // 添加裝備到玩家的可用裝備列表
        playerEquipment.AddEquipment(equipment);
        Debug.Log($"【箱子】已將{equipment.itemName}添加到玩家的可用裝備列表");
        
        // 直接裝備到玩家身上
        playerEquipment.EquipItem(equipment);
        Debug.Log($"【箱子】已將{equipment.itemName}裝備到玩家身上");
        
        // 強制更新裝備界面
        playerEquipment.UpdateUI();
        
        // 顯示獲得裝備的提示
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowText("獲得並裝備: " + equipment.itemName, 
                transform.position, Color.yellow);
        }
        
        // 驗證能量護盾腳本是否已啟用
        if (equipment.isEnergyShield)
        {
            Debug.Log("【箱子】檢查能量護盾腳本是否已啟用...");
            EnergyShield shield = playerObj.GetComponent<EnergyShield>();
            
            if (shield != null)
            {
                if (!shield.enabled)
                {
                    shield.enabled = true;
                    Debug.Log("【箱子】手動啟用能量護盾組件");
                }
                else
                {
                    Debug.Log("【箱子】能量護盾組件已處於啟用狀態");
                }
            }
            else
            {
                Debug.LogError("【箱子錯誤】玩家沒有EnergyShield組件，嘗試添加一個");
                shield = playerObj.AddComponent<EnergyShield>();
                if (shield != null)
                {
                    shield.enabled = true;
                    Debug.Log("【箱子】已添加並啟用新的EnergyShield組件");
                }
            }
        }
    }
    
    // 創建隨機裝備
    private EquipmentItem CreateRandomEquipment()
    {
        EquipmentItem item = new EquipmentItem();
        
        // 隨機決定裝備類型 (0-攻擊, 1-防禦, 2-暴擊, 3-速度, 4-護盾)
        int type = Random.Range(0, 5);
        
        switch (type)
        {
            case 0: // 攻擊裝備
                item.itemName = "鋒利匕首";
                item.description = "增加攻擊力";
                item.attackBonus = Random.Range(3f, 8f);
                break;
                
            case 1: // 防禦裝備
                item.itemName = "鐵甲護盾";
                item.description = "增加防禦力";
                item.defenseBonus = Random.Range(2f, 5f);
                break;
                
            case 2: // 暴擊裝備
                item.itemName = "獵人護符";
                item.description = "增加暴擊率";
                item.critRateBonus = Random.Range(0.05f, 0.15f);
                break;
                
            case 3: // 速度裝備
                item.itemName = "疾風靴";
                item.description = "增加移動速度";
                item.moveSpeedBonus = Random.Range(0.5f, 1.5f);
                break;
                
            case 4: // 能量護盾 (特殊裝備)
                item.itemName = "充能護盾";
                item.description = "被攻擊時提供保護";
                item.isEnergyShield = true;
                // 可以添加一些基本屬性加成
                item.defenseBonus = Random.Range(1f, 3f);
                break;
        }
        
        // 設置裝備圖標
        if (equipmentIcons != null && equipmentIcons.Length > type)
        {
            item.icon = equipmentIcons[type];
        }
        else
        {
            // 如果沒有設置圖標，使用寶箱的物品圖標作為默認
            item.icon = itemImage;
        }
        
        return item;
    }
}
