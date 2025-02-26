using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BallPool;

/// <summary>
/// The game time UI controller.
/// </summary>
public abstract class TimeController : MonoBehaviour
{
    public float maxPlayTime = 30.0f;
    protected float time01;
    public void UpdateTime(float time01)
    {
        if (this.time01 != time01)
        {
            this.time01 = time01;
            OnUpdateTime(time01);
        }
    }
    protected abstract void OnUpdateTime(float time01);
}
