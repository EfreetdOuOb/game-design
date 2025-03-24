using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    
    [Header("TIME")]
    [SerializeField] private float timer ;
    public Text timerText;
    public Text scoreText;
    public GameObject gameOverMenu; 
    public GameObject gamePauseMenu;

    public bool isTimeOut = false;
    private int score;

    void Start()
    {
        gameManager= FindObjectOfType<GameManager>();
        score = 0;
        UpdateScoreText();
        gameOverMenu.SetActive(false);
        gamePauseMenu.SetActive(false);
    }

    void Update()
    {
        Timer();

    }

    private void Timer()
    {
        if (isTimeOut == false)
        { 
            timer -= Time.deltaTime; 
            timerText.text = timer.ToString("F0"); 
            if (timer <= 0)
            {
                isTimeOut = true;
                timerText.text = "00:00";
                  
            }
        }
    }

     

    public void IncreaseScore(int amount)
    {
        if (!isTimeOut)
        {
            score += amount;
            UpdateScoreText();
        }
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

    
    
    

    
    
}
