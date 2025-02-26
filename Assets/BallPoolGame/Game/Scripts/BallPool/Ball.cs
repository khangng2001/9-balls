using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

namespace BallPool.Mechanics
{
    public class Ball : MonoBehaviour
    {
        /// <summary>
        /// The ball mechanical state.
        /// </summary>
        public struct MechanicalState
        {
            public readonly float time;
            public readonly int pocketId;
            public readonly int hitShapeId;
            public readonly Vector3 position;
            public readonly Vector3 velocity;
            public readonly Vector3 angularVelocity;

            public MechanicalState(float time, int pocketId, int hitShapeId, Vector3 position, Vector3 velocity, Vector3 angularVelocity)
            {
                this.time = time;
                this.pocketId = pocketId;
                this.hitShapeId = hitShapeId;
                this.position = position;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
            }
            public static string StateToString(MechanicalState state)
            {
                return "[" + state.time.ToString4() + ";" + state.pocketId + ";" + state.hitShapeId + ";" + DataManager.Vector3ToString(state.position) + "; " +  DataManager.Vector3ToString(state.velocity) + "; " +  DataManager.Vector3ToString(state.angularVelocity) + "]";
            }
            public static MechanicalState StateFromString (string state)
            {
                if (state == "")
                {
                    return new MechanicalState();
                }
                string[] values = DataManager.ConvertDataToStringArray(state);
                float time = values[0].ToFloat4();
                int pocketId = int.Parse(values[1], System.Globalization.NumberStyles.Integer);
                int hitShapeId = int.Parse(values[2], System.Globalization.NumberStyles.Integer);
                Vector3 position = DataManager.Vector3FromString(values[3]);
                Vector3 velocity = DataManager.Vector3FromString(values[4]);
                Vector3 angularVelocity = DataManager.Vector3FromString(values[5]);

                return new MechanicalState(time, pocketId, hitShapeId, position, velocity, angularVelocity);
            }
        }
        [System.NonSerialized] public Transform lightCentre;
        [System.NonSerialized] public Transform ballShadow;
        [System.NonSerialized] public Transform ballBlick;
        [SerializeField] private AudioClip ballHitBallClip;
        [SerializeField] private AudioClip ballHitPocketClip;
        [SerializeField] private AudioClip ballHitBoardClip;
        public BallListener listener;

        private AudioSource ballHitBall;
        private AudioSource ballHitBoard;
        private AudioSource ballHitPocket;
        private static int hitBallClipPlayingCount;
        private static int hitBoardClipPlayingCount;

        public int id;

        public float radius{ get; private set; }

        public bool isActive{ get; set; }

        public bool inPocket{ get{ return listener.body.isKinematic;} set{ listener.body.isKinematic = value;}}

        public int pocketId{ get{ return listener.pocketId; } set{ listener.pocketId = value; } }

        public int hitShapeId{ get{ return listener.hitShapeId; } set{ listener.hitShapeId = value; } }

        public bool inSpace{ get{ return Geometry.SphereInCube(position, radius, listener.physicsManager.clothSpace); } }

        public Impulse impulse { get; set; }

        public bool isCast{ get{ return listener.GetComponent<SphereCollider>().enabled;} set{ listener.GetComponent<SphereCollider>().enabled = value; }}


        public Vector3 position{ get { return listener.body.position; } set { transform.position = value; listener.body.position = value; } }

        public HitInfo firstHitInfo{ get; internal set; }

        private float savedTime = -1.0f;
        private Vector3 savedPosition;
        private Vector3 savedVelocity;
        private Vector3 savedlarAnguVelocity;
        public Vector3 savedSleepPosition{ get; set; }

        private bool NeedToSave()
        {
            return false;
//            if (savedTime != listener.physicsManager.moveTime && listener.body.velocity.magnitude < 0.25f)
//            {
//                savedTime = listener.physicsManager.moveTime;
//                if ((position - savedPosition).magnitude > 0.5f * radius)
//                {
//                    savedPosition = position;
//
//                }
//                return true;
//            }
//            return false;
        }

        public IEnumerator WaitAndStopBall(float moveTime)
        {
            while (listener.physicsManager.moveTime < moveTime)
            {
                yield return null;
            }
            if (!listener.body.isKinematic)
            {
                position = savedSleepPosition;
                listener.body.linearVelocity = Vector3.zero;
                listener.body.angularVelocity = Vector3.zero;
                listener.body.Sleep();
            }
        }

        public string mechanicalStateData
        {
            get
            {
                MechanicalState state = new MechanicalState(DataManager.CutValue(listener.physicsManager.moveTime), pocketId, hitShapeId, position, listener.body.linearVelocity, listener.body.angularVelocity);
                return  MechanicalState.StateToString(state); 
            }
            set
            {
                MechanicalState state = MechanicalState.StateFromString(value);
                inPocket = state.pocketId != -1;
                pocketId = state.pocketId;
                position = state.position;
                if (!inPocket)
                {
                    listener.body.linearVelocity = state.velocity;
                    listener.body.angularVelocity = state.angularVelocity;
                }
                if (BallPoolGameLogic.playMode == PlayMode.Replay)
                {
                    OnState(BallState.SetState);
                }
            }
        }
        public string moveData;

        void Awake ()
        {
            if (!NetworkManager.initialized)
            {
                enabled = false;
                return;
            }
                
            hitBallClipPlayingCount = 0;
            hitBoardClipPlayingCount = 0;

            ballHitBall = gameObject.AddComponent<AudioSource>();
            ballHitBall.playOnAwake = false;
            ballHitBall.clip = ballHitBallClip;

            ballHitBoard = gameObject.AddComponent<AudioSource>();
            ballHitBoard.playOnAwake = false;
            ballHitBoard.clip = ballHitBoardClip;

            ballHitPocket = gameObject.AddComponent<AudioSource>();
            ballHitPocket.playOnAwake = false;
            ballHitPocket.clip = ballHitPocketClip;

            radius = listener.body.GetComponent<SphereCollider>().radius;
            listener.body.position = transform.position;
        }
        void Start ()
        {
            if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
            {
                ballBlick.position = CalculateBallBlickPosition();
            }
            ballShadow.position = CalculateBallShadowPosition();

        }
     
        void Update()
        {
            if (!listener.physicsManager.inMove || inPocket || listener.body.isKinematic)
            {
                return;
            }
            SetBallShadowAndBlickBlick();
        }
        public void SetBallShadowAndBlickBlick()
        {
            if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
            {
                ballBlick.position = CalculateBallBlickPosition();
            }
            ballShadow.position = CalculateBallShadowPosition();
        }
        public void SetMechanicalState(int number)
        {
            listener.pocket = null;
            moveData = listener.physicsManager.replayManager.GetReplay(id, number);
            Debug.Log(moveData);
            string[] states = DataManager.ConvertArrayDataToStringArray(moveData);
            if (states != null && states.Length > 0)
            {
                this.mechanicalStateData = states[0];
            }
        }
        Vector3 CalculateBallShadowPosition()
        {
            Vector3 positionInCloth = new Vector3(position.x, 0.1f * radius, position.z);
            return positionInCloth + 0.5f * radius * (position - lightCentre.position);
        }
        Vector3 CalculateBallBlickPosition()
        {
            return position + 1.1f * radius * Vector3.up;
        }

        private string[] mechanicalStates;
        private  float currentTime = 0.0f;
        private float deltaTime = 0.0f;
        private bool isFollow = false;
        private int mechanicalStateId = 0;
        private int oldMechanicalStateId = -1;
        private  MechanicalState currentState;

        public void SrartFollow()
        {
            if (string.IsNullOrEmpty(moveData))
            {
                return;
            }
            mechanicalStates = DataManager.ConvertArrayDataToStringArray(moveData);
            currentTime = 0.0f;
            oldMechanicalStateId = -1;
            mechanicalStateId = 0;
            deltaTime = 0.0f;
            currentState =  MechanicalState.StateFromString(mechanicalStates[mechanicalStateId]);
            if (currentState.pocketId != -1)
            {
                return;
            }
            isFollow = true;
        }

        public IEnumerator SetMechanicalStatesFromNetwork(string state)
        {
            MechanicalState mState = MechanicalState.StateFromString(state);
            while (!listener.physicsManager.endFromNetwork && listener.physicsManager.moveTime < mState.time)
            {
                yield return new WaitForFixedUpdate();
            }
            if (!listener.physicsManager.endFromNetwork)
            {
                FollowMoveFromNetwork(mState);
                mechanicalStateData = state;
            }
        }
        void FollowMoveFromNetwork(MechanicalState mState)
        {
            if (mState.hitShapeId >= 0)
            {
                listener.OnHitBall(listener.physicsManager.ballsListener[mState.hitShapeId]);
            }
            else if (mState.hitShapeId == -1)
            {
                listener.OnHitBoard();
            }
            else if (mState.pocketId != -1)
            {
                listener.OnEnterPocket(listener.physicsManager.pocketListeners[mState.pocketId]);
            }
        }
        void FollowMoveInReplay()
        {
            if (currentState.hitShapeId >= 0)
            {
                OnState(BallState.HitBall);
            }
            else if (currentState.hitShapeId == -1)
            {
                OnState(BallState.HitBoard);
            }
            else if (currentState.pocketId != -1)
            {
                listener.physicsManager.CallOnBallHitPocket(this.listener, listener.physicsManager.pocketListeners[currentState.pocketId], true);
                OnState(BallState.EnterInPocket);
            }
        }
    
        void FixedUpdate()
        {
            if (isFollow)
            {
                if (oldMechanicalStateId != mechanicalStateId)
                {
                    oldMechanicalStateId = mechanicalStateId;
                    currentTime = currentState.time;
                    deltaTime = 0.0f;
                    if (listener.physicsManager.inMove)
                    {
                        FollowMoveInReplay();
                        mechanicalStateData = MechanicalState.StateToString(currentState);
                    }
                   
                    if (mechanicalStateId + 1 < mechanicalStates.Length)
                    {
                        currentState = MechanicalState.StateFromString(mechanicalStates[mechanicalStateId + 1]);
                    }
                    else
                    {
                        isFollow = false;
                    }
                }
                else if(isFollow)
                {
                    deltaTime += Time.fixedDeltaTime;
                    if (deltaTime >= currentState.time - currentTime)
                    {
                        mechanicalStateId++;
                    }
                }
            }
           
        }
   
        public void OnState(BallState state)
        {

            switch (state)
            {
                case BallState.SetState:
                    if (BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        hitShapeId = -2;
                        moveData = mechanicalStateData;
                    }
                    if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                    {
                        ballBlick.position = CalculateBallBlickPosition();
                    }
                    ballShadow.position = CalculateBallShadowPosition();
                    if (inPocket)
                    {
                        ballShadow.gameObject.SetActive(false);
                    }
                    break;
                case BallState.StartMove:
                    break;
                case BallState.Move:
                    position = listener.body.position;
                    transform.rotation = listener.body.rotation;
                    if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                    {
                        ballBlick.position = CalculateBallBlickPosition();
                    }
                    ballShadow.position = CalculateBallShadowPosition();
                    if (NeedToSave() && BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        if (!inPocket)
                        {
                            moveData += mechanicalStateData;
                            if (BallPoolGameLogic.controlInNetwork)
                            {
                                NetworkManager.network.SendRemoteMessage("SetMechanicalStatesFromNetwork", id, mechanicalStateData);
                            }
                        }
                    }
                    break;
                case BallState.EndMove:
                    
                    if (BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        if (!inPocket)
                        {
                            moveData += mechanicalStateData;
                            if (BallPoolGameLogic.controlInNetwork)
                            {
                                NetworkManager.network.SendRemoteMessage("SetMechanicalStatesFromNetwork", id, mechanicalStateData);
                            }
                        }
                        listener.physicsManager.replayManager.SaveReplay(id, moveData);
                        SetBallShadowAndBlickBlick();
                    }
                    break;
                case BallState.EnterInPocket:
                    if (BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        if (!BallPoolGameLogic.controlFromNetwork)
                        {
                            inPocket = true;
                        }
                        moveData += mechanicalStateData;
                        if (BallPoolGameLogic.controlInNetwork)
                        {
                            NetworkManager.network.SendRemoteMessage("SetMechanicalStatesFromNetwork", id, mechanicalStateData);
                        }
                    }
                    ballShadow.gameObject.SetActive(false);
                    if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                    {
                        ballBlick.gameObject.SetActive(false);
                    }
                    if (listener.physicsManager.inMove)
                    {
                        StartCoroutine(WaitAndPlayBallInPocket());
                    }
                    break;
                case BallState.MoveInPocket:
                    position = listener.body.position;
                    transform.rotation = listener.body.rotation;
                    break;
                case BallState.ExitFromPocket:
                    if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                    {
                        ballBlick.gameObject.SetActive(true);
                    }
                    if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                    {
                        ballBlick.position = CalculateBallBlickPosition();
                    }
                    ballShadow.position = CalculateBallShadowPosition();
                    ballShadow.gameObject.SetActive(!inPocket);
                    break;
                case BallState.HitBall:
                    if (BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        if (savedTime != listener.physicsManager.moveTime)
                        {
                            savedTime = listener.physicsManager.moveTime;
                            if (!inPocket)
                            {
                                moveData += mechanicalStateData;
                                if (BallPoolGameLogic.controlInNetwork)
                                {
                                    NetworkManager.network.SendRemoteMessage("SetMechanicalStatesFromNetwork", id, mechanicalStateData);
                                }
                            }
                        }
                    }
                    if (!ballHitBall.isPlaying && hitBallClipPlayingCount < 3 && listener.physicsManager.inMove)
                    {
                        hitBallClipPlayingCount++;
                        ballHitBall.volume = Mathf.Clamp01(2.0f * listener.normalizedVelocity.magnitude);
                        ballHitBall.Play();
                        StartCoroutine(WaitWhenHitBallClipIsPlaying());
                    }
                    break;
                case BallState.HitBoard:
                    if (BallPoolGameLogic.playMode != PlayMode.Replay)
                    {
                        if (savedTime != listener.physicsManager.moveTime)
                        {
                            savedTime = listener.physicsManager.moveTime;
                            if (!inPocket)
                            {
                                moveData += mechanicalStateData;
                                if (BallPoolGameLogic.controlInNetwork)
                                {
                                    NetworkManager.network.SendRemoteMessage("SetMechanicalStatesFromNetwork", id, mechanicalStateData);
                                }
                            }
                        }
                    }
                    if (!ballHitBoard.isPlaying && hitBoardClipPlayingCount < 3 && listener.physicsManager.inMove)
                    {
                        hitBoardClipPlayingCount++;
                        ballHitBoard.volume = Mathf.Clamp01(2.0f * listener.normalizedVelocity.magnitude);
                        ballHitBoard.Play();
                        StartCoroutine(WaitWhenHitBoardClipIsPlaying());
                    }
                    break;
           
                default:
                    break;
            }
        }

        IEnumerator WaitWhenHitBoardClipIsPlaying()
        {
            while (ballHitBoard.isPlaying)
            {
                yield return null;
            }
            hitBoardClipPlayingCount--;
        }

        IEnumerator WaitWhenHitBallClipIsPlaying()
        {
            while (ballHitBall.isPlaying)
            {
                yield return null;
            }
            hitBallClipPlayingCount--;
        }
        IEnumerator WaitAndPlayBallInPocket()
        {
            yield return new WaitForSeconds(0.2f);
            ballHitPocket.volume = Mathf.Clamp(3.0f * listener.normalizedVelocity.magnitude, 0.3f, 1.0f);
            ballHitPocket.Play();
        }
    }
}
