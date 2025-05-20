using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// using TMPro; // 如果你使用 TextMeshPro

public class ItemTipPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Text itemNameText; // 物品名稱的 Text
    public Image itemImage;   // 物品圖片的 Image
    public Text itemDescriptionText; // 物品描述的 Text
    public GameObject nextTip; // 顯示「按空白鍵繼續」提示，可選

    // 顯示物品資訊並等待玩家按鍵後自動隱藏
    public IEnumerator ShowItemInfoCoroutine(string name, Sprite image, string description)
    {
        if (itemNameText != null) itemNameText.text = name;
        if (itemImage != null) itemImage.sprite = image;
        if (itemDescriptionText != null) itemDescriptionText.text = description.Replace("\\n", "\n");
        gameObject.SetActive(true);
        if (nextTip != null) nextTip.SetActive(true);

        // 先等玩家放開按鍵
        yield return new WaitWhile(() => Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0));
        // 再等玩家新一次按下
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        if (nextTip != null) nextTip.SetActive(false);
        gameObject.SetActive(false);
    }

    // 傳統顯示（不等待玩家輸入）
    public void ShowItemInfo(string name, Sprite image, string description)
    {
        if (itemNameText != null) itemNameText.text = name;
        if (itemImage != null) itemImage.sprite = image;
        if (itemDescriptionText != null) itemDescriptionText.text = description.Replace("\\n", "\n");
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