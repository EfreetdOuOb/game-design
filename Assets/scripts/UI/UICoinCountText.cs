using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 金幣數量Text
/// </summary>

public class UICoinCountText : MonoBehaviour
{
    private static Text coinCountText;

    private void Awake()
    {
        coinCountText = GetComponent<Text>();
    }

    public static void UpdateText(int amount)
    {
        coinCountText.text = amount.ToString();
    }
}
