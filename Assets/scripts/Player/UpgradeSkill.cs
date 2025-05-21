using UnityEngine;

// 升級技能數據類
[System.Serializable]
public class UpgradeSkill
{
    public string skillName;
    public string description;
    public StatType statType;
    public float amount;
    public Sprite icon;
}

// 卡片配置類 - 用於在編輯器中設置不同類型的卡片
[System.Serializable]
public class UpgradeCardConfig
{
    [Tooltip("卡片名稱")]
    public string cardName;
    
    [Tooltip("卡片圖標")]
    public Sprite cardIcon;
    
    [Tooltip("影響的屬性類型")]
    public StatType statType;
    
    [Tooltip("描述文本格式 (使用 {0} 作為數值佔位符)")]
    public string descriptionFormat = "增加{0}點";
    
    [Tooltip("最小數值")]
    public float minValue = 1f;
    
    [Tooltip("最大數值")]
    public float maxValue = 5f;
} 