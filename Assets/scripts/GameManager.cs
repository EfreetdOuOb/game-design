using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;    
    public EnemySpawner enemySpawner;
     
    public bool isPaused = false;

    void Start()
    {
        uiManager= FindObjectOfType<UIManager>();
        Time.timeScale = 1;    
    }

    void Update()
    {
        if (uiManager.isTimeOut ==true)
        {
            EndGame();
        }
          
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            if (isPaused == false)
            { 
                PauseGame();
                
            }
            else 
            { 
                ResumeGame();
            }

        }

    } 
    public void EndGame()
    { 
        uiManager.ShowGameOverMenu(); 
        Time.timeScale = 0; 
    }

    public void PauseGame()
    { 
        Time.timeScale = 0;
        uiManager.ShowGamePauseMenu();
        isPaused = true;
    }

    public void PlayerScored(int score)
    {
        uiManager.IncreaseScore(score);
    }
    public void RestartGame()
    {
        // 載入當前場景以重新開始 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ResumeGame()
    { 
        Time.timeScale = 1;
        uiManager.gameOverMenu.SetActive(false);
        uiManager.gamePauseMenu.SetActive(false);
        isPaused = false;
        
    }

    public void BackToMenu()//按鈕
    {
        SceneManager.LoadScene("MainMenu");
    }

}

