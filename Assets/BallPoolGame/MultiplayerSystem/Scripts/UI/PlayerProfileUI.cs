using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkManagement;

/// <summary>
/// The player profile UI.
/// </summary>
public class PlayerProfileUI : MonoBehaviour
{
    public RawImage avatarImage;
    private Texture nullTexture;
    private Texture2D defaulImage;
    [SerializeField] private Text userName;
    [SerializeField] private Text coins;
    [SerializeField] private Text prize;
    [SerializeField] private Image state;
    [SerializeField] private Text isFriend;
    [SerializeField] private Text waitingOpponent;
    private float waitingProgress;
    private int waitingProgressInt;
    private float orient = 1.0f;
    private bool isFirstTimeSet = true;

    public void SetPrizeColor(Color color)
    {
        prize.color = color;
    }
    void Update()
    {
        if (!waitingOpponent)
        {
            enabled = false;
        }
        waitingProgress += 2.0f * orient * Time.deltaTime;
       
        int currentWaitingProgressInt = (int)waitingProgress;
        if (waitingProgress > 3.49f || waitingProgress < -0.49f)
        {
            orient *= -1.0f;
        }
        if (waitingProgressInt != currentWaitingProgressInt)
        {
            waitingProgressInt = currentWaitingProgressInt;
            string txt = "Waiting";
            for (int i = 1; i <= waitingProgressInt; i++)
            {
                txt += ".";
            }
            waitingOpponent.text = txt;
        }
    }
    public NetworkManagement.PlayerProfile player
    {
        get;
        private set;
    }
    public void UpdateCoinsFromPlayer()
    {
        if (player != null)
        {
            coins.text = player.coins + "";
        }
    }
    public void UpdatePrizeFromPlayer()
    {
        if (player != null)
        {
            prize.text = player.prize + "";
        }
    }
    public void SetPlayer(NetworkManagement.PlayerProfile player)
    {
        if (isFirstTimeSet)
        {
            defaulImage = (Texture2D)avatarImage.mainTexture;
            isFirstTimeSet = false;
        }
        if (nullTexture == null)
        {
            nullTexture = avatarImage.texture;
        }

        this.player = player;
        if (player == null)
        {
            enabled = true;
            avatarImage.texture = nullTexture;
            userName.text = "";
            coins.text = "";
            prize.text = "";
            state.enabled = false;
            if (isFriend)
            {
                isFriend.gameObject.SetActive(false);
            }
            waitingOpponent.gameObject.SetActive(true);
            return;
        }
        enabled = false;
        if (waitingOpponent)
        {
            waitingOpponent.gameObject.SetActive(false);
        }
        if (player.image)
        {
            avatarImage.texture = player.image;
        }
        else
        {
            if (!string.IsNullOrEmpty(player.imageURL) && gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(DownloadAvatar());
            }
            else
            {
                player.SetImage(defaulImage);
                avatarImage.texture = defaulImage;
            }
        }
        userName.text = player.userName;
        coins.text = player.coins + "";
        prize.text = player.prize + "";
        state.enabled = true;
        switch (player.state)
        {
            case PlayerState.Offline:
                state.color = Color.Lerp(Color.black, Color.white, 0.7f);
                break;
            case PlayerState.Online:
                state.color = Color.green;
                break;
            case PlayerState.Away:
                state.color = Color.yellow;
                break;
            case PlayerState.Busy:
                state.color = Color.red;
                break;
            case PlayerState.Playing:
                state.color = Color.blue;
                break;
            default:
                break;
        }
        if (isFriend)
        {
            isFriend.gameObject.SetActive(player.isFriend);
        }
    }

    private IEnumerator DownloadAvatar()
    {
        DownloadManager.DownloadParameters parameters = new DownloadManager.DownloadParameters(player.imageURL, player.userName, DownloadManager.DownloadType.Update);
        yield return DownloadManager.Download(parameters);
        if (parameters.texture)
        {
            avatarImage.texture = parameters.texture;
        }
    }
}
