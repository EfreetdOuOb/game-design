using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{

    [SerializeField]
    private Image bar;

    public void UpdateBar(float hp , float maxhp)
    {
        bar.fillAmount = hp / (float)maxhp;
    }  
}
