using System.Collections.Generic;
using UnityEngine;

public class BallsUIManager : MonoBehaviour 
{
    public Sprite defaultBall;
    public Color defaultColor;
    public Sprite solidsBall;
    public Sprite stripesBall;
    public Color[] ballsColors;

    public static BallsUIManager Instance;

    [SerializeField] private List<BallUI> ballUIList = new List<BallUI>();

    private void Awake()
    {
        Instance = this;
        Restart();
    }

    void Restart()
    {
        foreach (BallUI ball in ballUIList)
        {
            ball.Restart();
        }
    }

    public void NullOneBall(int id)
    {
        foreach (BallUI ball in ballUIList)
        {
            if (ball.ID == id)
            {
                ball.SetNull();
                break;
            }
        }
    }
}
