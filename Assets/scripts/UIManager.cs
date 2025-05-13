using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private EnemySpawner enemySpawner;

    [Header("UI組件")]
    public GameObject gameOverMenu; 
    public GameObject gamePauseMenu;
    public GameObject roomCompleteMenu;


    public Slider healthSlider;//血量
    public Slider dodgeCdSlider;//閃避冷卻條

    
    
    [Header("TIME")]
    [SerializeField] private float gameTime = 0f; // 改為正計時
    public Text timerText;
    public Text scoreText;
 
    

    private int score;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        score = 0;
        UpdateScoreText();
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

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
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
        StartCoroutine(HideRoomCompleteMenuAfterDelay(0.5f));
    }
    
    // 延遲關閉房間完成 UI 的協程
    private IEnumerator HideRoomCompleteMenuAfterDelay(float delay)
    {
        // 使用 WaitForSecondsRealtime 確保即使遊戲暫停也會計時
        yield return new WaitForSecondsRealtime(delay);
        roomCompleteMenu.SetActive(false);
    }
}
