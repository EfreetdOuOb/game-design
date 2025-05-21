using UnityEngine;

// 工具提示系統 - 管理工具提示顯示
public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    
    [SerializeField] private Tooltip tooltip;
    
    private void Awake()
    {
        current = this;
        
        // 檢查tooltip組件是否存在
        if (tooltip == null)
        {
            Debug.LogError("TooltipSystem: 未設置tooltip引用！請在Inspector中分配一個Tooltip組件。");
            
            // 嘗試在子物件中查找
            tooltip = GetComponentInChildren<Tooltip>(true);
            if (tooltip != null)
            {
                Debug.Log("TooltipSystem: 已自動找到Tooltip組件。");
            }
            else
            {
                Debug.LogError("TooltipSystem: 無法找到Tooltip組件，工具提示將無法顯示！");
                
                // 創建一個新的Tooltip對象
                GameObject tooltipObj = new GameObject("Tooltip");
                tooltipObj.transform.SetParent(transform);
                tooltip = tooltipObj.AddComponent<Tooltip>();
                Debug.Log("TooltipSystem: 已創建新的Tooltip組件。");
            }
        }
    }
    
    public static void Show(string content, string header = "")
    {
        if (current != null && current.tooltip != null)
        {
            current.tooltip.SetText(content, header);
            current.tooltip.gameObject.SetActive(true);
            Debug.Log("顯示工具提示: " + header + " - " + content);
        }
        else
        {
            Debug.LogWarning("TooltipSystem: 無法顯示工具提示，系統未初始化或tooltip為null");
        }
    }
    
    public static void Hide()
    {
        if (current != null && current.tooltip != null)
        {
            current.tooltip.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TooltipSystem: 無法隱藏工具提示，系統未初始化或tooltip為null");
        }
    }
    
    // 檢查工具提示系統是否已正確初始化
    public static bool IsInitialized()
    {
        return (current != null && current.tooltip != null);
    }
}

// Tooltip 類已移至 Tooltip.cs 文件 