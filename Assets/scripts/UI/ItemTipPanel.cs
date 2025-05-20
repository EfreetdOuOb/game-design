using UnityEngine;
using UnityEngine.UI;
// using TMPro; // 如果你使用 TextMeshPro

public class ItemTipPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Text itemNameText; // 物品名稱的 TextMeshProUGUI
    public Image itemImage;         // 物品圖片的 Image
    public Text itemDescriptionText; // 物品文本的 TextMeshProUGUI

    // 顯示物品資訊的方法
    public void ShowItemInfo(string name, Sprite image, string description)
    {
        if (itemNameText != null) itemNameText.text = name;
        if (itemImage != null) itemImage.sprite = image;
        if (itemDescriptionText != null) itemDescriptionText.text = description;

        // 啟用這個 Panel
        gameObject.SetActive(true);
    }

    // 隱藏物品提示面板
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // 可以添加其他方法，例如等待玩家輸入後關閉面板等
    // 這裡只提供顯示和隱藏的基本功能
} 