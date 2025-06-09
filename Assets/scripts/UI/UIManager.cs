using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set;}

    private GameManager gameManager;
    private EnemySpawner enemySpawner;

    [Header("UI組件")]
    public GameObject gameOverMenu; 
    public GameObject gamePauseMenu;
    public GameObject roomCompleteMenu;


    public Slider healthSlider;//血量
    public Slider dodgeCdSlider;//閃避冷卻條
    public Slider expSlider; // 經驗值條
    public Text levelText; // 等級文本

    
    
    [Header("TIME")]
    [SerializeField] private float gameTime = 0f; // 改為正計時
    public Text timerText;
    public Text scoreText;
    public Text expText; // 經驗值文本
 
    

    private int score;
    private int playerExp; // 玩家經驗值

    // 儲存當前運行的經驗值協程
    private Coroutine expUpdateCoroutine = null;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        score = 0;
        playerExp = 0;
        UpdateScoreText();
        UpdateExpText();
        gameOverMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
    }

    void Update()
    {
        Timer();
        if(enemySpawner._allWavesCompleted==true)
        {
            ShowRoomCompleteMenu();
        }
    }

    //血量條UI
    public void UpdateHelthSlider(float maxHealth, float currentHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
    //閃避CD條UI
    public void DodgeCdSlider(float CdTime)
    {
        StartCoroutine(UpdateCdCoroutine(CdTime));
    }
    //CD條過度效果
    public IEnumerator UpdateCdCoroutine(float CdTime)
    {
        dodgeCdSlider.maxValue = CdTime;
        dodgeCdSlider.value = CdTime;

        float elapsedTime = 0f;

        while(elapsedTime<CdTime)
        {
            dodgeCdSlider.value = elapsedTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // 經驗值條UI更新方法
    public void UpdateExpSlider(float currentExp, float maxExp)
    {
        // 如果有正在運行的協程，先停止它
        if (expUpdateCoroutine != null)
        {
            StopCoroutine(expUpdateCoroutine);
        }
        
        // 啟動新的協程並保存引用
        expUpdateCoroutine = StartCoroutine(UpdateExpCoroutine(currentExp, maxExp));
    }

    // 經驗值條平滑過渡效果
    public IEnumerator UpdateExpCoroutine(float currentExp, float maxExp)
    {
        if (expSlider == null) yield break;

        // 設置最大值 - 這很重要，確保slider的最大值已更新
        expSlider.maxValue = maxExp;
        
        // 平滑過渡動畫
        float startValue = expSlider.value;
        float targetValue = currentExp;
        float duration = 0.5f; // 過渡時間
        float elapsedTime = 0f;
        
        // 如果目標值小於起始值（例如升級時經驗值歸零），先將slider歸零
        if (targetValue < startValue)
        {
            // 快速過渡到0
            duration = 0.2f;
            while (elapsedTime < duration)
            {
                expSlider.value = Mathf.Lerp(startValue, 0, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 重置時間和起始值，準備向目標值過渡
            elapsedTime = 0f;
            startValue = 0f;
            duration = 0.3f; // 較短的過渡時間
        }

        // 標準過渡動畫
        while (elapsedTime < duration)
        {
            expSlider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 確保最終值正確
        expSlider.value = targetValue;
        expUpdateCoroutine = null; // 清除協程引用
    }
    
    // 更新等級文本
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Lv." + level.ToString();
        }
    }



    private void Timer()
    {
        // 正常計時
        gameTime += Time.deltaTime;
        
        // 格式化為分:秒
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    // 增加經驗值的方法
    public void IncreaseExp(int amount)
    {
        playerExp += amount;
        UpdateExpText();
        
        // 如果有玩家經驗系統組件，則傳遞經驗值
        PlayerExperience playerExpSystem = FindAnyObjectByType<PlayerExperience>();
        if (playerExpSystem != null)
        {
            playerExpSystem.GainExperience(amount);
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
    
    // 更新經驗值文本
    void UpdateExpText()
    {
        if (expText != null)
        {
            expText.text = "EXP: " + playerExp.ToString();
        }
    }

    public void ShowGameOverMenu()  
    {
        gameOverMenu.SetActive(true);  
    }

    public void ShowGamePauseMenu()
    {
        gamePauseMenu.SetActive(true);
    }
    
    public void ShowRoomCompleteMenu()
    {
        roomCompleteMenu.SetActive(true);
        
        // 啟動協程等待半秒後關閉
        StartCoroutine(HideRoomCompleteMenuAfterDelay(1.5f));
    }
    
    // 延遲關閉房間完成 UI 的協程
    private IEnumerator HideRoomCompleteMenuAfterDelay(float delay)
    {
        // 使用 WaitForSecondsRealtime 確保即使遊戲暫停也會計時
        yield return new WaitForSecondsRealtime(delay);
        roomCompleteMenu.SetActive(false);
    }
}
