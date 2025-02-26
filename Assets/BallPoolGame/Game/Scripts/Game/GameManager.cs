using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BallPool;
using BallPool.AI;
using BallPool.Mechanics;
using NetworkManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerUI mainPlayerUI;
    [SerializeField] private PlayerUI otherPlayerUI;
    [SerializeField] private Text prize;
    [SerializeField] private Text gameInfo;
    [SerializeField] private Text aiCountText;
    public PlayAgainMenu playAgainMenu;    
    [SerializeField] private GameUIController gameUIController;
    public ShotController shotController;
    [SerializeField] private PhysicsManager physicsManager;
    [SerializeField] private BallPoolAIManager aiManager;
    [SerializeField] private TimeController timeController;

    public Ball[] balls;
    private bool playAgainMenuIsActive;
    private AightBallPoolGameManager aightBallPoolGameManager;
    private bool applicationIsPaused;
    private int applicationPauseSeconds;

    #region New
    public static GameManager Instance;
    public List<Ball> ActiveBalls = new List<Ball>();
    public List<Ball> DeactiveBalls = new List<Ball>();
  
    public void ActiveBall(int id, bool isAll = false)
    {
        if (isAll)
        {
            ActiveBalls.Clear();
            DeactiveBalls.Clear();
            ActiveBalls.AddRange(balls);
            ActiveBalls.RemoveAt(0);
            return;
        }

        Ball ball = balls[id];
        if (DeactiveBalls.Contains(ball))
        {
            DeactiveBalls.Remove(ball);
        }
        if (!ActiveBalls.Contains(ball))
        {
            ActiveBalls.Add(ball);
        }
    }
    public void DeactiveBall(int id)
    {
        Ball ball = balls[id];
        if (ActiveBalls.Contains(ball))
        {
            ActiveBalls.Remove(ball);
        }
        if (!DeactiveBalls.Contains(ball))
        {
            DeactiveBalls.Add(ball);
        }
    }
    #endregion

    void Awake()
    {
        Instance = this;
        if (!NetworkManager.initialized)
        {
            Debug.LogWarning("not initialized");
            gameUIController.GoHome();
            enabled = false;
            return;
        }

        DataManager.SaveGameData();
        UpdateAICount();
        if (BallPoolGameLogic.playMode == BallPool.PlayMode.Replay)
        {
            enabled = false;
            shotController.enabled = false;
            TriggerPlayAgainMenu(false);

            physicsManager.OnBallMove += (int ballId, Vector3 position, Vector3 velocity, Vector3 angularVelocity) => 
                {
                    if(balls[ballId].inPocket)
                    {
                        balls[ballId].OnState(BallState.MoveInPocket);
                    }
                    else
                    {
                        balls[ballId].OnState(BallState.Move);
                    }
                };
            
            return;
        }
        NetworkManager.network.OnNetwork += NetworkManager_network_OnNetwork; 

        aightBallPoolGameManager = new AightBallPoolGameManager();
        aightBallPoolGameManager.Initialize(physicsManager, aiManager, balls);
        aightBallPoolGameManager.maxPlayTime = timeController.maxPlayTime;
        aightBallPoolGameManager.OnEndTime += AightBallPoolGameManager_OnEndTime;
        aightBallPoolGameManager.OnSetGameInfo += AightBallPoolGameManager_OnSetGameInfo;
        gameUIController.OnForceGoHome += GameUIController_OnForceGoHome;

        aightBallPoolGameManager.gameLogic = new AightBallPoolGameLogic();
    }
        
    void Start()
    {
        TriggerPlayAgainMenu(false);

        aightBallPoolGameManager.OnCalculateAI += AightBallPoolGameManager_OnCalculateAI;
        aightBallPoolGameManager.OnSetPrize += (int prize) => { this.prize.text = prize + ""; };
        aightBallPoolGameManager.OnSetPlayer += AightBallPoolGameManager_OnSetPlayer;
        aightBallPoolGameManager.OnSetAvatar += AightBallPoolGameManager_OnSetAvatar;
        aightBallPoolGameManager.OnSetActivePlayer += AightBallPoolGameManager_OnSetActivePlayer;
        aightBallPoolGameManager.OnGameComplite += AightBallPoolGameManager_OnGameComplite;
        aightBallPoolGameManager.OnSetActiveBallsIds += AightBallPoolGameManager_OnSetActiveBallsIds;
        aightBallPoolGameManager.OnEnableControl += (bool value) =>
            {
                shotController.OnEnableControl(value);
            };

        shotController.OnEndCalculateAI += ShotController_OnEndCalculateAI;
        shotController.OnSelectBall += ShotController_OnSelectBall;
        shotController.OnUnselectBall += ShotController_OnUnselectBall;
        physicsManager.OnSaveEndStartReplay += PhysicsManager_OnSaveEndStartReplay;

        if (BallPoolGameLogic.playMode != BallPool.PlayMode.Replay)
        {
            BallPoolPlayer.UpdateCouns(true);
            BallPoolPlayer.SaveCoins();
        }
        aightBallPoolGameManager.Start();

        if (BallPoolGameLogic.isOnLine)
        {
            NetworkManager.network.SendRemoteMessage("OnOpponenWaitingForYourTurn");
            NetworkManager.network.SendRemoteMessage("OnOpponenInGameScene");
            StartCoroutine(UpdateNetworkTime());
        }
        gameInfo.text = (BallPoolPlayer.mainPlayer.myTurn? "Your turn":"Your opponent turn") + "\nGood lack!";
        StartCoroutine(HideGameInfoText(gameInfo.text));

        Resources.UnloadUnusedAssets();

        //
        ActiveBall(0, true);
    }


    void Update()
    {
        aightBallPoolGameManager.Update(Time.deltaTime);
        timeController.UpdateTime(aightBallPoolGameManager.playTime);
        if (BallPoolGameLogic.controlFromNetwork && !shotController.inMove && !physicsManager.inMove)
        {
            shotController.UpdateFromNetwork();
        }
    }
   
    public void SetBallsState(int number)
    {
        for (int i = 0; i < balls.Length; i++)
        {
            balls[i].SetMechanicalState(number);
        }
    }
    IEnumerator UpdateNetworkTime()
    {
        while (NetworkManager.network != null)
        {
            if (BallPoolGameLogic.controlInNetwork && !shotController.inMove)
            {
                yield return new WaitForSeconds(0.3f);
                SendToNetwork();
            }
            else
            {
                yield return null;
            }
        }
    }
    void PhysicsManager_OnSaveEndStartReplay (string impulse)
    {
        NetworkManager.network.OnMadeTurn();
        if (BallPoolGameLogic.controlInNetwork)
        {
            NetworkManager.network.SendRemoteMessage("StartSimulate", impulse);
        }
    }
    private void SendToNetwork()
    {
        if (!NetworkManager.network || aightBallPoolGameManager == null)
        {
            return;
        }
        NetworkManager.network.SendRemoteMessage("OnSendTime", aightBallPoolGameManager.playTime);
        if (shotController.cueChanged)
        {
            NetworkManager.network.SendRemoteMessage("OnSendCueControl", shotController.cuePivot.localRotation.eulerAngles.y, shotController.cueVertical.localRotation.eulerAngles.x, 
                new Vector2(shotController.cueDisplacement.localPosition.x, shotController.cueDisplacement.localPosition.y), shotController.cueSlider.localPosition.z, shotController.force);
        }
        if (shotController.ballChanged)
        {
            Debug.LogWarning("ballChanged");
            NetworkManager.network.SendRemoteMessage("OnMoveBall", shotController.cueBall.position);
        }
    }
    private void SendToNetworkOnEnd()
    {
        Debug.LogWarning("SendToNetworkOnEnd");
        NetworkManager.network.SendRemoteMessage("OnSendTime", aightBallPoolGameManager.playTime);
    }
    void ShotController_OnSelectBall ()
    {
        if (BallPoolGameLogic.controlInNetwork)
        {
            NetworkManager.network.SendRemoteMessage("SelectBallPosition", shotController.cueBall.position);
        }
    }
    void ShotController_OnUnselectBall ()
    {
        if (BallPoolGameLogic.isOnLine)
        {
            Debug.LogWarning("ShotController_OnUnselectBall" + BallPoolGameLogic.controlInNetwork);
            NetworkManager.network.SendRemoteMessage("SetBallPosition", shotController.cueBall.position);
        }
    }
    void OnDisable()
    {
        if (aightBallPoolGameManager != null)
        {
            aightBallPoolGameManager.OnDisable();
            aightBallPoolGameManager = null;
        }
        NetworkManager.Disable();
    }
    void NetworkManager_network_OnNetwork (NetworkState state)
    {
        Debug.Log("NetworkManager_network_OnNetwork " + state);
        if (BallPoolGameLogic.playMode == BallPool.PlayMode.OnLine)
        {
            if (state != NetworkState.Connected)
            {
                shotController.enabled = false;
                StartCoroutine(WaitAndGoToHome(state));
            }
        }
    }
    IEnumerator WaitAndGoToHome(NetworkState state)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        enabled = false;
        if (!playAgainMenu.wasOpened && state == NetworkState.LeftRoom)
        {
            OpenPlayAgainMenuForMainPlayer();
        }
        else if (playAgainMenu.wasOpened || state == NetworkState.LostConnection)
        {
            gameUIController.GoHome();
        }
    }
    void OnApplicationPause(bool pauseStatus)
    {
        if (Application.isEditor)
        {
            return;
        }
       
        if (pauseStatus)
        {
            applicationPauseSeconds = GetTimeInSeconds();
        }
        else
        {
            if (applicationIsPaused && Mathf.Abs(applicationPauseSeconds - GetTimeInSeconds()) > 3)
            {
                gameUIController.ForceGoHome();
            }
        }
        applicationIsPaused = pauseStatus;
    }
    private int GetTimeInSeconds()
    {
        return 60 * 60 * System.DateTime.Now.Hour + 60 * System.DateTime.Now.Minute + System.DateTime.Now.Second;
    }

    public void UpdateAICount()
    {
        aiCountText.text = ProductAI.aiCount + "";
    }
    private void OpenPlayAgainMenuForMainPlayer()
    {
        shotController.shotBack.enabled = false;
        BallPoolPlayer.SetWinner(BallPoolPlayer.mainPlayer.playerId);
        if(shotController.opponenIsReadToPlay)
        {
            BallPoolPlayer.SetMainPlayerWinnerCouns();
        }
        else
        {
            BallPoolPlayer.ReturnMainPlayerCouns();
        }
        BallPoolPlayer.SaveCoins();
        playAgainMenu.ShowMainPlayer();
        mainPlayerUI.SetPlayer(AightBallPoolPlayer.mainPlayer);
        otherPlayerUI.SetPlayer(AightBallPoolPlayer.otherPlayer);

    }

    void GameUIController_OnForceGoHome ()
    {
        if (BallPoolGameLogic.isOnLine)
        {
            NetworkManager.network.SendRemoteMessage("OnOpponentForceGoHome");
        }
        aightBallPoolGameManager.OnForceGoHome(AightBallPoolPlayer.otherPlayer.playerId);
    }


    private void TriggerPlayAgainMenu(bool isOn)
    {
        if (isOn)
        {
            if (!playAgainMenu.wasOpened)
            {
                BallPoolPlayer.UpdateCouns(false);
                BallPoolPlayer.SaveCoins();
            }
            shotController.shotBack.enabled = false;
            BallPoolPlayer winner = BallPoolPlayer.GetWinner();
            mainPlayerUI.SetPlayer(AightBallPoolPlayer.mainPlayer);
            otherPlayerUI.SetPlayer(AightBallPoolPlayer.otherPlayer);
            playAgainMenu.Show(winner);
        }
        else
        {
            playAgainMenu.Hide();
        }
    }

    void AightBallPoolGameManager_OnEndTime ()
    {
        if (BallPoolGameLogic.controlInNetwork)
        {
            SendToNetworkOnEnd();
        }
        shotController.OnEndTime();
        shotController.UndoShot();
    }
    private bool gameInfoInProgress = false;

    void AightBallPoolGameManager_OnSetGameInfo (string info)
    {
        StartCoroutine(HideGameInfoText(info));
    }
    IEnumerator HideGameInfoText(string info)
    {
        while (gameInfoInProgress)
        {
            yield return null;
        }
        gameInfoInProgress = true;
        gameInfo.text = info;
        yield return new WaitForSeconds(5.0f);
        gameInfo.text = "";
        gameInfoInProgress = false;
    }
    void AightBallPoolGameManager_OnSetPlayer (BallPoolPlayer player)
    {
        if (player.playerId == 0)
        {
            mainPlayerUI.SetPlayer(player);
        }
        else if (player.playerId == 1)
        {
            otherPlayerUI.SetPlayer(player);
        }
    }

    private IEnumerator DownloadAndSetAvatar(BallPoolPlayer player)
    {
        if (player.playerId == 0)
        {
            if (AightBallPoolPlayer.mainPlayer.avatar == null)
            {
                yield return StartCoroutine(AightBallPoolPlayer.mainPlayer.DownloadAvatar());
            }
            if (AightBallPoolPlayer.mainPlayer.avatar != null)
            {
                mainPlayerUI.avatarImage.texture = (Texture2D)AightBallPoolPlayer.mainPlayer.avatar;
            }
        }
        else if (player.playerId == 1)
        {
            if (AightBallPoolPlayer.otherPlayer.avatar == null)
            {
                yield return StartCoroutine(AightBallPoolPlayer.otherPlayer.DownloadAvatar());
            }
            if (AightBallPoolPlayer.otherPlayer.avatar != null)
            {
                otherPlayerUI.avatarImage.texture = (Texture2D)AightBallPoolPlayer.otherPlayer.avatar;
            }
        }
    }

    private IEnumerator WaitAndShotAI()
    {
        yield return new WaitForSeconds(0.3f);
        if (!shotController.inMove && !shotController.activateAfterCalculateAI)
        {
            gameUIController.Shot(false);
        }
    }
    void AightBallPoolGameManager_OnSetActiveBallsIds (BallPoolPlayer player)
    {
        if (player.playerId == 0)
        {
            mainPlayerUI.SetActiveBallsIds(player);
        }
        else if (player.playerId == 1)
        {
            otherPlayerUI.SetActiveBallsIds(player);
        }
    }

    void AightBallPoolGameManager_OnGameComplite ()
    {
        shotController.enabled = false;
        TriggerPlayAgainMenu(true);
    }

    void AightBallPoolGameManager_OnSetActivePlayer (BallPoolPlayer player, bool value)
    {
        if (player.playerId == 0)
        {
            mainPlayerUI.SetActive(value);
        }
        else if (player.playerId == 1)
        {
            otherPlayerUI.SetActive(value);
        }
    }

    void AightBallPoolGameManager_OnCalculateAI ()
    {
        shotController.enabled = false;
        StopCoroutine("WaitAndCalculateAI");
        StartCoroutine("WaitAndCalculateAI");
    }
    private IEnumerator WaitAndCalculateAI()
    {
        yield return new WaitForSeconds(0.3f);
        aiManager.CalculateAI();
    }

    void AightBallPoolGameManager_OnSetAvatar (BallPoolPlayer player)
    {
        StartCoroutine(DownloadAndSetAvatar(player));
    }

    void ShotController_OnEndCalculateAI()
    {
        StartCoroutine(WaitAndShotAI());
    }
}
