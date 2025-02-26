using System.Collections;
using System.Collections.Generic;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif
using UnityEngine;

namespace NetworkManagement
{
    public delegate void NetworkHandler(NetworkState state);

    /// <summary>
    /// The network current state.
    /// </summary>
    public enum NetworkState
    {
        Disconnected = 0,
        Connected,
        LostConnection,
        CreatedRoom,
        JoinedToRoom,
        LeftRoom,
        RoomCreateFailed,
        JoinRoomFailed,
        OpponentReadToPlay
    }
    
    /// <summary>
    /// The network API, who works with some network system.
    /// </summary>
#if PHOTON_UNITY_NETWORKING
    public abstract class NetworkEngine : MonoBehaviourPunCallbacks
#else
public abstract class NetworkEngine : MonoBehaviour
#endif
    {
        public event NetworkHandler OnNetwork;

        public NetworkGameAdapter adapter { get; private set; }

        public int sendRate { get; protected set; }

        public bool opponenWaitingForYourTurn { get; protected set; }


        /// <summary>
        /// Sends the remote message to opponent.
        /// </summary>
        public abstract void SendRemoteMessage(string message, params object[] args);

        public abstract void OnGoToPLayWithPlayer(PlayerProfile player);


        public void SetAdapter(NetworkGameAdapter adapter)
        {
            this.adapter = adapter;
        }

        protected void CallNetworkState(NetworkState state)
        {
            Debug.Log("CallNetworkState " + state);
            this.state = state;
            if (OnNetwork != null)
            {
                OnNetwork(state);
            }
        }

        public virtual void Inicialize()
        {
            opponenWaitingForYourTurn = false;
        }

        protected const float BackgroundTimeout = 10.0f;
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            state = NetworkState.Disconnected;
        }
        private float reachableTimer = 0.0f;
        protected bool reachable { get { return (Application.internetReachability != NetworkReachability.NotReachable); } }
        protected bool oldReachable = false;


        protected virtual void Update()
        {
            if (oldReachable != reachable)
            {
                if (!reachable)
                {
                    if (oldReachable != reachable)
                    {
                        if (reachableTimer > BackgroundTimeout)
                        {
                            reachableTimer = 0.0f;
                            oldReachable = reachable;
                        }
                        else
                        {
                            reachableTimer += Time.deltaTime;
                        }
                    }
                }
                else
                {
                    oldReachable = reachable;
                    reachableTimer = 0.0f;
                }

                if (!oldReachable)
                {
                    CallNetworkState(NetworkState.LostConnection);
                }
            }
            else
            {
                reachableTimer = 0.0f;
            }
        }

        IEnumerator Start()
        {
            if (reachable)
            {
                Connect();
            }

            while (true)
            {
                yield return new WaitForSeconds(3.0f);
                if ((state == NetworkState.Disconnected || state == NetworkState.LostConnection))
                {
                    if (reachable)
                    {
                        Debug.Log("IEnumerator Start Connect ");
                        Connect();
                    }
                }
            }
        }

        public virtual void Disable()
        {
            OnNetwork = null;
        }

        public NetworkState state
        {
            get;
            private set;
        }

        public abstract void Resset();

        public abstract void Disconnect();

        public abstract bool ChackIsFriend(string id);

        public abstract void CreateRoom();

        public abstract void LeftRoom();

        public abstract void Connect();

        public abstract void OnOpponenReadToPlay(string playerData, bool is3DGraphicMode);

        public abstract void OnOpponenStartToPlay(int turnId);

        public abstract void OnSendTime(float time01);

        public void OnMadeTurn()
        {
            opponenWaitingForYourTurn = false;
        }

        public abstract void StartSimulate(string ballsState);

        public abstract void EndSimulate(string ballsState);

        public virtual void OnOpponenWaitingForYourTurn()
        {
            opponenWaitingForYourTurn = true;
        }

        public abstract void OnOpponenInGameScene();
        public abstract void OnOpponentForceGoHome();
        public abstract void StartUpdatePlayers();

        public abstract void LoadPlayers(ref NetworkManagement.PlayerProfile[] players);
    }
}
