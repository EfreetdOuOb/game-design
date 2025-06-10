using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ScreenFader : MonoBehaviour
{
     //單例
     public static ScreenFader Instance{get; private set;}
     public CanvasGroup faderCanvasGroup;
     public float fadeDuration = 1f;
     private void Awake()
     {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
     }

     //淡入
     public IEnumerator FadeSceneIn()
     {
        yield return StartCoroutine(Fade(0f, faderCanvasGroup));
        faderCanvasGroup.gameObject.SetActive(false);
     }
      
     
     //淡出
     public IEnumerator FadeSceneOut()
     {
        faderCanvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(1f, faderCanvasGroup));
     }
      
     //淡入淡出
     public IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
     {
        yield return canvasGroup.DOFade(finalAlpha, fadeDuration).WaitForCompletion();
     }

     
}
