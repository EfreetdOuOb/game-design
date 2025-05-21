using UnityEngine;
using UnityEngine.UI;

// 工具提示UI元素
public class Tooltip : MonoBehaviour
{
    [SerializeField] private Text headerText;
    [SerializeField] private Text contentText;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit = 40;
    [SerializeField] private RectTransform rectTransform;
    
    [Header("外觀設置")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color headerColor = Color.white;
    [SerializeField] private Color contentColor = new Color(0.9f, 0.9f, 0.9f);
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // 確保所有必要組件存在
        EnsureComponents();
    }
    
    // 確保所有必要組件都存在
    private void EnsureComponents()
    {
        // 確保有RectTransform
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        // 確保有CanvasGroup用於淡入淡出
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // 如果沒有指定背景圖像，創建一個
        Image background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
            background.color = backgroundColor;
        }
        
        // 如果沒有佈局元素，創建一個
        if (layoutElement == null)
        {
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 200;
                layoutElement.preferredHeight = 80;
                layoutElement.flexibleWidth = 1;
                layoutElement.flexibleHeight = 0;
            }
        }
        
        // 如果沒有標題文本，創建一個
        if (headerText == null)
        {
            GameObject headerObj = new GameObject("HeaderText");
            headerObj.transform.SetParent(transform);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, -5);
            headerRect.sizeDelta = new Vector2(-10, 30);
            
            headerText = headerObj.AddComponent<Text>();
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.fontSize = 16;
            headerText.fontStyle = FontStyle.Bold;
            headerText.alignment = TextAnchor.UpperCenter;
            headerText.color = headerColor;
        }
        
        // 如果沒有內容文本，創建一個
        if (contentText == null)
        {
            GameObject contentObj = new GameObject("ContentText");
            contentObj.transform.SetParent(transform);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = new Vector2(0, -15);
            contentRect.sizeDelta = new Vector2(-20, -35);
            
            contentText = contentObj.AddComponent<Text>();
            contentText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            contentText.fontSize = 14;
            contentText.alignment = TextAnchor.UpperLeft;
            contentText.color = contentColor;
        }
    }
    
    public void SetText(string content, string header = "")
    {
        // 確保組件已初始化
        if (headerText == null || contentText == null)
            EnsureComponents();
            
        // 設置標頭文本
        if (string.IsNullOrEmpty(header))
        {
            headerText.gameObject.SetActive(false);
        }
        else
        {
            headerText.gameObject.SetActive(true);
            headerText.text = header;
        }
        
        // 設置內容文本
        contentText.text = content;
        
        // 根據文本長度調整佈局
        int headerLength = header.Length;
        int contentLength = content.Length;
        
        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);
    }
    
    private void Update()
    {
        // 跟隨鼠標移動
        if (rectTransform != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            
            // 調整位置，使其不超出屏幕邊界
            float pivotX = mousePosition.x / Screen.width;
            float pivotY = mousePosition.y / Screen.height;
            
            // 避免懸停提示超出屏幕
            pivotX = (mousePosition.x + 200 > Screen.width) ? 1 : 0;
            pivotY = (mousePosition.y - 100 < 0) ? 0 : 1;
            
            rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = mousePosition;
        }
    }
} 