#if PHOTON_UNITY_NETWORKING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;
using Photon.Realtime;
using Photon.Pun;

public class PunNetwork : NetworkEngine, AightBallPoolMessenger
{
    private PhotonView view;
    private Player opponentPlayer;
    private AightBallPoolNetworkMessenger messenger;

    public override void Inicialize()
    {
        if (!messenger)
        {
            messenger = gameObject.AddComponent<AightBallPoolNetworkMessenger>();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Connect ");
        sendRate = 10;
        PhotonNetwork.SendRate = sendRate;
        PhotonNetwork.SerializationRate = sendRate;
        //PhotonNetwork.autoJoinLobby = true;
        //PhotonNetwork.EnableLobbyStatistics = true;
        view = gameObject.AddComponent<PhotonView>();
        view.ObservedComponents = new List<Component>(0);
        view.ObservedComponents.Add(this);
        view.Synchronization = ViewSynchronization.Off;
        view.ViewID = 1;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.KeepAliveInBackground = BackgroundTimeout;
        Connect();
    }

    public override void Disable()
    {
        base.Disable();
    }
    protected override void Update()
    {
        base.Update();
    }

    public override void SendRemoteMessage(string message, params object[] args)
    {
        view.RPC(message, opponentPlayer, args);
    }
 
    public override void OnGoToPLayWithPlayer(PlayerProfile player)
    {
        if (PhotonNetwork.LocalPlayer != PhotonNetwork.MasterClient)
        {
            adapter.homeMenuManager.UpdatePrize(player.prize);

            Debug.LogWarning("SetPrize " + player.prize);
            NetworkManager.opponentPlayer = player;
            PhotonNetwork.JoinRoom(NetworkManager.PlayerToString(NetworkManager.opponentPlayer));
        }
        else
        {
            //The client who first created the room
            view.RPC("OnOpponenReadToPlay", opponentPlayer, NetworkManager.PlayerToString(NetworkManager.mainPlayer), AightBallPoolNetworkGameAdapter.is3DGraphics);
        }
    }


   
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.LocalPlayer != PhotonNetwork.MasterClient)
        {
            opponentPlayer = PhotonNetwork.MasterClient;
            view.RPC("OnOpponenReadToPlay", opponentPlayer, NetworkManager.PlayerToString(NetworkManager.mainPlayer), AightBallPoolNetworkGameAdapter.is3DGraphics);
        }
        CallNetworkState(NetworkState.JoinedToRoom);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPhotonPlayerConnected " + newPlayer.ActorNumber);
        opponentPlayer = newPlayer;
    }

    public override void OnConnected()
    {
        CallNetworkState(NetworkState.Connected);
        Debug.Log("state " + state);
    }

    public override void OnMasterClientSwitched (Player  newMasterClient )
    {
        Debug.LogWarning("OnMasterClientSwitched");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        CallNetworkState(NetworkState.LostConnection);
        tryToConnect = false;
        Debug.LogWarning(state);
    }
        
    public override void CreateRoom()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            PhotonNetwork.LeaveRoom();
        }
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        {
            PhotonNetwork.CreateRoom(NetworkManager.PlayerToString(NetworkManager.mainPlayer)/* + PhotonNetwork.GetRoomList().Length*/, new RoomOptions() { MaxPlayers = 2 }, null);
        }
    }
    public override void Resset()
    {
        LeftRoom();
    }
    public override void LeftRoom()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    private bool tryToConnect;
    public override void Connect()
    {
        if (view && reachable)
        {
            Debug.Log("photonView " + view + "  reachable " + reachable + " NetworkClientState " + PhotonNetwork.NetworkClientState);
            if(!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                tryToConnect = true;
            }
            else if (!tryToConnect)
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
        
        
    public override void Disconnect()
    {
        if (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnJoinedLobby ()
    {
        
    }

    public override void StartUpdatePlayers()
    {
        StartCoroutine(UpdatePlayers());
    }
    private IEnumerator UpdatePlayers()
    {
        NetworkManager.social.UpdateFriendsList();
        while (!NetworkManager.social.friendsListIsUpdated)
        {
            yield return null;
        }
        NetworkManager.UpdatePlayers();
        StartCoroutine(NetworkManager.LoadRandomPlayer());
        StartCoroutine(NetworkManager.LoadFriendsAndRandomPlayers(50));
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        
    }
    public override void OnCreatedRoom()
    {
        StartUpdatePlayers();
        CallNetworkState(NetworkState.CreatedRoom);
    }

    public override void OnLeftRoom()
    {
        opponentPlayer = null;
        PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
        CallNetworkState(NetworkState.LeftRoom);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("OnPhotonPlayerDisconnected");
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.RemoveRPCs(otherPlayer);
        PhotonNetwork.DestroyPlayerObjects(otherPlayer);
    }

    private List<RoomInfo> roomList;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        this.roomList = roomList;
        StartUpdatePlayers();
    }

    public override void LoadPlayers(ref PlayerProfile[] players)
    {
        RoomInfo[] rooms = roomList?.ToArray();
        List<PlayerProfile> playersList = new List<PlayerProfile>(0);

        if (rooms != null)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                RoomInfo room = rooms[i];
                if (room.PlayerCount < room.MaxPlayers)
                {
                    playersList.Add(NetworkManager.PlayerFromString(rooms[i].Name, ChackIsFriend));
                }
            }
        }
        players = playersList.ToArray();
    }
    public override bool ChackIsFriend(string id)
    {
        string[] friendsId = NetworkManager.social.GetFriendsId();
        if (friendsId != null)
        {
            foreach (string friendId in friendsId)
            {
                if (id == friendId)
                {
                    return true;
                }
            }
        }
        return false;
    }
    [PunRPC]
    public override void OnOpponenReadToPlay(string playerData, bool is3DGraphicMode)
    {
        NetworkManager.opponentPlayer = NetworkManager.PlayerFromString(playerData, ChackIsFriend);
        AightBallPoolNetworkGameAdapter.isSameGraphicsMode = AightBallPoolNetworkGameAdapter.is3DGraphics == is3DGraphicMode;
        Debug.Log("OnOpponenReadToPlay " + playerData);
        CallNetworkState(NetworkState.OpponentReadToPlay);
        if (PhotonNetwork.LocalPlayer != PhotonNetwork.MasterClient)
        {
            int turnId = Random.Range(0, 2);
            int turnIdForSend = turnId == 1 ? 0 : 1;
            OnOpponenStartToPlay(turnId);
            view.RPC("OnOpponenStartToPlay", opponentPlayer, turnIdForSend);
        }
    }
    [PunRPC]
    public override void OnOpponenStartToPlay(int turnId)
    {
        Debug.Log(" OnOpponenStartToPlay " + turnId);
        adapter.SetTurn(turnId);
        adapter.homeMenuManager.GoToPlay();
    }

    [PunRPC]
    public override void OnSendTime(float time01)
    {
        messenger.SetTime(time01);
    }
    [PunRPC]
    public override void StartSimulate(string impulse)
    {
        messenger.StartSimulate(impulse);
    }
    [PunRPC]
    public override void EndSimulate(string ballsState)
    {
        messenger.EndSimulate(ballsState);
    }
    [PunRPC]
    public override void OnOpponenWaitingForYourTurn()
    {
        base.OnOpponenWaitingForYourTurn();
    }
    [PunRPC]
    public override void OnOpponenInGameScene()
    {
        StartCoroutine(messenger.OnOpponenInGameScene());
    }
    [PunRPC]
    public override void OnOpponentForceGoHome()
    {
        messenger.OnOpponentForceGoHome();
    }
    #region AightBallPool Interface
    [PunRPC]
    public void OnSendCueControl(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
    {
        messenger.OnSendCueControl(cuePivotLocalRotationY, cueVerticalLocalRotationX, cueDisplacementLocalPositionXY, cueSliderLocalPositionZ, force);
    }
    [PunRPC]
    public void OnForceSendCueControl(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
    {
        messenger.OnForceSendCueControl(cuePivotLocalRotationY, cueVerticalLocalRotationX, cueDisplacementLocalPositionXY, cueSliderLocalPositionZ, force);
    }
    [PunRPC]
    public void OnMoveBall(Vector3 ballPosition)
    {
        messenger.OnMoveBall(ballPosition);
    }
    [PunRPC]
    public void SelectBallPosition(Vector3 ballPosition)
    {
        messenger.SelectBallPosition(ballPosition);
    }
    [PunRPC]
    public void SetBallPosition(Vector3 ballPosition)
    {
        messenger.SetBallPosition(ballPosition);
    }

    [PunRPC]
    public void SetMechanicalStatesFromNetwork(int ballId, string mechanicalStateData)
    {
        messenger.SetMechanicalStatesFromNetwork(ballId, mechanicalStateData);
    }
    [PunRPC]
    public void WaitAndStopMoveFromNetwork(float time)
    {
        messenger.WaitAndStopMoveFromNetwork(time);
    }
    [PunRPC]
    public void SendOpponentCueURL(string url)
    {
        messenger.SetOpponentCueURL(url);
    }
    [PunRPC]
    public void SendOpponentTableURLs(string boardURL, string clothURL, string clothColor)
    {
        messenger.SetOpponentTableURLs(boardURL, clothURL, clothColor);
    }
    #endregion
}
#endif
