using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 工具提示觸發器 - 添加到需要工具提示的UI元素上
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    public string content;
    
    private static Coroutine delayedCall;
    private static float tooltipDelay = 0.5f;
    
    private static MonoBehaviour coroutineRunner;
    
    private void Awake()
    {
        // 確保有物件可以執行協程
        if (coroutineRunner == null)
        {
            coroutineRunner = this;
        }
    }
    
    // 實現IPointerEnterHandler接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 取消任何正在進行的調用
        if (delayedCall != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(delayedCall);
            delayedCall = null;
        }
        
        // 延遲顯示工具提示
        if (coroutineRunner != null)
        {
            delayedCall = coroutineRunner.StartCoroutine(DelayedShow());
        }
    }
    
    private IEnumerator DelayedShow()
    {
        yield return new WaitForSeconds(tooltipDelay);
        TooltipSystem.Show(content, header);
    }
    
    // 實現IPointerExitHandler接口
    public void OnPointerExit(PointerEventData eventData)
    {
        // 取消延遲顯示
        if (delayedCall != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(delayedCall);
            delayedCall = null;
        }
        
        TooltipSystem.Hide();
    }
} 