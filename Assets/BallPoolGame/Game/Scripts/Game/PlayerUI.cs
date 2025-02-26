using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BallPool;
using BallPool.Mechanics;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private bool isMainPlayer;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text nameText;
    public RawImage avatarImage;
    [SerializeField] private BallsUIManager ballsUIManager;
    [SerializeField] private Image[] ballsImage;
    private Text[] ballsText;
    private Image[] ballsImageColor;


    private List<Ball> balls;

    void Awake()
    {
        ballsText = new Text[ballsImage.Length];
        ballsImageColor = new Image[ballsImage.Length];
        for (int i = 0; i < ballsImage.Length; i++)
        {
            ballsText[i] = ballsImage[i].GetComponentInChildren<Text>();
            ballsImageColor[i] = ballsImage[i].transform.Find("Color").GetComponent<Image>();
        }
    }
    public void SetPlayer(BallPoolPlayer player)
    {
        nameText.text = player.name;
        coinsText.text = player.coins + "";
    }
    public void SetActiveBallsIds(BallPoolPlayer player)
    {
        string[] activeBallsIds = player.GetActiveBallsIds();
        if (activeBallsIds == null)
        {
            return;
        }
        for (int i = 0; i < ballsImage.Length; i++)
        {
            if (i < activeBallsIds.Length)
            {
                int id = int.Parse(activeBallsIds[i]);
                ballsText[i].text = id + "";
                if (isMainPlayer)
                {
                    ballsImageColor[i].sprite = AightBallPoolPlayer.mainPlayer.isSolids ? ballsUIManager.solidsBall : (AightBallPoolGameLogic.isBlackBall(id) ? ballsUIManager.solidsBall : ballsUIManager.stripesBall);
                }
                else
                {
                    ballsImageColor[i].sprite = AightBallPoolPlayer.otherPlayer.isSolids ? ballsUIManager.solidsBall : (AightBallPoolGameLogic.isBlackBall(id) ? ballsUIManager.solidsBall : ballsUIManager.stripesBall);
                }
                Color color = ballsUIManager.ballsColors[id - 1];
                ballsImageColor[i].color = new Color(color.r, color.g, color.b);
            }
            else
            {
                ballsText[i].text = "";
                ballsImageColor[i].sprite = ballsUIManager.defaultBall;
                Color color = ballsUIManager.defaultColor;
                ballsImageColor[i].color = new Color(color.r, color.g, color.b);
            }
        }
    }

    public void SetActive(bool value)
    {
        avatarImage.gameObject.SetActive(value);
    }
}
