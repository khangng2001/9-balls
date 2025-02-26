using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BallPool;

public class CircularTimeController : TimeController
{
    [SerializeField] private Image slider1;
    [SerializeField] private Image slider2;

    protected override void OnUpdateTime(float time01)
    {
        if (BallPoolPlayer.mainPlayer.myTurn)
        {
            slider1.color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(time01 + 0.1f));
            slider1.color = new Color(slider1.color.r, slider1.color.g, slider1.color.b, Mathf.Clamp01(time01 + 0.1f));
            slider1.fillAmount = 1.0f - time01;
        }
        else
        {
            slider2.color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(time01 + 0.1f));
            slider2.color = new Color(slider1.color.r, slider1.color.g, slider1.color.b, Mathf.Clamp01(time01 + 0.1f));
            slider2.fillAmount = 1.0f - time01;
        }
    }
}
