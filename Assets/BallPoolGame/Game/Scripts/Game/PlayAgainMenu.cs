using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BallPool;

public class PlayAgainMenu : MonoBehaviour 
{
    [SerializeField] GameObject menu;
    [SerializeField] private Text winnerName;
    [SerializeField] private Text winnerCoins;
    [SerializeField] private RawImage winnerImage;
    [SerializeField] private RectTransform playAgainButton;

    private bool _wasOpened = false;
    public bool wasOpened{ get{ return _wasOpened; } }

    public void HidePlayAgainButton()
    {
        playAgainButton.gameObject.SetActive(false);
    }
    public void Hide()
    {
        menu.SetActive(false);
    }
    public void Show(BallPoolPlayer player)
    {
        winnerName.text = player.name;
        winnerImage.texture = (Texture2D)player.avatar;
        winnerCoins.text = player.coins + "";
        menu.SetActive(true);
        _wasOpened = true;
    }

    public void ShowMainPlayer()
    {
        HidePlayAgainButton();
        Show(BallPoolPlayer.mainPlayer);
    }
}
