using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 金幣數量Text
/// </summary>

public class UICoinCountText : MonoBehaviour
{
    private static Text coinCountText;
    private static int coinCount; // 新增金幣數量變數

    private void Awake()
    {
        coinCountText = GetComponent<Text>();
    }

    public static void UpdateText(int amount)
    {
        coinCount = amount; // 更新金幣數量
        coinCountText.text = coinCount.ToString();
    }

    // 新增獲取金幣數量的方法
    public static int GetCoinCount()
    {
        return coinCount;
    }
}
