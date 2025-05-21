using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldUI : MonoBehaviour
{
    [Header("護盾計數UI")]
    [SerializeField] private GameObject[] shieldIndicators; // 護盾指示器陣列
    [SerializeField] private Color activeShieldColor = Color.cyan; // 激活的護盾顏色
    [SerializeField] private Color inactiveShieldColor = Color.gray; // 未激活的護盾顏色
    
    [Header("充能進度UI")]
    [SerializeField] private Slider chargeProgressSlider; // 充能進度條
    [SerializeField] private Text chargeProgressText; // 充能進度文本
    
    private EnergyShield playerShield; // 能量護盾引用
    
    private void Start()
    {
        // 查找玩家的護盾組件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerShield = player.GetComponent<EnergyShield>();
            
            if (playerShield != null)
            {
                // 訂閱事件
                playerShield.OnShieldCountChanged.AddListener(UpdateShieldIndicators);
                playerShield.OnKillCountChanged.AddListener(UpdateChargeProgress);
                playerShield.OnShieldActivated.AddListener(PlayActivationAnimation);
            }
            else
            {
                Debug.LogWarning("無法找到能量護盾組件！");
            }
        }
        else
        {
            Debug.LogWarning("無法找到玩家物件！");
        }
    }
    
    // 更新護盾指示器顯示
    private void UpdateShieldIndicators(int currentCount, int maxCount)
    {
        // 確保有足夠的指示器
        if (shieldIndicators == null || shieldIndicators.Length < maxCount)
        {
            Debug.LogWarning("護盾指示器數量不足!");
            return;
        }
        
        // 根據當前護盾數量更新指示器顯示
        for (int i = 0; i < shieldIndicators.Length; i++)
        {
            if (i < currentCount)
            {
                // 有護盾
                SetIndicatorActive(shieldIndicators[i], true);
            }
            else if (i < maxCount)
            {
                // 無護盾但在最大數量內
                SetIndicatorActive(shieldIndicators[i], false);
            }
            else
            {
                // 超出最大數量，隱藏
                shieldIndicators[i].SetActive(false);
            }
        }
    }
    
    // 設置指示器狀態
    private void SetIndicatorActive(GameObject indicator, bool isActive)
    {
        if (indicator == null) return;
        
        // 顯示指示器
        indicator.SetActive(true);
        
        // 更新顏色
        Image indicatorImage = indicator.GetComponent<Image>();
        if (indicatorImage != null)
        {
            indicatorImage.color = isActive ? activeShieldColor : inactiveShieldColor;
        }
    }
    
    // 更新充能進度顯示
    private void UpdateChargeProgress(int currentKills, int requiredKills)
    {
        if (chargeProgressSlider != null)
        {
            chargeProgressSlider.maxValue = requiredKills;
            chargeProgressSlider.value = currentKills;
        }
        
        if (chargeProgressText != null)
        {
            chargeProgressText.text = $"{currentKills}/{requiredKills}";
        }
    }
    
    // 播放護盾激活動畫
    private void PlayActivationAnimation()
    {
        // 可以在這裡添加UI動畫效果
        StartCoroutine(FlashShieldUI());
    }
    
    // 護盾激活時閃爍UI
    private IEnumerator FlashShieldUI()
    {
        // 閃爍次數和時間
        int flashCount = 3;
        float flashDuration = 0.1f;
        
        // 獲取所有UI元素
        List<Image> uiElements = new List<Image>();
        
        // 添加護盾指示器
        foreach (GameObject indicator in shieldIndicators)
        {
            if (indicator != null && indicator.activeSelf)
            {
                Image img = indicator.GetComponent<Image>();
                if (img != null)
                {
                    uiElements.Add(img);
                }
            }
        }
        
        // 添加充能進度條
        if (chargeProgressSlider != null)
        {
            Image sliderFill = chargeProgressSlider.fillRect.GetComponent<Image>();
            if (sliderFill != null)
            {
                uiElements.Add(sliderFill);
            }
        }
        
        // 保存原始顏色
        Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
        foreach (Image img in uiElements)
        {
            originalColors[img] = img.color;
        }
        
        // 執行閃爍
        for (int i = 0; i < flashCount; i++)
        {
            // 閃亮
            foreach (Image img in uiElements)
            {
                img.color = Color.white;
            }
            
            yield return new WaitForSeconds(flashDuration);
            
            // 恢復
            foreach (Image img in uiElements)
            {
                img.color = originalColors[img];
            }
            
            yield return new WaitForSeconds(flashDuration);
        }
    }
} 