using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class EquipmentItem
{
    public string itemName;
    public string description;
    public Sprite icon;
    public float attackBonus;
    public float defenseBonus;
    public float critRateBonus;
    public float moveSpeedBonus;
    
    // 特殊裝備標記
    public bool isEnergyShield = false;
    
    // 獲取屬性描述
    public string GetStatsDescription()
    {
        if (isEnergyShield)
        {
            return "產生一個護罩於玩家周圍，\n被攻擊時會消除周遭投射物並免疫該次傷害，\n被攻擊後需要充能才能再次使用，最多疊加三層。\n\n擊殺5隻怪物可以恢復一格充能。";
        }
        
        string desc = "";
        if (attackBonus != 0) desc += $"攻擊力: +{attackBonus:F1}\n";
        if (defenseBonus != 0) desc += $"防禦力: +{defenseBonus:F1}\n";
        if (critRateBonus != 0) desc += $"暴擊率: +{critRateBonus * 100:F1}%\n";
        if (moveSpeedBonus != 0) desc += $"移動速度: +{moveSpeedBonus:F1}\n";
        return desc;
    }
}

// 裝備槽位類
[System.Serializable]
public class EquipmentSlot
{
    public string slotId;
    public string itemId;
    public int level;
    public Image slotImage;           // 槽位的圖像組件
    public Button slotButton;         // 槽位的按鈕組件
    public EquipmentItem item;        // 槽位中的裝備
    public int slotIndex;             // 槽位索引
}

public class Equipment : MonoBehaviour
{
    [Header("裝備欄")]
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private Image equippedItemImage;
    [SerializeField] private Text statsText;
    
    [Header("裝備槽位設置")]
    [SerializeField] private Transform slotsContainer;  // 裝備槽位的父容器
    [SerializeField] private EquipmentSlot[] itemSlots; // 裝備槽位數組
    [SerializeField] private GameObject emptySlotPrefab; // 空槽位預製體
    [SerializeField] private int maxSlots = 8;          // 最大槽位數量
    
    [Header("可用裝備數據")]
    [SerializeField] private List<EquipmentItem> availableEquipment = new List<EquipmentItem>();
    
    [Header("當前裝備")]
    [SerializeField] private EquipmentItem currentEquipment; // 序列化以便在Inspector中查看
    
    [Header("預設裝備")]
    [SerializeField] private EquipmentItem defaultEnergyShield;
    
    [Header("裝備設置")]
    public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
    private Dictionary<string, EquipmentSlot> equippedItems = new Dictionary<string, EquipmentSlot>();
    
    // 玩家屬性引用
    private PlayerStats playerStats;
    // 護盾系統引用
    private EnergyShield energyShield;
    
    public UnityEvent OnEquipmentChanged;
    
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        energyShield = GetComponent<EnergyShield>();
        
        // 初始設置面板為關閉
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
        
        // 確保能量護盾組件開始時為禁用狀態
        if (energyShield != null)
        {
            energyShield.enabled = false;
        }
        
        // 初始化裝備槽位
        InitializeEquipmentSlots();
        
        // 更新UI顯示
        UpdateEquipmentUI();
    }
    
    // 初始化裝備槽位
    private void InitializeEquipmentSlots()
    {
        // 如果沒有指定槽位數組，則創建新的
        if (itemSlots == null || itemSlots.Length == 0)
        {
            itemSlots = new EquipmentSlot[maxSlots];
            
            // 確保槽位容器存在
            if (slotsContainer == null)
            {
                Debug.LogError("裝備槽位容器未設置！");
                return;
            }
            
            // 創建槽位
            for (int i = 0; i < maxSlots; i++)
            {
                // 創建槽位UI
                GameObject slotObj = null;
                
                // 檢查是否已存在槽位
                if (i < slotsContainer.childCount)
                {
                    slotObj = slotsContainer.GetChild(i).gameObject;
                }
                else if (emptySlotPrefab != null)
                {
                    // 創建新的槽位
                    slotObj = Instantiate(emptySlotPrefab);
                    slotObj.transform.SetParent(slotsContainer, false);
                    slotObj.name = $"Slot_{i}";
                }
                
                if (slotObj != null)
                {
                    // 創建槽位數據
                    EquipmentSlot slot = new EquipmentSlot();
                    slot.slotImage = slotObj.GetComponentInChildren<Image>();
                    slot.slotButton = slotObj.GetComponent<Button>();
                    slot.slotIndex = i;
                    slot.item = null;
                    
                    // 添加按鈕事件
                    int index = i; // 捕獲當前索引
                    slot.slotButton.onClick.AddListener(() => OnSlotClicked(index));
                    
                    // 保存到數組
                    itemSlots[i] = slot;
                }
            }
        }
        else
        {
            // 如果已經設置了槽位數組，確保按鈕事件被正確設置
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] != null && itemSlots[i].slotButton != null)
                {
                    int index = i; // 捕獲當前索引
                    itemSlots[i].slotButton.onClick.RemoveAllListeners();
                    itemSlots[i].slotButton.onClick.AddListener(() => OnSlotClicked(index));
                    itemSlots[i].slotIndex = i;
                }
            }
        }
    }
    
    // 槽位點擊事件
    private void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= itemSlots.Length)
            return;
            
        EquipmentSlot slot = itemSlots[slotIndex];
        if (slot != null && slot.item != null)
        {
            // 裝備點擊的物品
            EquipItem(slot.item);
            
            // 可選：顯示物品信息
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowText("已裝備: " + slot.item.itemName, transform.position, Color.green);
            }
        }
    }
    
    private void Start()
    {
        // 確保能量護盾組件已啟用/禁用
        UpdateEnergyShieldComponent();
        
        // 更新裝備槽位顯示
        UpdateEquipmentSlots();
        
        // 初始化裝備字典
        foreach (var slot in equipmentSlots)
        {
            if (!string.IsNullOrEmpty(slot.itemId))
            {
                equippedItems[slot.slotId] = slot;
            }
        }
    }
    
    private void Update()
    {
        // 按TAB鍵開關裝備欄
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleEquipmentPanel();
        }
    }
    
    // 開關裝備欄
    public void ToggleEquipmentPanel()
    {
        if (equipmentPanel != null)
        {
            bool active = !equipmentPanel.activeSelf;
            equipmentPanel.SetActive(active);
            
            if (active)
            {
                UpdateEquipmentUI();
                UpdateEquipmentSlots(); // 更新所有裝備槽
            }
        }
    }
    
    // 更新UI顯示
    private void UpdateEquipmentUI()
    {
        if (currentEquipment != null)
        {
            // 顯示當前裝備圖標
            if (equippedItemImage != null)
            {
                equippedItemImage.sprite = currentEquipment.icon;
                equippedItemImage.color = Color.white;
            }
            
            // 顯示裝備屬性
            if (statsText != null)
            {
                statsText.text = currentEquipment.GetStatsDescription();
            }
            
            // 輸出調試信息
            Debug.Log($"當前裝備: {currentEquipment.itemName}，是否為能量護盾: {currentEquipment.isEnergyShield}");
        }
        else
        {
            // 沒有裝備時的顯示
            if (equippedItemImage != null)
            {
                equippedItemImage.sprite = null;
                equippedItemImage.color = new Color(1, 1, 1, 0.5f);
            }
            
            if (statsText != null)
            {
                statsText.text = "未裝備任何物品";
            }
            
            Debug.Log("當前未裝備任何物品");
        }
        
        // 更新能量護盾組件的啟用狀態
        UpdateEnergyShieldComponent();
    }
    
    // 更新裝備槽位顯示
    private void UpdateEquipmentSlots()
    {
        // 確保槽位數組已初始化
        if (itemSlots == null)
            return;
            
        // 首先隱藏所有槽位
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].slotButton != null)
            {
                // 僅顯示與玩家擁有的裝備相對應的槽位
                itemSlots[i].slotButton.gameObject.SetActive(i < availableEquipment.Count);
            }
        }
        
        // 填充可用裝備
        for (int i = 0; i < availableEquipment.Count && i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].slotImage != null && availableEquipment[i] != null)
            {
                itemSlots[i].item = availableEquipment[i];
                itemSlots[i].slotImage.sprite = availableEquipment[i].icon;
                itemSlots[i].slotImage.color = Color.white;
                
                // 高亮顯示當前裝備的槽位
                if (currentEquipment != null && currentEquipment == availableEquipment[i])
                {
                    // 可以添加邊框或其他視覺效果來突出顯示
                    itemSlots[i].slotImage.color = new Color(1f, 1f, 0.5f, 1f); // 淡黃色高亮
                }
                
                // 添加工具提示
                TooltipTrigger tooltip = itemSlots[i].slotButton.GetComponent<TooltipTrigger>();
                if (tooltip != null)
                {
                    tooltip.header = availableEquipment[i].itemName;
                    tooltip.content = availableEquipment[i].GetStatsDescription();
                    Debug.Log("已設置槽位" + i + "的工具提示：" + availableEquipment[i].itemName);
                }
                else
                {
                    // 如果沒有工具提示組件，添加一個
                    tooltip = itemSlots[i].slotButton.gameObject.AddComponent<TooltipTrigger>();
                    tooltip.header = availableEquipment[i].itemName;
                    tooltip.content = availableEquipment[i].GetStatsDescription();
                    Debug.Log("已為槽位" + i + "添加工具提示組件：" + availableEquipment[i].itemName);
                }
            }
        }
    }
    
    // 裝備一個物品
    public void EquipItem(EquipmentItem item)
    {
        if (item == null)
        {
            Debug.LogError("【裝備系統】嘗試裝備空物品");
            return;
        }
        
        // 先解除當前裝備
        UnequipCurrentItem();
        
        // 設置新裝備
        currentEquipment = item;
        Debug.Log($"【裝備系統】裝備物品: {item.itemName}，是護盾：{item.isEnergyShield}");
        
        // 應用裝備效果
        ApplyEquipmentEffects();
        
        // 更新UI
        UpdateEquipmentUI();
        
        // 更新槽位顯示（高亮當前裝備）
        UpdateEquipmentSlots();
        
        // 觸發裝備變更事件
        OnEquipmentChanged?.Invoke();
    }
    
    // 解除當前裝備
    public void UnequipCurrentItem()
    {
        if (currentEquipment != null)
        {
            // 移除當前裝備效果
            RemoveEquipmentEffects();
            
            currentEquipment = null;
            
            // 更新UI
            UpdateEquipmentUI();
            
            // 觸發裝備變更事件
            OnEquipmentChanged?.Invoke();
        }
    }
    
    // 應用裝備效果
    private void ApplyEquipmentEffects()
    {
        if (currentEquipment == null) return;
        
        // 應用基本屬性加成
        if (playerStats != null)
        {
            playerStats.UpdateEquipmentBonus(
                currentEquipment.attackBonus,
                currentEquipment.defenseBonus,
                currentEquipment.critRateBonus,
                currentEquipment.moveSpeedBonus
            );
            
            Debug.Log($"【裝備系統】已應用屬性加成 - 攻擊:{currentEquipment.attackBonus}，防禦:{currentEquipment.defenseBonus}");
        }
        
        // 應用特殊裝備效果
        if (currentEquipment.isEnergyShield)
        {
            Debug.Log("【裝備系統】正在應用能量護盾效果...");
            
            // 檢查能量護盾組件
            if (energyShield == null)
            {
                energyShield = GetComponent<EnergyShield>();
                if (energyShield == null)
                {
                    Debug.Log("【裝備系統】添加新的能量護盾組件");
                    energyShield = gameObject.AddComponent<EnergyShield>();
                }
            }
            
            // 確保能量護盾組件已啟用
            if (energyShield != null)
            {
                if (!energyShield.enabled)
                {
                    energyShield.enabled = true;
                    Debug.Log("【裝備系統】成功啟用能量護盾組件");
                }
                else
                {
                    Debug.Log("【裝備系統】能量護盾組件已經處於啟用狀態");
                }
            }
            else
            {
                Debug.LogError("【裝備系統】無法創建或啟用能量護盾組件");
            }
        }
        else
        {
            // 非能量護盾裝備，確保護盾組件被禁用
            if (energyShield != null && energyShield.enabled)
            {
                energyShield.enabled = false;
                Debug.Log("【裝備系統】已禁用能量護盾組件（當前裝備非護盾）");
            }
        }
    }
    
    // 移除裝備效果
    private void RemoveEquipmentEffects()
    {
        if (currentEquipment == null) return;
        
        // 重置基本屬性加成
        if (playerStats != null)
        {
            playerStats.UpdateEquipmentBonus(0, 0, 0, 0);
        }
        
        // 移除特殊裝備效果
        if (currentEquipment.isEnergyShield)
        {
            // 禁用能量護盾組件
            if (energyShield != null)
            {
                energyShield.enabled = false;
            }
        }
    }
    
    // 更新能量護盾組件的啟用狀態
    private void UpdateEnergyShieldComponent()
    {
        if (energyShield != null)
        {
            bool shouldBeEnabled = (currentEquipment != null && currentEquipment.isEnergyShield);
            energyShield.enabled = shouldBeEnabled;
        }
    }
    
    // 獲取當前裝備
    public EquipmentItem GetCurrentEquipment()
    {
        return currentEquipment;
    }
    
    // 創建充能護盾裝備實例
    public EquipmentItem CreateEnergyShieldItem()
    {
        EquipmentItem energyShield = new EquipmentItem
        {
            itemName = "充能護盾",
            description = "產生一個護罩於玩家周圍，被攻擊時會消除周遭投射物並免疫該次傷害，被攻擊後需要充能才能再次使用，最多疊加三層。",
            isEnergyShield = true,
            // 可以加一些基本屬性
            defenseBonus = 2f,
            // icon屬性需要在Inspector中設置
        };
        
        return energyShield;
    }
    
    // 針對測試用 - 隨機獲取並裝備一個物品
    public void EquipRandomItem()
    {
        if (availableEquipment.Count > 0)
        {
            int randomIndex = Random.Range(0, availableEquipment.Count);
            EquipItem(availableEquipment[randomIndex]);
        }
    }
    
    // 添加新裝備到可用列表
    public void AddEquipment(EquipmentItem item)
    {
        if (item == null)
        {
            Debug.LogError("【裝備系統】嘗試添加空物品");
            return;
        }
        
        Debug.Log($"【裝備系統】添加裝備：{item.itemName}，是護盾：{item.isEnergyShield}");
        
        // 檢查是否已經存在相同物品
        bool exists = false;
        foreach (var existingItem in availableEquipment)
        {
            if (existingItem.itemName == item.itemName)
            {
                exists = true;
                Debug.Log($"【裝備系統】物品 {item.itemName} 已存在於列表中，不再添加");
                break;
            }
        }
        
        if (!exists)
        {
            availableEquipment.Add(item);
            Debug.Log($"【裝備系統】已將物品 {item.itemName} 添加到可用列表，當前列表數量：{availableEquipment.Count}");
            
            // 更新槽位顯示
            UpdateEquipmentSlots();
        }
    }
    
    // 檢查裝備面板是否開啟
    public bool IsEquipmentPanelOpen()
    {
        return equipmentPanel != null && equipmentPanel.activeSelf;
    }
    
    // 公開方法用於強制更新UI
    public void UpdateUI()
    {
        // 更新裝備UI
        UpdateEquipmentUI();
        // 更新槽位顯示
        UpdateEquipmentSlots();
        // 確保能量護盾狀態正確
        UpdateEnergyShieldComponent();
        
        Debug.Log($"【裝備系統】強制更新UI，當前裝備：{(currentEquipment != null ? currentEquipment.itemName : "無")}");
    }

    // 獲取已裝備的物品
    public Dictionary<string, int> GetEquippedItems()
    {
        var result = new Dictionary<string, int>();
        foreach (var item in equippedItems)
        {
            result[item.Key] = item.Value.level;
        }
        return result;
    }

    // 載入已裝備的物品
    public void LoadEquippedItems(Dictionary<string, int> items)
    {
        equippedItems.Clear();
        foreach (var item in items)
        {
            var equipmentItem = availableEquipment.Find(e => e.itemName == item.Key);

            // 如果找不到，針對充能護盾自動補全
            if (equipmentItem == null)
            {
                if (item.Key == "充能護盾")
                {
                    equipmentItem = new EquipmentItem
                    {
                        itemName = "充能護盾",
                        description = "產生一個護罩於玩家周圍，被攻擊時會消除周遭投射物並免疫該次傷害，被攻擊後需要充能才能再次使用，最多疊加三層。",
                        isEnergyShield = true,
                        defenseBonus = 2f,
                        // 你可以根據實際情況補上 icon
                    };
                    availableEquipment.Add(equipmentItem);
                    Debug.Log("【裝備系統】自動補全充能護盾到 availableEquipment");
                }
                else
                {
                    // 其他裝備可依需求補全
                    equipmentItem = new EquipmentItem { itemName = item.Key };
                    availableEquipment.Add(equipmentItem);
                }
            }

            EquipItem(equipmentItem);
            Debug.Log($"【裝備系統】已重新裝備物品：{equipmentItem.itemName}");
        }

        UpdateEquipmentUI();
        UpdateEquipmentSlots();
        OnEquipmentChanged?.Invoke();
    }

    // 裝備物品
    public bool EquipItem(string slotId, string itemId, int level)
    {
        var slot = equipmentSlots.Find(s => s.slotId == slotId);
        if (slot == null)
            return false;

        // 卸下當前裝備
        if (equippedItems.ContainsKey(slotId))
        {
            UnequipItem(slotId);
        }

        // 裝備新物品
        slot.itemId = itemId;
        slot.level = level;
        equippedItems[slotId] = slot;

        // 更新玩家屬性
        UpdatePlayerStats();

        OnEquipmentChanged?.Invoke();
        return true;
    }

    // 卸下裝備
    public void UnequipItem(string slotId)
    {
        if (equippedItems.ContainsKey(slotId))
        {
            var slot = equippedItems[slotId];
            slot.itemId = null;
            slot.level = 0;
            equippedItems.Remove(slotId);

            // 更新玩家屬性
            UpdatePlayerStats();

            OnEquipmentChanged?.Invoke();
        }
    }

    // 更新玩家屬性
    private void UpdatePlayerStats()
    {
        if (playerStats == null)
            return;

        float attackBonus = 0;
        float defenseBonus = 0;
        float critRateBonus = 0;
        float moveSpeedBonus = 0;
        float maxHealthBonus = 0;

        // 計算所有裝備的加成
        foreach (var item in equippedItems.Values)
        {
            // 這裡可以根據物品ID和等級計算具體加成
            // 這是一個示例計算方式
            attackBonus += item.level * 2;
            defenseBonus += item.level;
            critRateBonus += item.level * 0.01f;
            moveSpeedBonus += item.level * 0.5f;
            maxHealthBonus += item.level * 5;
        }

        // 更新玩家屬性
        playerStats.UpdateEquipmentBonus(
            attackBonus,
            defenseBonus,
            critRateBonus,
            moveSpeedBonus,
            maxHealthBonus
        );
    }
} 