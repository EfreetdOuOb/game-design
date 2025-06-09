using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq; // 添加這行以使用 ElementAt

[System.Serializable]
public class Skill
{
    public string skillId;
    public string skillName;
    public string description;
    public int maxLevel;
    public int currentLevel;
    public Sprite icon;
    public List<int> upgradeCosts;
    public UnityEvent<int> OnLevelUp;
}

public class SkillUpgrades : MonoBehaviour
{
    [Header("卡片設置")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UpgradeCard[] cardSlots; // 預先放置在場景中的卡片對象
    
    [Header("技能格設置")]
    [SerializeField] private Transform skillSlotContainer;
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private int maxSkillSlots = 10;

    [Header("UI元素")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Text availableUpgradesText; // 顯示可用升級次數的文本
    
    [Header("可配置卡片系統")]
    [Tooltip("可用的升級卡片類型")]
    [SerializeField] private UpgradeCardConfig[] cardConfigs;
    [Tooltip("每次升級時顯示的卡片數量")]
    [SerializeField] private int cardsPerUpgrade = 3;

    [Header("技能設置")]
    public List<Skill> skills = new List<Skill>();
    private HashSet<string> unlockedSkills = new HashSet<string>();

    [Header("事件")]
    public UnityEvent OnSkillsChanged;

    private bool isPanelOpen = false;
    private PlayerStats playerStats;
    
    // 可用升級次數
    private int availableUpgrades = 0;
    private bool isRefreshing = false; // 用於檢查是否正在刷新卡片

    private void Awake()
    {
        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        SyncSkillsFromCardConfigs(); // 自動同步
        
        // 初始化技能格
        InitializeSkillSlots();
        
        // 設置關閉按鈕
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUpgradePanel);
        }
        
        // 初始設置面板為關閉
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        // 訂閱玩家升級事件
        PlayerExperience expSystem = GameObject.FindWithTag("Player").GetComponent<PlayerExperience>();
        if (expSystem != null)
        {
            expSystem.OnLevelUp.AddListener(OnPlayerLevelUp);
        }
        
        // 初始化所有卡片的按鈕事件
        InitializeCardButtons();
    }
    
    // 初始化所有卡片的按鈕事件
    private void InitializeCardButtons()
    {
        if (cardSlots == null) return;
        
        for (int i = 0; i < cardSlots.Length; i++)
        {
            UpgradeCard card = cardSlots[i];
            if (card != null)
            {
                // 清除現有的監聽器
                card.OnCardSelected.RemoveAllListeners();
                // 設置新的監聽器
                int index = i; // 捕獲當前索引
                card.OnCardSelected.AddListener(() => SelectCard(index));
            }
        }
    }

    private void Update()
    {
        // 按Y鍵開關升級面板 - 只有在有可用升級次數時才打開
        if (Input.GetKeyDown(KeyCode.Y) && availableUpgrades > 0 && !isPanelOpen)
        {
            OpenUpgradePanel();
        }
        
        // 按TAB鍵開關物品/技能欄
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatsPanel();
        }
        
        // 更新統計面板中的玩家屬性
        if (statsPanel != null && statsPanel.activeSelf && playerStats != null)
        {
            UpdateStatsPanel();
        }
    }

    // 初始化技能格
    private void InitializeSkillSlots()
    {
        if (skillSlotContainer == null || skillSlotPrefab == null) return;
        
        // 清空現有的技能格
        foreach (Transform child in skillSlotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 創建新的技能格
        for (int i = 0; i < maxSkillSlots; i++)
        {
            // 修改為二步驟創建：先創建物體，再設置父級
            GameObject slot = Instantiate(skillSlotPrefab);
            slot.transform.SetParent(skillSlotContainer, false);
            slot.name = $"SkillSlot_{i}";
        }
    }

    // 打開升級面板
    public void OpenUpgradePanel()
    {
        // 如果沒有可用升級次數，則不打開面板
        if (availableUpgrades <= 0)
        {
            Debug.Log("沒有可用的升級次數！");
            return;
        }
        
        isPanelOpen = true; // 設置面板為開啟狀態
        isRefreshing = true; // 禁用刷新

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // 更新可用升級次數顯示
            UpdateAvailableUpgradesText();
            
            // 生成卡片內容
            GenerateCardContents();
            
            // 暫停遊戲時間
            Time.timeScale = 0f;
        }
    }
    
    // 關閉升級面板
    public void CloseUpgradePanel()
    {
        isPanelOpen = false; // 設置面板為關閉狀態
        isRefreshing = false; // 恢復刷新

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            
            // 恢復遊戲時間
            Time.timeScale = 1f;
        }
    }

    // 開關物品/技能欄
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            bool active = !statsPanel.activeSelf;
            statsPanel.SetActive(active);
            
            // 更新屬性顯示
            if (active)
            {
                UpdateStatsPanel();
            }
        }
    }

    // 更新屬性面板
    private void UpdateStatsPanel()
    {
        // 查找屬性文本並更新
        Text statsText = statsPanel.GetComponentInChildren<Text>();
        if (statsText != null && playerStats != null)
        {
            statsText.text = playerStats.GetStatsDescription();
        }
    }

    // 玩家升級時的回調
    public void OnPlayerLevelUp(int level)
    {
        // 等級1時不增加升級次數，其他等級增加1次
        if (level > 1)
        {
            availableUpgrades++;
            Debug.Log($"玩家升級到{level}級，獲得1次技能選擇機會，當前可用次數：{availableUpgrades}");
            
            // 更新可用升級次數顯示（如果面板打開）
            if (isPanelOpen)
            {
                UpdateAvailableUpgradesText();
            }
            
            // 可選：顯示升級提示
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowText("等級提升！按Y選擇技能", transform.position, Color.yellow);
            }
        }
    }
    
    // 更新可用升級次數顯示
    private void UpdateAvailableUpgradesText()
    {
        if (availableUpgradesText != null)
        {
            availableUpgradesText.text = $"可用升級次數: {availableUpgrades}";
        }
    }
    
    // 公共方法，用於HUD按鈕呼叫
    public void OpenUpgradePanel_HUDButton()
    {
        // 直接呼叫 OpenUpgradePanel 方法
        OpenUpgradePanel();
    }

    // 生成卡片內容
    private void GenerateCardContents()
    {
        if (cardSlots == null || cardSlots.Length == 0) return;
        
        // 檢查是否有可用的卡片配置
        if (cardConfigs == null || cardConfigs.Length == 0)
        {
            Debug.LogWarning("沒有設置卡片配置！");
            return;
        }
        
        // 創建索引列表並隨機打亂
        List<int> indices = new List<int>();
        for (int i = 0; i < cardConfigs.Length; i++)
        {
            indices.Add(i);
        }
        ShuffleList(indices);
        
        // 決定顯示多少張卡片
        int cardCount = Mathf.Min(cardsPerUpgrade, cardSlots.Length);
        
        // 更新每個卡片的內容
        for (int i = 0; i < cardCount; i++)
        {
            int configIndex = indices[i % indices.Count]; // 如果索引不夠，則重複使用
            
            if (i < cardSlots.Length)
            {
                UpgradeCard card = cardSlots[i];
                if (card != null)
                {
                    // 使用配置創建技能
                    UpgradeSkill skill = CreateSkillFromConfig(cardConfigs[configIndex]);
                    
                    // 初始化卡片
                    card.Initialize(skill);
                    
                    // 顯示卡片
                    card.gameObject.SetActive(true);
                }
            }
        }
        
        // 隱藏多餘的卡片
        for (int i = cardCount; i < cardSlots.Length; i++)
        {
            if (i < cardSlots.Length && cardSlots[i] != null)
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // 打亂列表
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    // 從配置創建技能
    private UpgradeSkill CreateSkillFromConfig(UpgradeCardConfig config)
    {
        float amount = UnityEngine.Random.Range(config.minValue, config.maxValue);
        
        return new UpgradeSkill
        {
            skillName = config.cardName,
            description = GetFormattedDescription(config.descriptionFormat, amount),
            statType = config.statType,
            amount = amount,
            icon = config.cardIcon
        };
    }
    
    // 格式化描述文本
    private string GetFormattedDescription(string format, float value)
    {
        // 如果是暴擊率，以百分比顯示
        if (format.Contains("{0:P}"))
        {
            return string.Format(format, value);
        }
        
        return string.Format(format, value.ToString("F1"));
    }

    // 選擇卡片
    private void SelectCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardSlots.Length) return;
        
        UpgradeCard card = cardSlots[cardIndex];
        if (card != null && card.Skill != null && playerStats != null && availableUpgrades > 0)
        {
            // 應用技能效果
            playerStats.AddUpgradeBonus(card.Skill.statType, card.Skill.amount);
            
            // 添加技能到已解鎖列表
            unlockedSkills.Add(card.Skill.skillName);
            
            // 更新技能格顯示
            UpdateSkillSlots();
            
            // 減少可用升級次數
            availableUpgrades--;
            
            Debug.Log($"選擇了技能：{card.Skill.skillName}，剩餘可用升級次數：{availableUpgrades}");
            
            // 更新可用升級次數顯示
            UpdateAvailableUpgradesText();
            
            // 如果還有可用升級次數，刷新卡片內容
            if (availableUpgrades > 0)
            {
                GenerateCardContents();
            }
            else
            {
                // 沒有可用升級次數，關閉面板
                CloseUpgradePanel();
            }
        }
    }

    // 更新技能格顯示
    private void UpdateSkillSlots()
    {
        if (skillSlotContainer == null) return;
        for (int i = 0; i < maxSkillSlots; i++)
        {
            Transform slotTransform = skillSlotContainer.GetChild(i);
            if (slotTransform == null) continue;
            Image skillIcon = slotTransform.GetComponentInChildren<Image>();
            if (skillIcon == null) continue;
            if (i < unlockedSkills.Count)
            {
                string skillId = unlockedSkills.ElementAt(i);
                var skill = skills.Find(s => s.skillId == skillId);
                if (skill != null)
                {
                    skillIcon.sprite = skill.icon;
                    skillIcon.color = Color.white;
                    var components = slotTransform.GetComponents<MonoBehaviour>();
                    foreach (var comp in components)
                    {
                        if (comp.GetType().Name == "TooltipTrigger")
                        {
                            comp.GetType().GetField("header").SetValue(comp, skill.skillName);
                            comp.GetType().GetField("content").SetValue(comp, skill.description);
                            break;
                        }
                    }
                }
            }
            else
            {
                skillIcon.sprite = null;
                skillIcon.color = new Color(0, 0, 0, 0.5f);
            }
        }
    }

    // 檢查是否有任何面板開啟
    public bool IsAnyPanelOpen()
    {
        // 檢查升級面板是否開啟
        if (isPanelOpen && upgradePanel != null && upgradePanel.activeSelf)
        {
            return true;
        }
        
        // 檢查統計/技能面板是否開啟
        if (statsPanel != null && statsPanel.activeSelf)
        {
            return true;
        }
        
        // 所有面板都未開啟
        return false;
    }

    private void Start()
    {
        // 初始化已解鎖技能列表
        foreach (var skill in skills)
        {
            if (skill.currentLevel > 0)
            {
                unlockedSkills.Add(skill.skillId);
            }
        }
    }

    // 獲取已解鎖的技能列表
    public List<string> GetUnlockedSkills()
    {
        return new List<string>(unlockedSkills);
    }

    // 載入已解鎖的技能
    public void LoadUnlockedSkills(List<string> skills)
    {
        unlockedSkills = new HashSet<string>(skills);
        OnSkillsChanged?.Invoke();
    }

    // 升級技能
    public bool UpgradeSkill(string skillId)
    {
        var skill = skills.Find(s => s.skillId == skillId);
        if (skill == null || skill.currentLevel >= skill.maxLevel)
            return false;

        skill.currentLevel++;
        unlockedSkills.Add(skillId);
        skill.OnLevelUp?.Invoke(skill.currentLevel);
        OnSkillsChanged?.Invoke();
        return true;
    }

    // 檢查技能是否已解鎖
    public bool IsSkillUnlocked(string skillId)
    {
        return unlockedSkills.Contains(skillId);
    }

    // 獲取技能等級
    public int GetSkillLevel(string skillId)
    {
        var skill = skills.Find(s => s.skillId == skillId);
        return skill?.currentLevel ?? 0;
    }

    // 獲取已解鎖的技能和其效果
    public Dictionary<string, float> GetSkillEffects()
    {
        var effects = new Dictionary<string, float>();
        foreach (var skillId in unlockedSkills)
        {
            var skill = skills.Find(s => s.skillId == skillId);
            if (skill != null)
            {
                // 根據技能等級計算效果值
                float effect = CalculateSkillEffect(skill);
                effects[skillId] = effect;
            }
        }
        return effects;
    }

    // 載入技能效果
    public void LoadSkillEffects(Dictionary<string, float> effects)
    {
        foreach (var effect in effects)
        {
            var skill = skills.Find(s => s.skillId == effect.Key);
            if (skill != null)
            {
                // 應用技能效果
                ApplySkillEffect(skill, effect.Value);
            }
        }
    }

    // 計算技能效果值
    private float CalculateSkillEffect(Skill skill)
    {
        // 這裡根據技能類型和等級計算具體效果
        // 例如：攻擊力提升、防禦力提升等
        return skill.currentLevel * 2f; // 示例：每級增加2點效果
    }

    // 應用技能效果
    private void ApplySkillEffect(Skill skill, float effect)
    {
        // 獲取 PlayerStats 組件
        var playerStats = GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            // 根據技能類型應用效果
            switch (skill.skillId)
            {
                case "attack_boost":
                    playerStats.AddUpgradeBonus(StatType.AttackPower, effect);
                    break;
                case "defense_boost":
                    playerStats.AddUpgradeBonus(StatType.Defense, effect);
                    break;
                // 添加其他技能效果...
            }
        }
    }

    // 修改 GetUnlockedSkills 方法，同時返回技能效果
    public (List<string> skills, Dictionary<string, float> effects) GetUnlockedSkillsWithEffects()
    {
        return (new List<string>(unlockedSkills), GetSkillEffects());
    }

    // 修改 LoadUnlockedSkills 方法，同時載入技能效果
    public void LoadUnlockedSkillsWithEffects(List<string> skills, Dictionary<string, float> effects)
    {
        unlockedSkills = new HashSet<string>(skills);
        LoadSkillEffects(effects);
        OnSkillsChanged?.Invoke();
    }

    // 自動從 cardConfigs 生成 skills 列表
    private void SyncSkillsFromCardConfigs()
    {
        skills.Clear();
        if (cardConfigs == null) return;
        foreach (var config in cardConfigs)
        {
            skills.Add(new Skill {
                skillId = config.cardName, // 用 cardName 當作唯一ID
                skillName = config.cardName,
                description = config.descriptionFormat,
                icon = config.cardIcon,
                // maxLevel 欄位省略，因 cardConfigs 沒有
            });
        }
    }
}

// 這些類現在已經移動到獨立的腳本文件中
// UpgradeSkill.cs 包含 UpgradeSkill 和 UpgradeCardConfig 類
// UpgradeCard.cs 包含 UpgradeCard 類
