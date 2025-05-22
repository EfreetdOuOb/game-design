using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnergyShield : MonoBehaviour
{
     

    [Header("護盾設置")]
    [SerializeField] public int maxShieldCount = 3; // 最大護盾數量
    [SerializeField] public int currentShieldCount; // 當前護盾數量
    [SerializeField] public int killsRequiredForCharge = 5; // 充能所需擊殺數
    
    [Header("視覺效果")]
    [SerializeField] private GameObject shieldEffectPrefab; // 護盾視覺效果預製體
    [SerializeField] private Color shieldColor = new Color(0, 0.8f, 1f, 0.5f); // 護盾顏色
    
    [Header("音效設置")]
    [SerializeField] private string shieldActivateSound = "shield_activate";
    [SerializeField] private string shieldRechargeSound = "shield_recharge";
    
    [Header("事件")]
    public UnityEvent<int, int> OnShieldCountChanged; // 護盾數量變化事件(當前, 最大)
    public UnityEvent<int, int> OnKillCountChanged; // 擊殺計數變化事件(當前, 需要)
    public UnityEvent OnShieldActivated; // 護盾激活事件
    
    public int currentKillCount; // 當前擊殺計數
    private List<GameObject> shieldEffects = new List<GameObject>(); // 護盾特效對象列表
    private Health playerHealth; // 玩家生命值引用
    
    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        Debug.Log("EnergyShield Awake: 已獲取playerHealth組件");
         
    }
    
    public void OnEnable()
    {
        Debug.Log("【能量護盾】OnEnable開始執行");
        
        // 檢查此組件是否應該啟用
        Equipment equipment = GetComponent<Equipment>();
        if (equipment != null)
        {
            // 如果裝備系統沒有裝備護盾，則禁用自己
            EquipmentItem currentEquip = equipment.GetCurrentEquipment();
            Debug.Log($"【能量護盾】獲取當前裝備：{(currentEquip != null ? currentEquip.itemName : "無")}，是否為護盾：{(currentEquip != null && currentEquip.isEnergyShield)}");
            
            if (currentEquip == null || !currentEquip.isEnergyShield)
            {
                Debug.LogWarning("【能量護盾】OnEnable: 玩家未裝備護盾，禁用此組件");
                this.enabled = false;
                return; // 提前返回，不執行剩餘的初始化
            }
            else
            {
                Debug.Log("【能量護盾】OnEnable: 玩家已裝備護盾，繼續初始化");
            }
        }
        else
        {
            Debug.LogWarning("【能量護盾】OnEnable: 找不到Equipment組件");
        }
        
        // 初始化護盾
        currentShieldCount = maxShieldCount;
        currentKillCount = 0;
        
        // 創建初始護盾特效
        UpdateShieldVisuals();
        
        // 通知UI
        NotifyUIChanges();
        
        // 輸出調試信息
        Debug.Log("【能量護盾】OnEnable: 能量護盾已啟用，護盾值：" + currentShieldCount);
        
        // 訂閱GameManager的敵人擊殺事件
        if (GameManager.Instance != null && GameManager.Instance.OnEnemyKilled != null)
        {
            GameManager.Instance.OnEnemyKilled.AddListener(OnEnemyKilled);
            Debug.Log("【能量護盾】OnEnable: 已訂閱OnEnemyKilled事件");
        }
        else
        {
            Debug.LogError("【能量護盾】OnEnable: GameManager.Instance或OnEnemyKilled事件為空，無法訂閱");
        }
    }
    
    private void Start()
    {
        Debug.Log("EnergyShield Start: 組件已啟動");
        
        // 再次檢查訂閱是否成功，並在需要時重新訂閱
        if (GameManager.Instance != null)
        {
            // 先移除之前可能的監聽，避免重複添加
            GameManager.Instance.OnEnemyKilled.RemoveListener(OnEnemyKilled);
            // 再添加監聽
            GameManager.Instance.OnEnemyKilled.AddListener(OnEnemyKilled);
            Debug.Log("EnergyShield Start: 已確保訂閱擊殺事件");
        }
        else
        {
            Debug.LogWarning("EnergyShield Start: 無法找到GameManager，護盾充能可能無法正常工作");
        }
    }
    
    private void Update()
    {
        // 可以添加一些視覺效果的更新
        // 例如護盾旋轉、脈衝等
    }
    
    public void OnDisable()
    {
        // 取消訂閱GameManager的敵人擊殺事件
        if (GameManager.Instance != null && GameManager.Instance.OnEnemyKilled != null)
        {
            GameManager.Instance.OnEnemyKilled.RemoveListener(OnEnemyKilled);
            Debug.Log("EnergyShield OnDisable: 已取消訂閱OnEnemyKilled事件");
        }
        
        // 清除護盾特效
        foreach (GameObject effect in shieldEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        shieldEffects.Clear();
        
        Debug.Log("EnergyShield OnDisable: 組件已禁用，護盾特效已清除");
    }
    
    // 當敵人被擊殺時調用
    public void OnEnemyKilled()
    {
        currentKillCount++;
        Debug.Log("擊殺敵人，計數：" + currentKillCount);
        
        // 每擊殺5個敵人充能一格護盾
        if (currentKillCount >= killsRequiredForCharge)
        {
            if (currentShieldCount < maxShieldCount)
            {
                // 更新護盾視覺效果
                currentShieldCount++;
                UpdateShieldVisuals();
                NotifyUIChanges();
                Debug.Log("護盾充能完成，當前護盾：" + currentShieldCount);
                
                // 顯示充能完成提示
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowText("護盾充能完成！", transform.position, Color.cyan);
                }
            }
            else
            {
                currentKillCount = 0; // 重置擊殺計數
                Debug.Log("護盾已滿，無需充能");
            }
            currentKillCount = 0;
        }
    }
    
    // 觸發護盾效果，只對遠程攻擊有效
    public bool TriggerShield(bool isRangedAttack = false)
    {
        // 如果不是遠程攻擊，護盾不生效
        if (!isRangedAttack)
        {
            Debug.Log("護盾只能阻擋遠程攻擊");
            return false;
        }
        
        // 檢查是否有護盾可用
        if (currentShieldCount <= 0)
        {
            Debug.Log("沒有護盾可用");
            return false;
        }
        
        // 觸發護盾效果
        currentShieldCount--;
        
        // 移除投射物的效果（如果有）
        ClearProjectiles();
        
        // 更新護盾視覺效果
        UpdateShieldVisuals();
        
        // 通知UI
        NotifyUIChanges();
        
        Debug.Log("護盾觸發成功，剩餘護盾：" + currentShieldCount);
        
        // 播放護盾激活特效
        PlayShieldActivationEffect();
        
        return true;
    }
    
    // 更新護盾視覺效果
    public void UpdateShieldVisuals()
    {
        // 首先清除現有特效
        foreach (GameObject effect in shieldEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        shieldEffects.Clear();
        
        // 為每個護盾創建視覺效果
        for (int i = 0; i < currentShieldCount; i++)
        {
            if (shieldEffectPrefab != null)
            {
                // 計算每個護盾的大小和位置偏移
                // 將護盾尺寸設置為僅比玩家稍大一些
                float baseScale = 0.1f; // 更小的基礎尺寸 (玩家大小的1.1倍)
                float increment = 0.03f; // 更小的增量
                float scale = baseScale + (i * increment);
                
                GameObject effect = Instantiate(shieldEffectPrefab, transform.position, Quaternion.identity, transform);
                effect.transform.localScale = new Vector3(scale, scale, 1);
                
                // 設置顏色
                SpriteRenderer renderer = effect.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    // 每層護盾稍微改變透明度
                    Color layerColor = shieldColor;
                    layerColor.a = shieldColor.a * (1.0f - (i * 0.2f));
                    renderer.color = layerColor;
                }
                
                shieldEffects.Add(effect);
            }
        }
        
        // 通知 UI 更新護盾數量
        OnShieldCountChanged?.Invoke(currentShieldCount, maxShieldCount);
    }
    
    // 播放護盾激活特效
    private void PlayShieldActivationEffect()
    {
        // 在這裡可以播放護盾激活時的特效
        // 例如閃光、波紋等
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(shieldActivateSound);
        }
        
        // 創建臨時視覺效果
        if (shieldEffectPrefab != null)
        {
            GameObject tempEffect = Instantiate(shieldEffectPrefab, transform.position, Quaternion.identity);
            // 設置更小的初始大小
            tempEffect.transform.localScale = new Vector3(0.2f, 0.2f, 1);
            
            SpriteRenderer renderer = tempEffect.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = new Color(1f, 1f, 1f, 0.7f); // 明亮的白色
                
                // 使用動畫或縮放效果
                StartCoroutine(PulseEffect(tempEffect));
            }
        }
    }
    
    // 脈衝效果協程
    private IEnumerator PulseEffect(GameObject effectObject)
    {
        float duration = 0.5f;
        float elapsed = 0;
        
        // 調整脈衝尺寸
        float startScale = 0.2f;  // 更小的起始尺寸
        float endScale = 0.4f;    // 更小的最大尺寸
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            float scale = Mathf.Lerp(startScale, endScale, elapsed / duration);
            effectObject.transform.localScale = new Vector3(scale, scale, 1);
            
            // 同時淡出
            SpriteRenderer renderer = effectObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = Mathf.Lerp(0.7f, 0, elapsed / duration);
                renderer.color = color;
            }
            
            yield return null;
        }
        
        // 效果結束後銷毀物體
        Destroy(effectObject);
    }
    
    // 播放充能完成特效
    private void PlayRechargeEffect()
    {
        // 播放充能完成的特效和音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(shieldRechargeSound);
        }
        
        // 可以添加粒子效果或其他視覺提示
    }
    
    // 清除周圍的投射物
    public void ClearProjectiles()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 5f); // 5米範圍內
        
        foreach (Collider2D collider in colliders)
        {
            // 檢查是否是投射物 - 只處理敵人的遠程攻擊
            // 同時支持Tag和Layer兩種檢測方式
            if (collider.CompareTag("EnemyProjectile") || collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
            {
                // 播放消除特效
                PlayProjectileDestroyEffect(collider.transform.position);
                
                // 銷毀投射物
                Destroy(collider.gameObject);
                Debug.Log("能量護盾摧毀了一個敵方投射物");
            }
        }
    }
    
    // 播放投射物消除特效
    private void PlayProjectileDestroyEffect(Vector3 position)
    {
        // 在投射物位置創建消除特效
        // 例如閃光、粒子等
    }
    
    // 通知UI變化
    private void NotifyUIChanges()
    {
        // 更新護盾計數UI
        OnShieldCountChanged?.Invoke(currentShieldCount, maxShieldCount);
        
        // 更新擊殺計數UI
        OnKillCountChanged?.Invoke(currentKillCount, killsRequiredForCharge);
    }
} 