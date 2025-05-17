using UnityEngine;
using TMPro;
using DG.Tweening;
/// <summary>
/// UI浮動文字顯示效果文本
/// </summary>
public class UIShowText : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        //DOTween動畫效果
        transform.DOMoveY(transform.position.y + 20, 0.5f);
        transform.DOScale(transform.localScale*2, 0.2f);
        Destroy(gameObject,0.6f);//0.6秒後銷毀
    }

    //設定文本內容
    public void SetText(string str, Color color)
    {
        text.text = str;
        text.color = color;
    }
}
