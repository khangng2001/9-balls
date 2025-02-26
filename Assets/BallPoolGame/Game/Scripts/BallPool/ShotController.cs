using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NetworkManagement;
using BallPool.Mechanics;
using BallPool.AI;
using BallPool;

namespace BallPool
{
    public class ShotController : MonoBehaviour
    {
        public static bool canControl = true;
        public enum CueControlType
        {
            FirstPerson = 0,
            ThirdPerson
        }
        public enum CueStateType
        {
            Non = 0,
            TargetingAtCueBall,
            TargetingAtTargetBall,
            TargetingFromTop,
            StretchCue
        }

        public CueControlType cueControlType;
        [SerializeField] private LayerMask cueThirdPersonControlMask;

        private float maxVelocity
        {
            get
            { 
                float verticalAngleFactor = cueVertical.localRotation.eulerAngles.x / (maxVertical - minVertical) > 0.7f ? 1.0f : 0.0f;
                return  Mathf.Lerp(cueBallMaxVelocity, cueBallJumpVelocity,   jumpFactor * verticalAngleFactor); 
            }
        }

        /// <summary>
        /// The cue ball max velocity in metre per second.
        /// </summary>
        [SerializeField] private float cueBallMaxVelocity;
        /// <summary>
        /// The cue ball velocity for jumping in metre per second.
        /// </summary>
        [SerializeField] private float cueBallJumpVelocity;
        /// <summary>
        /// The mouse rotation speed when cue targeting in desktop platform and 3D mode .
        /// </summary>
        [SerializeField] private float mouseRotationSpeed3D;
        /// <summary>
        /// The mouse rotation speed when cue targeting in mobile platform and 3D mode .
        /// </summary>
        [SerializeField] private float rotationSpeed3D;
        /// <summary>
        /// The mouse rotation speed when cue targeting in mobile platform and 2D mode .
        /// </summary>
        [SerializeField] private float rotationSpeed2D;
        /// <summary>
        /// The cue minimum vertical rotation.
        /// </summary>
        [SerializeField] private float minVertical;
        /// <summary>
        /// The cue maximum vertical rotation.
        /// </summary>
        [SerializeField] private float maxVertical;
        /// <summary>
        /// The rotation speed curve.
        /// </summary>
        [SerializeField] private AnimationCurve rotationSpeedCurve;
        /// <summary>
        /// The displacement speed of the cue targeting on the cue ball.
        /// </summary>
        [SerializeField] private float targetingDisplacementSpeed;
        /// <summary>
        /// The cue slide speed before shot
        /// </summary>
        [SerializeField] private float cueSlideSpeed;
        /// <summary>
        /// The length of the cue ball and target balls lines.
        /// </summary>
        [SerializeField] private float lineLength;
        /// <summary>
        /// The cue slider max displacement.
        /// </summary>
        [SerializeField] private float cueSlidingMaxDisplacement;

        /// <summary>
        /// The cue ball radius for selecting and moving the cue ball.
        /// </summary>
        private float cueBallRadius;
        public int clothLayer{ get; set; }
        public int boardLayer{ get; set; }
        public int ballLayer{ get; set; }
        public int cueBallLayer{ get; set; }

        public PhysicsManager physicsManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AightBallPoolAIManager aiManager;
        [SerializeField] private GameUIController uiController;
        private AudioSource cueHitBall;
        [SerializeField] private AudioClip hitBallClip;
        private float jumpFactor;
        public Transform cuePivot;
        [SerializeField] private Transform cuePivotAfterShotPosition;
        [SerializeField] private Transform tableCameraCenter;
        [SerializeField] private Transform cameraThirdPersonPosition;
        private Quaternion cueVerticalStartRotation;
        public Transform cueVertical;
        public Transform cueDisplacement;
        public Transform cueSlider;

        public Transform firstMoveSpace;
        [SerializeField] private Transform clothSpace;

        [SerializeField] private Transform cameraStandartPosition;
        [SerializeField] private Transform cueTargetingIn3DModePosition;
        [SerializeField] private Transform cameraAimingPosition;
        [SerializeField] private Camera cueCamera;

        private float cameraPivotRotationY;
        private float cameraRotationZ;
        public float cueSliderDisplacementZ{ get; private set; }

        private Vector3 cueDisplacementOnBall;
        private CueStateType cueStateType;
        public Ball cueBall;
        [SerializeField] private Transform hand;
        [SerializeField] private LineRenderer cueBallSimpleLine;
        [SerializeField] private LineRenderer targetBallSimpleLine;
        public Vector3 targetBallSavedDirection{ get; private set; }
        public BallListener tragetBallListener{ get; set; }

        private Vector3 savedCueSliderLocalPosition;
        public float force{ get; private set; }
        [SerializeField] private Transform ballChecker;
        [SerializeField] private MeshRenderer ballCheckerRenderer;
        [System.NonSerialized] public Vector3 shotPoint;
        [SerializeField] private Targeting2DManager targeting2DManager;

        [SerializeField] private Load2DCue load2DCue;
        [SerializeField] private Load2DTable load2DTable;
        [SerializeField] private Load3DCue load3DCue;
        [SerializeField] private Load3DTableScene load3DTable;
        private Vector3 cueForwardInScreen;

        private Vector3 oldShotPoint;
        private Vector3 oldCueBallPosition;
        private float oldForce;
        private Vector3 oldForward;
        private bool from2D;
        private bool isSimpleControl = true;
        public bool inShot
        {
            get;
            private set;
        }
        public bool inMove
        {
            get;
            private set;
        }
        public bool activateAfterCalculateAI
        {
            get;
            private set;
        }
        private bool stretchCue = false;
       
        [SerializeField] private Slider forceSlider;
        [SerializeField] private Text haveReplayText;
        [SerializeField] private Text waitingOpponent;
        private bool shotFromAI = false;
        public Image shotBack;
        private MouseState mouseState;
        private bool useAI;

        public bool ballInHand
        {
            get;
            private set;
        }

        public event System.Action OnEndCalculateAI;
        public event System.Action OnSelectBall;
        public event System.Action OnUnselectBall;

        private float cuePivotLocalRotationY;
        private float cueVerticalLocalRotationX;
        private Vector2 cueDisplacementLocalPositionXY;
        private float cueSliderLocalPositionZ;
        private Vector3 ballPosition;
        private Vector3 chackPosition;
        private Vector3 smoothBallPosition;
        private Impulse impulseFromNetwork;
        public bool canUpdateBallFromNetwork
        {
            get;
            set;
        }

        public bool cueChanged
        {
            get
            {
                if (cuePivotLocalRotationY != cuePivot.localRotation.eulerAngles.y || cueVerticalLocalRotationX != cueVertical.localRotation.eulerAngles.x ||
                    cueDisplacementLocalPositionXY != new Vector2(cueDisplacement.localPosition.x, cueDisplacement.localPosition.y) || cueSliderLocalPositionZ != cueSlider.localPosition.z)
                {
                    cuePivotLocalRotationY = cuePivot.localRotation.eulerAngles.y;
                    cueVerticalLocalRotationX = cueVertical.localRotation.eulerAngles.x;
                    cueDisplacementLocalPositionXY = new Vector2(cueDisplacement.localPosition.x, cueDisplacement.localPosition.y);
                    cueSliderLocalPositionZ = cueSlider.localPosition.z;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool ballChanged
        {
            get
            {
                if (Vector3.Distance( chackPosition, cueBall.position) > 0.1f * cueBall.radius)
                {
                    chackPosition = cueBall.position;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool changed
        {
            get
            {
                if (!enabled)
                {
                    return false;
                }
                if (oldShotPoint != shotPoint || oldForce != force || oldForward != cueSlider.forward || oldCueBallPosition != cueBall.position)
                {
                    oldShotPoint = shotPoint;
                    oldForce = force;
                    oldForward = cueSlider.forward;
                    oldCueBallPosition = cueBall.position;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        void Awake()
        {
            if (!NetworkManager.initialized)
            {
                enabled = false;
                return;
            }
            targetBallSavedDirection = Vector3.zero;

            useAI = false;
            if (ProductLines.lineLength != 0.0f)
            {
                lineLength = ProductLines.lineLength;
            }
            activateAfterCalculateAI = false;
            cueBallRadius = 0.5f * cueBall.transform.lossyScale.x;
            clothLayer = 1 << LayerMask.NameToLayer("Cloth");
            boardLayer = 1 << LayerMask.NameToLayer("Board");
            ballLayer = 1 << LayerMask.NameToLayer("Ball");
            cueBallLayer = 1 << LayerMask.NameToLayer("CueBall");

            cueVerticalStartRotation = cueVertical.localRotation;
            waitingOpponent.enabled = false;
            canUpdateBallFromNetwork = false;
            ballPosition = cueBall.position;
            smoothBallPosition = cueBall.position;
            cuePivotLocalRotationY = cuePivot.localRotation.eulerAngles.y;
            cueVerticalLocalRotationX = cueVertical.localRotation.eulerAngles.x;
            cueDisplacementLocalPositionXY = new Vector2(cueDisplacement.localPosition.x, cueDisplacement.localPosition.y);
            cueSliderLocalPositionZ = cueSlider.localPosition.z;


            inShot = false;
            inMove = false;
            mouseState = MouseState.Down;

            if (cueControlType == CueControlType.FirstPerson)
            {
                cueCamera.transform.position = cameraStandartPosition.position;
                cueCamera.transform.rotation = cameraStandartPosition.rotation;
            }
            else if (cueControlType == CueControlType.ThirdPerson)
            {
                cueCamera.transform.parent = tableCameraCenter;
                cueCamera.transform.position = cameraThirdPersonPosition.position;
                cueCamera.transform.LookAt(tableCameraCenter.position);
            }

            uiController.OnShot += (bool follow) =>
                {
                    activateAfterCalculateAI = false;
                    if(follow)
                    {
                        inShot = true;
                        inMove = true;
                        shotFromAI = false;
                        StartCoroutine("WaitAndStartShot");
                    }
                    else if ((enabled || BallPoolGameLogic.playMode == PlayMode.PlayerAI) && !physicsManager.inMove)
                    {
                        force = Mathf.Clamp01(-cueSliderDisplacementZ / cueSlidingMaxDisplacement);
                        cueStateType = CueStateType.Non;
                        if (!inShot && force > 0.03f && (!uiController.shotOnUp || shotFromAI))
                        {
                            inShot = true;
                            inMove = true;
                            shotFromAI = false;
                            StartCoroutine("WaitAndStartShot");
                        }
                    }
                };
            
            if (BallPoolGameLogic.playMode == PlayMode.Replay)
            {
                physicsManager.OnStartShot += Replay_PhysicsManager_OnStartShot;
                physicsManager.OnEndShot += Replay_PhysicsManager_OnEndShot;
                hand.gameObject.SetActive(false);
                enabled = false;
                int replayDataCount = physicsManager.replayManager.GetReplayDataCount();
                if (replayDataCount == 0)
                {
                    shotBack.enabled = true;
                    haveReplayText.enabled = true;
                }
                else
                {
                    shotBack.enabled = false;
                }
                ballChecker.gameObject.SetActive(false);
                if (!AightBallPoolNetworkGameAdapter.is3DGraphics)
                {
                    cuePivot.GetComponentInChildren<MeshRenderer>().enabled = false;
                }
                return;
            }



            ballInHand = false;
            RessetChanged();
            from2D = false;


            savedCueSliderLocalPosition = cueSlider.localPosition;
            physicsManager.OnSetState += PhysicsManager_OnSetState;
            physicsManager.OnStartShot += PhysicsManager_OnStartShot;
            physicsManager.OnBallExitFromPocket += PhysicsManager_OnBallExitFromPocket;

            aiManager.OnStartCalculateAI += AIManager_OnStartCalculateAI;
            aiManager.OnEndCalculateAI += AIManager_OnEndCalculateAI;

            cueHitBall = gameObject.AddComponent<AudioSource>();
            cueHitBall.playOnAwake = false;

            BallPoolGameManager.instance.OnShotEnded += BallPoolGameManager_instance_OnShotEnded;

            if (BallPoolGameLogic.isOnLine)
            {
                physicsManager.OnSaveEndStartReplay += PhysicsManager_OnSaveEndStartReplay;
            }
        }

        void PhysicsManager_OnSaveEndStartReplay (string impulse)
        {
            if (BallPoolGameLogic.controlFromNetwork)
            {
                impulseFromNetwork = DataManager.ImpulseFromString(impulse);
                Debug.Log("PhysicsManager_OnSaveEndStartReplay");
                StartCoroutine("WaitAndStartShot");
            }
        }

        void Replay_PhysicsManager_OnStartSave ()
        {
            shotBack.enabled = true;
        }
        void Replay_PhysicsManager_OnStartShot (string data)
        {
            shotBack.enabled = true;
        }
  
        void Replay_PhysicsManager_OnEndShot (string data)
        {
            int replayNumber = uiController.replayNumberValue;
            int replayDataCount = physicsManager.replayManager.GetReplayDataCount();
            if (replayDataCount != 1 && replayNumber < replayDataCount - 1)
            {
                replayNumber++;
            }
            else
            {
                replayNumber = 0;
            }
            Debug.LogWarning("replayNumber " + replayNumber + "  " + replayDataCount);
            uiController.SetReplayNumber(replayNumber, replayDataCount == 1);
            shotBack.enabled = false;
        }

        void Start()
        {
            StartCoroutine(SetControl());
        }

        void OnEnable()
        {
            InputOutput.OnMouseState += InputOutput_OnMouseState;
        }

        public void OnJumpToggle(Toggle value)
        {
            jumpFactor = value.isOn ? 1.0f : 0.0f;
        }
        public void OnEnableControl(bool value)
        {
            forceSlider.value = 0.0f;
            force = 0.0f;
            forceSlider.enabled = value;

            if (!value)
            {
                hand.gameObject.SetActive(false);
            }
            enabled = value;
            if (ballInHand)
            {
                UnselectBall();
            }
            if (cueStateType == CueStateType.TargetingAtCueBall)
            {
                RessetFromTargetingAtCueBall();
            }
            StartCoroutine(SetControl());
            if (BallPoolGameLogic.isOnLine && AightBallPoolNetworkGameAdapter.isSameGraphicsMode)
            {
                if (AightBallPoolNetworkGameAdapter.is3DGraphics)
                {
                    StartCoroutine(load3DCue.SetCue2DTextureOnChangeTurn(value));
                }
                else
                {
                    StartCoroutine(load2DCue.SetCue2DTextureOnChangeTurn(value));
                }
            }
        }

        private bool _opponenIsReadToPlay = false; 
        public bool opponenIsReadToPlay{ get { return _opponenIsReadToPlay; } }
            
        public void OpponenIsReadToPlay()
        {
            _opponenIsReadToPlay = true;
            if (AightBallPoolNetworkGameAdapter.isSameGraphicsMode)
            {
                //int number = (AightBallPoolPlayer.mainPlayer.coins == AightBallPoolPlayer.otherPlayer.coins) ? 0 : (AightBallPoolPlayer.mainPlayer.coins > AightBallPoolPlayer.otherPlayer.coins ? 1 : 2);

                if (AightBallPoolNetworkGameAdapter.is3DGraphics)
                {
                    load3DCue.OnStart();
                    load3DTable.OnStart();
                    //StartCoroutine(load3DTable.SetTable3DTextureOnStartGame(number));
                }
                else
                {
                    load2DCue.OnStart();
                    load2DTable.OnStart();
                    //StartCoroutine(load2DTable.SetTable2DTextureOnStartGame(number));
                }
            }
        }
        public void SetOpponentCueURL(string url)
        {
            if (AightBallPoolNetworkGameAdapter.is3DGraphics)
            {
                StartCoroutine(load3DCue.SetOpponentCueURL(url));
            }
            else
            {
                StartCoroutine(load2DCue.SetOpponentCueURL(url));
            }
        }
        public void SetOpponentTableURLs(string boardURL, string clothURL, string clothColor)
        {
            if (AightBallPoolNetworkGameAdapter.is3DGraphics)
            {
                StartCoroutine(load3DTable.SetOpponentTableURLs(boardURL, clothURL, clothColor));
            }
            else
            {
                StartCoroutine(load2DTable.SetOpponentTableURLs(boardURL, clothURL, clothColor));
            }
        }
        private IEnumerator SetControl()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (BallPoolGameLogic.isOnLine)
            {
                float waitingTime = 0.0f;
                while (!NetworkManager.network.opponenWaitingForYourTurn)
                {
                    yield return new WaitForEndOfFrame();
                    waitingTime += Time.deltaTime;
                    if (waitingTime > 2.0f)
                    {
                        waitingOpponent.enabled = true;
                    }
                }
                waitingOpponent.enabled = false;
            }
            if (!enabled && !BallPoolPlayer.mainPlayer.myTurn && BallPoolGameLogic.playMode != PlayMode.HotSeat && BallPoolGameLogic.playMode != PlayMode.Replay)
            {
                shotBack.enabled = true;
            }
            else
            {
                shotBack.enabled = false;
            }
        }
        void OnDisable()
        {
            InputOutput.OnMouseState -= InputOutput_OnMouseState;
        }

        void OnDestroy()
        {
            physicsManager.OnStartShot -= PhysicsManager_OnStartShot;
            physicsManager.OnBallExitFromPocket -= PhysicsManager_OnBallExitFromPocket;

            physicsManager.OnStartShot -= Replay_PhysicsManager_OnStartShot;
            physicsManager.OnEndShot -= Replay_PhysicsManager_OnEndShot;

            aiManager.OnStartCalculateAI -= AIManager_OnStartCalculateAI;
            aiManager.OnEndCalculateAI -= AIManager_OnEndCalculateAI;
        }
        public void OnEndTime()
        {
            Debug.Log("OnEndTime");
            activateAfterCalculateAI = false;
            cueVertical.parent = cuePivot;
            cueVertical.localPosition = Vector3.zero;
            cueVertical.localRotation = cueVerticalStartRotation;
            cueDisplacement.localPosition = Vector3.zero;
            ballChecker.gameObject.SetActive(true);
            targeting2DManager.Resset();
        }
        public void UndoShot()
        {
            Debug.Log("UndoShot");
            StopCoroutine("WaitAndStartShot");
            cueSliderDisplacementZ = 0.0f;
            cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
            savedCueSliderLocalPosition = cueSlider.localPosition;
            force = Mathf.Clamp01(-cueSliderDisplacementZ / cueSlidingMaxDisplacement);
            physicsManager.HideBallsLine();
            aiManager.CancelCalculateAI();
        }

        void RessetChanged()
        {
            oldShotPoint = shotPoint;
            oldForce = force;
            oldForward = cueSlider.forward;
            oldCueBallPosition = cueBall.position;
        }
        private int shotNumber = 0;
        public void DecreaseAICount()
        {
            if (useAI && BallPoolPlayer.mainPlayer.myTurn && BallPoolGameLogic.playMode == PlayMode.OnLine)
            {
                Debug.LogWarning("DecreaseAICount");
                ProductAI.aiCount--;
                gameManager.UpdateAICount();
            }
            useAI = false;
        }
        public IEnumerator WaitAndStartShot()
        {
            if (BallPoolPlayer.mainPlayer.myTurn && BallPoolGameLogic.playMode != PlayMode.HotSeat)
            {
                ProductLines.OnShot(ref lineLength);
            }
            DecreaseAICount();
           
            physicsManager.moveTime = 0.0f;
            physicsManager.endFromNetwork = false;
            shotNumber++;
            hand.gameObject.SetActive(false);
            inShot = true;
            inMove = true;
            shotBack.enabled = true;

            float checkTime = 0.0f;
            if (!BallPoolGameLogic.controlFromNetwork)
            {
                while (checkTime < 1.0f && cueSlider.localPosition.z < 0.0f)
                {
                    checkTime += Time.fixedDeltaTime;
                    cueSlider.localPosition += Vector3.forward * force;
                    yield return new WaitForFixedUpdate();
                }
                cueSlider.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }

            yield return new WaitForSeconds(3.0f * Time.fixedDeltaTime);
            Impulse impulse = new Impulse();
            if (BallPoolGameLogic.playMode == PlayMode.Replay)
            {
                impulse =  physicsManager.replayManager.GetImpulse(uiController.replayNumberValue);
                Debug.Log("set impulse " + impulse.impulse);
                physicsManager.SetImpulse(impulse);
            }
            else
            {
                if (BallPoolGameLogic.controlFromNetwork)
                {
                    impulse = impulseFromNetwork;
                    yield return new WaitForSeconds(1.0f);
                    while (checkTime < 1.0f && cueSlider.localPosition.z < 0.0f)
                    {
                        checkTime += Time.fixedDeltaTime;
                        cueSlider.localPosition += Vector3.forward * force;
                        yield return new WaitForFixedUpdate();
                    }
                    cueSlider.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                }
                else
                {
                    Vector3 forceVector = force * (maxVelocity * physicsManager.ballMass) * ((Vector3.ProjectOnPlane(cueSlider.forward, Vector3.up) + 0.5f * Vector3.Project(cueSlider.forward, Vector3.up)).normalized);
                    impulse = new Impulse(shotPoint, forceVector);
                }
                physicsManager.SetImpulse(impulse);
                physicsManager.replayManager.SaveImpulse(impulse);
            }

            if (BallPoolGameLogic.controlInNetwork)
            {
                yield return null;
                physicsManager.StartRaplayShot(DataManager.ImpulseToString(impulse));
            }
            physicsManager.StartShot(cueBall.listener);

            if (BallPoolGameLogic.playMode == PlayMode.Replay)
            {
                foreach (var ball in gameManager.balls) 
                {
                    ball.SrartFollow();
                }
            }

            forceSlider.value = 0.0f;
            hand.gameObject.SetActive(false);
            inShot = false;

            yield return new WaitForSeconds(0.7f);
            if (physicsManager.inMove)
            {
                cueVertical.parent = cuePivotAfterShotPosition;
                cueVertical.localPosition = Vector3.zero;
                cueVertical.localRotation = Quaternion.identity;
                cueDisplacement.localPosition = Vector3.zero;
                targeting2DManager.Resset();
            }
        }

        void PhysicsManager_OnBallExitFromPocket(BallListener listener, PocketListener pocket, BallExitType exitType, bool inSimulate)
        {
            if (listener == cueBall.listener)
            {
                cueBall.isActive = false;
            }
        }

        void AIManager_OnEndCalculateAI(BallPoolAIManager aiManager)
        {
            if (aiManager.haveExaption)
            {
                Debug.LogWarning("haveExaption");
            }

            cueBall.position = aiManager.info.shotBallPosition;
            cueBall.OnState(BallState.SetState);

            ballChecker.position = aiManager.info.aimpoint;
            ballChecker.gameObject.SetActive(true);
            shotPoint = aiManager.info.shotPoint;
            Vector3 impulseVector = aiManager.info.impulse * (aiManager.info.aimpoint - aiManager.info.shotBallPosition).normalized;
            cuePivot.position = aiManager.info.shotBallPosition;
            cuePivot.LookAt( cuePivot.position + Vector3.ProjectOnPlane(impulseVector.normalized, Vector3.up ));
            cueSlider.forward = impulseVector.normalized;
            cueDisplacement.position = shotPoint;
            float displacement = (aiManager.info.impulse / physicsManager.ballMaxVelocity) * cueSlidingMaxDisplacement;
            cueSliderDisplacementZ = -displacement;
           
            cueSlider.localPosition = new Vector3(0.0f, 0.0f, -displacement);
            RessetChanged();
            TryCalculateShot(true);
            Impulse impulse = new Impulse(shotPoint, force * (maxVelocity * cueBall.listener.body.mass) * cueSlider.forward);
            physicsManager.SetImpulse(impulse);

            if (BallPoolGameLogic.controlInNetwork)
            {
                NetworkManager.network.SendRemoteMessage("SetBallPosition", cueBall.position);
                NetworkManager.network.SendRemoteMessage("OnForceSendCueControl", cuePivot.localRotation.eulerAngles.y, cueVertical.localRotation.eulerAngles.x, 
                    new Vector2(cueDisplacement.localPosition.x, cueDisplacement.localPosition.y), cueSlider.localPosition.z, force);

            }
            if (OnEndCalculateAI != null)
            {
                OnEndCalculateAI();
            }
            if (BallPoolGameLogic.playMode != PlayMode.PlayerAI || BallPoolPlayer.mainPlayer.myTurn)
            {
                if (uiController.shotOnUp)
                {
                    Debug.Log("StartShot ");
                    inMove = true;
                    enabled = false;
                    shotBack.enabled = true;
                    inShot = true;
                    shotFromAI = false;
                    StartCoroutine("WaitAndStartShot");
                }
                else
                {
                    Debug.Log("Activate ");
                    enabled = true;
                    shotBack.enabled = false;
                    activateAfterCalculateAI = true;
                }
            }
        }
       
        void AIManager_OnStartCalculateAI(BallPoolAIManager AIManager)
        {
            useAI = true;
            shotFromAI = true;
            enabled = false;
            activateAfterCalculateAI = false;
            cueVertical.localPosition = Vector3.zero;
            cueVertical.localRotation = Quaternion.identity;
            ballChecker.gameObject.SetActive(false);
        }

        void PhysicsManager_OnSetState()
        {
            cuePivot.position = cueBall.position;
            Vector3 impulse = cueBall.impulse.impulse.normalized;
            cuePivot.LookAt(cuePivot.position + Vector3.ProjectOnPlane(impulse, Vector3.up));
            cueSlider.forward = cueBall.impulse.impulse.normalized;
            cueDisplacement.position = cueBall.impulse.point;
            TryCalculateShot(true);
        }

        void PhysicsManager_OnStartShot(string data)
        {
            cueHitBall.volume = force;
            cueHitBall.PlayOneShot(hitBallClip);
            enabled = false;
            ballChecker.gameObject.SetActive(false);
            HideAllLines();
        }


        void BallPoolGameManager_instance_OnShotEnded ()
        {
            inMove = false;
            activateAfterCalculateAI = false;
            ballPosition = cueBall.position;
            smoothBallPosition = ballPosition;
            enabled = BallPoolPlayer.mainPlayer.myTurn || BallPoolGameLogic.playMode == PlayMode.HotSeat;       
            if (cueBall.firstHitInfo.shapeType != ShapeType.Non)
            {
                ballChecker.position = cueBall.firstHitInfo.positionInHit;
            }
            ballChecker.gameObject.SetActive(true);
            StartCoroutine(SetControl());

            foreach (var ball in BallPoolGameManager.instance.balls)
            {
                if (!ball.inSpace && !ball.inPocket)
                {
                    bool canReactivate = false;
                    Vector3 ballNewPosition = Vector3.zero;
                    physicsManager.ReactivateBallInCube(ball.radius, clothSpace, clothLayer, ballLayer | cueBallLayer, ref canReactivate, ref ballNewPosition);
                    if (canReactivate)
                    {
                        ball.position = ballNewPosition;
                        ball.isActive = false;
                        ball.inPocket = false;
                        ball.pocketId = -1;
                        ball.hitShapeId = -2;
                        ball.OnState(BallState.ExitFromPocket);
                    }
                }
            }
            if (AightBallPoolGameLogic.gameState.cueBallInPocket)
            {
                bool canReactivate = false;
                Vector3 ballNewPosition = Vector3.zero;
                physicsManager.ReactivateBallInCube(cueBall.radius, clothSpace, clothLayer, ballLayer | cueBallLayer, ref canReactivate, ref ballNewPosition);
                if (canReactivate)
                {
                    physicsManager.CallOnBallExitFromPocket(cueBall.listener, cueBall.listener.pocket, true);
                    Debug.LogWarning("Nuul");
                    cueBall.listener.pocket = null;
                    cueBall.position = ballNewPosition;
                    cueBall.isActive = false;
                    cueBall.inPocket = false;
                    cueBall.pocketId = -1;
                    cueBall.hitShapeId = -2;
                    cueBall.OnState(BallState.ExitFromPocket);
                }
            }
       
            cuePivot.position = cueBall.position;
            cueDisplacement.localPosition = Vector3.zero;
            targeting2DManager.Resset();
            cueSlider.localPosition = Vector3.zero;
            cueSliderDisplacementZ = 0.0f;
            forceSlider.value = 0.0f;

            cueVertical.parent = cuePivot;
            cueVertical.localPosition = Vector3.zero;
            cueVertical.localRotation = cueVerticalStartRotation;

            TryCalculateShot(true);
            if (BallPoolGameLogic.isOnLine)
            {
                NetworkManager.network.SendRemoteMessage("OnOpponenWaitingForYourTurn");
            }
        }

        void HideAllLines()
        {
            cueBallSimpleLine.positionCount = 0;
            targetBallSimpleLine.positionCount = 0;
            physicsManager.HideBallsLine();
        }

        void Update ()
        {
            if (InputOutput.isMobilePlatform)
            {
                CheckActivateHand();
            }
        }
        void InputOutput_OnMouseState(MouseState mouseState)
        {
            if (!InputOutput.inUsedCameraScreen && mouseState != MouseState.Up)
            {
                return;
            }
            if (!canControl)
            {
                return;
            }
            if (shotBack.enabled)
            {
                return;
            }
            this.mouseState = mouseState;
      
            if (!InputOutput.isMobilePlatform)
            {
                CheckActivateHand();
            }
            if (!(mouseState == MouseState.Down || mouseState == MouseState.Up || mouseState == MouseState.Press || mouseState == MouseState.Move))
            {
                return;
            }
            if (stretchCue && InputOutput.isMobilePlatform && !inShot && force > 0.03f && uiController.shotOnUp && mouseState == MouseState.Up)
            {
                inShot = true;
                inMove = true;
                physicsManager.SetImpulse(new Impulse(shotPoint, force * (maxVelocity * physicsManager.ballMass) * cueSlider.forward));
                StartCoroutine("WaitAndStartShot");
            }
            if (!enabled || (!InputOutput.inUsedCameraScreen  && mouseState != MouseState.Up) )
            {
                return;
            }
            if (mouseState == MouseState.Up || mouseState == MouseState.Down)
            {
                stretchCue = false;
            }
            if (stretchCue)
            {
                return;
            }
            if (AightBallPoolGameLogic.gameState.cueBallInHand)
            {
                if (!ballInHand && mouseState == MouseState.Down)
                {
                    TryingToSelectBall();
                }
                if (ballInHand)
                {
                    if (mouseState == MouseState.Press)
                    {
                        TryingToMoveBall();
                    }
                    else if (mouseState == MouseState.Up)
                    {
                        UnselectBall();
                    }
                    return;
                }
            }
            if (!inShot)
            {
                if (uiController.is3D)
                {
                    ControlIn3D(mouseState);
                }
                else
                {
                    ControlIn2D(mouseState);
                }
                force = Mathf.Clamp01(-cueSliderDisplacementZ / cueSlidingMaxDisplacement);
                TryCalculateShot(false);

            }
            if (!InputOutput.isMobilePlatform && !inShot && force > 0.03f && uiController.shotOnUp && mouseState == MouseState.Up)
            {
                if (cueStateType == CueStateType.Non || cueStateType == CueStateType.TargetingAtTargetBall || cueStateType == CueStateType.StretchCue)
                {
                    inShot = true;
                    inMove = true;
                    physicsManager.SetImpulse(new Impulse(shotPoint, force * (maxVelocity * physicsManager.ballMass) * cueSlider.forward));
                    StartCoroutine("WaitAndStartShot");
                }
            }
        }
        public void UpdateFromNetwork()
        {
            cuePivot.localRotation = Quaternion.Lerp(cuePivot.localRotation, Quaternion.Euler(0.0f, cuePivotLocalRotationY, 0.0f), 5.0f * Time.deltaTime);
            cueVertical.localRotation = Quaternion.Lerp(cueVertical.localRotation, Quaternion.Euler(cueVerticalLocalRotationX, 0.0f, 0.0f), 5.0f * Time.deltaTime);
            cueDisplacement.localPosition = Vector3.Lerp(cueDisplacement.localPosition, new Vector3(cueDisplacementLocalPositionXY.x, cueDisplacementLocalPositionXY.y, 0.0f), 5.0f * Time.deltaTime);
            targeting2DManager.SetPointTargetingPosition(-cueDisplacement.localPosition/ cueBallRadius);
            cueSlider.localPosition = Vector3.Lerp(cueSlider.localPosition, new Vector3(0.0f, 0.0f, cueSliderLocalPositionZ), 5.0f * Time.deltaTime);
            if (canUpdateBallFromNetwork)
            {
                smoothBallPosition = Vector3.Lerp(smoothBallPosition, ballPosition, 5.0f * Time.deltaTime);
                cueBall.position = smoothBallPosition;
                cueBall.OnState(BallState.SetState);
            }


            if (Mathf.Abs(cuePivot.localRotation.eulerAngles.y - cuePivotLocalRotationY) > 0.1f)
            {
                TryCalculateShot(true);
            }
            if (Mathf.Abs(cuePivot.localRotation.eulerAngles.y - cuePivotLocalRotationY) < 0.1f && cuePivot.localRotation.eulerAngles.y != cuePivotLocalRotationY)
            {
                cuePivot.localRotation = Quaternion.Euler(0.0f, cuePivotLocalRotationY, 0.0f);
                TryCalculateShot(true);
            }
        }

        public void SelectBallPositionFromNetwork(Vector3 ballSelectPosition)
        {
            canUpdateBallFromNetwork = true;
            ballPosition = ballSelectPosition;
            smoothBallPosition = ballSelectPosition;
            cueBall.position = ballSelectPosition;
            cueBall.OnState(BallState.SetState);
        }
        public void SetBallPositionFromNetwork(Vector3 ballNewPosition)
        {
            Debug.LogWarning("SetBallPositionFromNetwork");
            canUpdateBallFromNetwork = false;
            smoothBallPosition = ballNewPosition;
            ballPosition = ballNewPosition;
            cuePivot.position = ballNewPosition;
            cueBall.position = ballNewPosition;
            cueBall.OnState(BallState.SetState);
            TryCalculateShot(true);
        }
        public void MoveBallFromNetwork(Vector3 ballPosition)
        {
            Debug.LogWarning("MoveBallFromNetwork " + DataManager.Vector3ToString(ballPosition));
            this.ballPosition = ballPosition;
        }
        public void CueControlFromNetwork(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
        {
            this.cuePivotLocalRotationY = cuePivotLocalRotationY;
            this.cueVerticalLocalRotationX = cueVerticalLocalRotationX;
            this.cueDisplacementLocalPositionXY = cueDisplacementLocalPositionXY;
            this.cueSliderLocalPositionZ = cueSliderLocalPositionZ;
            this.force = force;
            this.cueSliderDisplacementZ = Mathf.Lerp(0, -cueSlidingMaxDisplacement, force);
        }
        public void ForceCueControlFromNetwork(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
        {
            CueControlFromNetwork(cuePivotLocalRotationY, cueVerticalLocalRotationX, cueDisplacementLocalPositionXY, cueSliderLocalPositionZ, force);
            cuePivot.localRotation = Quaternion.Euler(0.0f, cuePivotLocalRotationY, 0.0f);
            cueVertical.localRotation = Quaternion.Euler(cueVerticalLocalRotationX, 0.0f, 0.0f);
            cueDisplacement.localPosition = new Vector3(cueDisplacementLocalPositionXY.x, cueDisplacementLocalPositionXY.y, 0.0f);
            targeting2DManager.SetPointTargetingPosition(-cueDisplacement.localPosition/ cueBallRadius);
            cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderLocalPositionZ);

            smoothBallPosition = Vector3.Lerp(smoothBallPosition, ballPosition, 5.0f * Time.deltaTime);
            cueBall.position = smoothBallPosition;
            cueBall.OnState(BallState.SetState);

            if (Mathf.Abs(cuePivot.localRotation.eulerAngles.y - cuePivotLocalRotationY) > 0.1f)
            {
                TryCalculateShot(true);
            }
            if (Mathf.Abs(cuePivot.localRotation.eulerAngles.y - cuePivotLocalRotationY) < 0.1f && cuePivot.localRotation.eulerAngles.y != cuePivotLocalRotationY)
            {
                cuePivot.localRotation = Quaternion.Euler(0.0f, cuePivotLocalRotationY, 0.0f);
                TryCalculateShot(true);
            }
        }
        private float cueBallCheckBallDistance;

        void TryCalculateShot(bool forceCalculate)
        {
            shotPoint = cueDisplacement.position;
            force = Mathf.Clamp01(-cueSliderDisplacementZ / cueSlidingMaxDisplacement);
            if ((forceCalculate || changed))
            {
                if (force > 0.03f)
                {
                    physicsManager.SetImpulse(new Impulse(shotPoint, force * (maxVelocity * physicsManager.ballMass) * cueSlider.forward));
                }
                physicsManager.HideBallsLine();
                Vector3 origin = cueBall.position;
                Vector3 direction = Vector3.ProjectOnPlane(cuePivot.forward, Vector3.up).normalized;
                RaycastHit targetShapelHit;
                Color ballCheckerColor = Color.white;

                targetBallSavedDirection = Vector3.zero;
                tragetBallListener = null;

                if (Physics.SphereCast(origin, cueBallRadius, direction, out targetShapelHit,  1.2f * clothSpace.lossyScale.x, ballLayer | boardLayer))
                {
                    BallListener listener = targetShapelHit.collider.gameObject.GetComponent<BallListener>();
                    if (listener)
                    {
                        Vector3 positionInHit = targetShapelHit.point + cueBallRadius * targetShapelHit.normal;
                        ballChecker.position = positionInHit;

                        if (aiManager.FindException(listener.id))
                        {
                            ballCheckerColor = Color.red;
                        }

                        cueBallSimpleLine.positionCount = 3;
                        cueBallSimpleLine.SetPosition(0, cueBall.position);
                        cueBallSimpleLine.SetPosition(1, ballChecker.position);
                        Vector3 secontLineDirection = Vector3.Cross(Vector3.up, Vector3.ProjectOnPlane(targetShapelHit.normal , Vector3.up).normalized);
                        float tangent = Vector3.Dot(Vector3.ProjectOnPlane( cuePivot.forward , Vector3.up), secontLineDirection);
                        if (tangent < 0.0f)
                        {
                            secontLineDirection = -secontLineDirection;
                        }

                        cueBallSimpleLine.SetPosition(2, (positionInHit + Mathf.Clamp(Mathf.Abs(tangent), 0.2f, 1.0f) * lineLength * secontLineDirection));

                        targetBallSimpleLine.positionCount = 2;
                        targetBallSimpleLine.SetPosition(0, listener.body.position);
                        targetBallSimpleLine.SetPosition(1, (listener.body.position + Mathf.Clamp(1.0f - Mathf.Abs(tangent), 0.2f, 1.0f) * lineLength * (listener.body.position - positionInHit).normalized));

                        targetBallSavedDirection = (targetBallSimpleLine.GetPosition(1) - targetBallSimpleLine.GetPosition(0)).normalized;
                        tragetBallListener = listener;
                    }
                    else
                    {
                        Vector3 positionInHit = targetShapelHit.point + cueBallRadius * targetShapelHit.normal;
                        ballChecker.position = positionInHit;

                        cueBallSimpleLine.positionCount = 3;
                        cueBallSimpleLine.SetPosition(0, cueBall.position);
                        cueBallSimpleLine.SetPosition(1, ballChecker.position);
                        Vector3 projectOnNormal = Vector3.Project(Vector3.ProjectOnPlane( cuePivot.forward, Vector3.up), Vector3.ProjectOnPlane(targetShapelHit.normal , Vector3.up).normalized);
                        Vector3 reactionDirection = Vector3.ProjectOnPlane( cuePivot.forward, Vector3.up) - 2.0f * projectOnNormal;
                        cueBallSimpleLine.SetPosition(2, positionInHit + lineLength * reactionDirection);

                        targetBallSimpleLine.positionCount = 0;
                    }
                    cueBallCheckBallDistance = Vector3.Distance(cueBall.position, ballChecker.position);
                }
                else
                {
                    ballChecker.position = cueBall.position + cueBallCheckBallDistance * Vector3.ProjectOnPlane( cuePivot.forward, Vector3.up);
                    cueBallSimpleLine.positionCount = 3;
                    cueBallSimpleLine.SetPosition(0, cueBall.position);
                    cueBallSimpleLine.SetPosition(1, ballChecker.position);

                    targetBallSimpleLine.positionCount = 0;
                    ballCheckerColor = Color.red;
                }

                ballCheckerColor.a = 0.3f;
                ballCheckerRenderer.sharedMaterial.color = ballCheckerColor;
                physicsManager.SetImpulse(new Impulse(shotPoint, force * (maxVelocity * physicsManager.ballMass) * cueSlider.forward));
            }
        }

        void UnselectBall()
        {
            ballInHand = false;
            cueBall.position = ballPosition;
            cueBall.OnState(BallState.SetState);
            cuePivot.position = cueBall.position;
            TryCalculateShot(true);
            if (OnUnselectBall != null)
            {
                OnUnselectBall();
            }
        }
        void TryingToSelectBall()
        {
            ballInHand = false;
            Vector3 handScreenPosition = InputOutput.WorldToScreenPoint(hand.position);
            float handRadius = InputOutput.WorldToScreenRadius(0.5f * hand.lossyScale.x, hand);
            float handDistance = Vector3.Distance(handScreenPosition, InputOutput.mouseScreenPosition);
            if (handDistance < handRadius)
            {
                ballInHand = true;
                if (OnSelectBall != null)
                {
                    OnSelectBall();
                }
            }
            else if(cueControlType == CueControlType.ThirdPerson || !AightBallPoolNetworkGameAdapter.is3DGraphics)
            {
                Vector3 centerPointInSceen = InputOutput.WorldToScreenPoint(cuePivot.position);
                float cueBallRadiusInScreen = InputOutput.WorldToScreenRadius(cueBallRadius, cueBall.transform);
                Vector3 mouseScreenPosition = InputOutput.mouseScreenPosition;

                if (Vector3.Distance(centerPointInSceen, mouseScreenPosition) <= 5.0f * cueBallRadiusInScreen)
                {
                    ballInHand = true;
                    if (OnSelectBall != null)
                    {
                        OnSelectBall();
                    }
                }
            }
        }

        void CheckActivateHand()
        {
            Vector3 handScreenPosition = InputOutput.WorldToScreenPoint(hand.position);
            float handRadius = InputOutput.WorldToScreenRadius(0.5f * hand.lossyScale.x, hand);
            float handDistance = Vector3.Distance(handScreenPosition, InputOutput.mouseScreenPosition);
            //Debug.LogWarning("AightBallPoolGameLogic.gameState.cueBallInHand " + AightBallPoolGameLogic.gameState.cueBallInHand);
            if (shotBack.enabled || (InputOutput.isMobilePlatform && mouseState != MouseState.Up) || (!InputOutput.isMobilePlatform && (mouseState == MouseState.Press || mouseState == MouseState.PressAndMove || mouseState == MouseState.PressAndStay) ))
            {
                hand.gameObject.SetActive(false);
            }
            else if (cueStateType != CueStateType.StretchCue && cueStateType != CueStateType.TargetingAtCueBall && AightBallPoolGameLogic.gameState.cueBallInHand && !ballInHand && (handDistance < 3.0f * handRadius || InputOutput.isMobilePlatform) && !hand.gameObject.activeInHierarchy)
            {
                hand.gameObject.SetActive(true);
            }
            else if (!AightBallPoolGameLogic.gameState.cueBallInHand || ballInHand || (handDistance > 3.5f * handRadius && hand.gameObject.activeInHierarchy && !InputOutput.isMobilePlatform) || cueStateType == CueStateType.TargetingAtCueBall || cueStateType == CueStateType.StretchCue)
            {
                hand.gameObject.SetActive(false);
            }
            if (uiController.is3D || InputOutput.isMobilePlatform || !uiController.shotOnUp)
            {
                hand.position = cuePivot.position + 4.0f * cueBallRadius * cuePivot.right;
                hand.right = cuePivot.right;
            }
            else
            {
                hand.position = cuePivot.position - 4.0f * cueBallRadius * Vector3.forward;
                hand.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
        }

        void TryingToMoveBall()
        {
            RaycastHit clothHit;
            Vector3 origin = InputOutput.mouseWordRay.origin;
            Vector3 direction = InputOutput.mouseWordRay.direction;
            if (Physics.Raycast(origin, direction, out clothHit, 3.0f, clothLayer))
            {
                RaycastHit ballHitInfo;
                if (!Physics.SphereCast(origin, cueBallRadius, direction, out ballHitInfo, 3.0f, ballLayer))
                {
                    Vector3 ballNewPosition = clothHit.point + cueBallRadius * clothHit.normal;
                    Vector3 ballNewPositionInClothSpace = Geometry.ClampPositionInCube(ballNewPosition, cueBallRadius, AightBallPoolGameLogic.gameState.tableIsOpened ? firstMoveSpace : clothSpace);
                    cueBall.position = ballNewPositionInClothSpace;
                    ballPosition = ballNewPositionInClothSpace;
                    smoothBallPosition = ballPosition;
                    cueBall.OnState(BallState.SetState);
                }
            }
        }
            

        public void StretchCue()
        {
            if ((uiController.shotOnUp && !InputOutput.isMobilePlatform) || inShot)
            {
                return;
            }

            stretchCue = true;
            cueStateType = CueStateType.StretchCue;
            cueSliderDisplacementZ = Mathf.Lerp(0, -cueSlidingMaxDisplacement, forceSlider.value);
            cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
            savedCueSliderLocalPosition = cueSlider.localPosition;
            TryCalculateShot(true);
        }
        void RessetFromTargetingAtCueBall()
        {
            if (cueStateType == CueStateType.TargetingAtCueBall)
            {
                if (cueControlType == CueControlType.FirstPerson)
                {
                    cueCamera.transform.position = cameraStandartPosition.position;
                    cueCamera.transform.rotation = cameraStandartPosition.rotation;
                }
                cueSlider.localPosition = savedCueSliderLocalPosition;
                if (from2D)
                {
                    uiController.is3D = false;
                    uiController.CameraToggle();
                    from2D = false;
                }
                cueStateType = CueStateType.TargetingAtTargetBall;
            }
        }
        public void RessetCueAfterTargeting()
        {
            cueSlider.localPosition = savedCueSliderLocalPosition;
        }
        public void RessetCueForTargeting()
        {
            cueDisplacement.localPosition = new Vector3(cueDisplacement.localPosition.x, cueDisplacement.localPosition.y, 0.0f);
            savedCueSliderLocalPosition = cueSlider.localPosition;
            cueSlider.localPosition = Vector3.zero;
        }
        public void SetCueTargetingPosition(Vector3 normalizedPosition)
        {
            cueDisplacement.localPosition = cueDisplacementOnBall = normalizedPosition * cueBallRadius;
        }
        void ControlIn2D(MouseState mouseState)
        {
            if (cueControlType == CueControlType.FirstPerson && mouseState == MouseState.Down)
            {
                Vector3 centerPointInSceen = InputOutput.WorldToScreenPoint(cuePivot.position);
                float cueBallRadiusInScreen = InputOutput.WorldToScreenRadius(cueBallRadius, cueBall.transform);
                Vector3 mouseScreenPosition = InputOutput.mouseScreenPosition;

                if (AightBallPoolNetworkGameAdapter.is3DGraphics && Vector3.Distance(centerPointInSceen, mouseScreenPosition) <= 5.0f * cueBallRadiusInScreen)
                {
                    cueStateType = CueStateType.TargetingAtCueBall;
                    if (cueControlType == CueControlType.FirstPerson)
                    {
                        cueCamera.transform.position = cameraAimingPosition.position;
                        cueCamera.transform.rotation = cameraAimingPosition.rotation;
                    }
                    RessetCueForTargeting();
                    uiController.is3D = true;
                    uiController.CameraToggle();
                    from2D = true;
                }
                else
                {
                    cueStateType = CueStateType.Non;
                }
            }
            else if (mouseState == MouseState.Press || (uiController.shotOnUp && mouseState == MouseState.Move))
            {
                Vector3 cueScreenPivot0 = InputOutput.WorldToScreenPoint(cueSlider.position);
                Vector3 cueScreenPivot1 = InputOutput.WorldToScreenPoint(cueSlider.position - cueSlider.forward);
                Vector3 cueScreenDirection = -(cueScreenPivot1 - cueScreenPivot0).normalized;

                Vector3 mouseScreenSpeed = InputOutput.mouseScreenSpeed;
                float mouseScreenSpeedToCue = Vector3.Dot(mouseScreenSpeed, cueScreenDirection);
                float mouseScreenSpeedToCueDirection = Vector3.Dot(mouseScreenSpeed.normalized, cueScreenDirection);

                if (!isSimpleControl && Mathf.Abs(mouseScreenSpeedToCueDirection) > 0.9f)
                {
                    cueStateType = CueStateType.StretchCue;
                    cueSliderDisplacementZ += cueSlideSpeed * mouseScreenSpeedToCue * Time.deltaTime;
                    cueSliderDisplacementZ = Mathf.Clamp(cueSliderDisplacementZ, -cueSlidingMaxDisplacement, 0.0f);
                    cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
                    savedCueSliderLocalPosition = cueSlider.localPosition;
                }
                else
                {
                    cueStateType = CueStateType.TargetingAtTargetBall;
                    if (InputOutput.isMobilePlatform)
                    {
                        Vector3 cueBallScreenPoint = InputOutput.WorldToScreenPoint(cueBall.position);
                        float orientX = InputOutput.mouseScreenPosition.x > cueBallScreenPoint.x ? 1.0f : -1.0f;
                        float orientY = InputOutput.mouseScreenPosition.y > cueBallScreenPoint.y ? 1.0f : -1.0f;
                        float rSpeed2D = (orientY * InputOutput.mouseScreenSpeed.x - orientX * InputOutput.mouseScreenSpeed.y) * Time.deltaTime;

                        float mouseScreenSpeedValue = 100.0f * rotationSpeedCurve.Evaluate(rSpeed2D / 100.0f) * rotationSpeed2D;
                        cuePivot.Rotate(cuePivot.up, mouseScreenSpeedValue);
                    }
                    else
                    {
                        if (uiController.shotOnUp && mouseState == MouseState.Press)
                        {
                            cueStateType = CueStateType.StretchCue;
                            cueSliderDisplacementZ += cueSlideSpeed * mouseScreenSpeedToCue * Time.deltaTime;
                            cueSliderDisplacementZ = Mathf.Clamp(cueSliderDisplacementZ, -cueSlidingMaxDisplacement, 0.0f);
                            cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
                            savedCueSliderLocalPosition = cueSlider.localPosition;
                        }
                        else if(force < 0.03f || !uiController.shotOnUp)
                        {
                            Vector3 mouseWordDirectionFromCuePivot = Vector3.ProjectOnPlane(InputOutput.mouseWordPosition - cuePivot.position, Vector3.up).normalized;
                            Quaternion cuePivotRotation = Quaternion.identity;
                            cuePivotRotation.SetLookRotation(mouseWordDirectionFromCuePivot);
                            cuePivot.rotation = Quaternion.Lerp(cuePivot.rotation, cuePivotRotation, 10.0f * Time.deltaTime);
                        }
                    }
                }
            }
            else if (mouseState == MouseState.Up)
            {

            }
        }
        IEnumerator MoveCameraToStandartPosition()
        {
            while (Vector3.Distance(cueCamera.transform.position, cameraStandartPosition.position) > 0.1f)
            {
                cueCamera.transform.position = Vector3.Lerp(cueCamera.transform.position, cameraStandartPosition.position, 10.0f * Time.deltaTime);
                cueCamera.transform.rotation = Quaternion.Lerp(cueCamera.transform.rotation, cameraStandartPosition.rotation, 10.0f * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            cueCamera.transform.position = cameraStandartPosition.position;
            cueCamera.transform.rotation = cameraStandartPosition.rotation;
        }
        private void TargetingAtTargetBallInThirdPerson()
        {
            if (!InputOutput.isMobilePlatform)
            {
                Ray ray = cueCamera.ScreenPointToRay(InputOutput.mouseScreenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10.0f, cueThirdPersonControlMask))
                {
                    Vector3 targetPoint = new Vector3(hit.point.x, cuePivot.position.y, hit.point.z);
                    cuePivot.LookAt(targetPoint);
                }
            }
            else
            {
                Vector3 cueBallScreenPoint = InputOutput.WorldToScreenPoint(cueBall.position);
                float orientX = InputOutput.mouseScreenPosition.x > cueBallScreenPoint.x ? 1.0f : -1.0f;
                float orientY = InputOutput.mouseScreenPosition.y > cueBallScreenPoint.y ? 1.0f : -1.0f;
                float rSpeed2D = (orientY * InputOutput.mouseScreenSpeed.x - orientX * InputOutput.mouseScreenSpeed.y) * Time.deltaTime;

                float mouseScreenSpeedValue = 100.0f * rotationSpeedCurve.Evaluate(rSpeed2D / 100.0f) * rotationSpeed2D;
                cuePivot.Rotate(cuePivot.up, mouseScreenSpeedValue);
            }
        }
        void ControlIn3D(MouseState mouseState)
        {
            if (mouseState == MouseState.Down)
            {
                if (cueControlType == CueControlType.ThirdPerson && !InputOutput.isMobilePlatform)
                {
                    cueStateType = CueStateType.StretchCue;
                    cueForwardInScreen = (cueCamera.WorldToScreenPoint(cuePivot.position + cuePivot.forward) - cueCamera.WorldToScreenPoint(cuePivot.position)).normalized;
                    TargetingAtTargetBallInThirdPerson();
                }
                else if (cueControlType == CueControlType.FirstPerson)
                {
                    Vector3 centerPointInSceen = InputOutput.WorldToScreenPoint(cuePivot.position);
                    float cueBallRadiusInScreen = InputOutput.WorldToScreenRadius(cueBallRadius, cueBall.transform);
                    Vector3 mouseScreenPosition = InputOutput.mouseScreenPosition;

                    if (Vector3.Distance(centerPointInSceen, mouseScreenPosition) <= 5.0f * cueBallRadiusInScreen)
                    {
                        cueStateType = CueStateType.TargetingAtCueBall;
                        cueCamera.transform.position = cameraAimingPosition.position;
                        cueCamera.transform.rotation = cameraAimingPosition.rotation;
                        RessetCueForTargeting();
                    }
                    else if (!isSimpleControl && Mathf.Abs(InputOutput.mouseViewportSymmetricalPoint.x) < 0.075f)
                    {
                        cueStateType = CueStateType.TargetingFromTop;
                    }
                    else
                    {
                        cueStateType = CueStateType.TargetingAtTargetBall;
                    }
                }
            }
            else if (mouseState == MouseState.Up)
            {
                if (cueControlType == CueControlType.ThirdPerson)
                {
                    cueStateType = CueStateType.TargetingAtTargetBall;
                }
                else if (cueControlType == CueControlType.FirstPerson)
                {
                    StartCoroutine(MoveCameraToStandartPosition());
                    cueSlider.localPosition = savedCueSliderLocalPosition;
                    if (from2D)
                    {
                        uiController.is3D = false;
                        uiController.CameraToggle();
                        from2D = false;
                    }
                    cueStateType = CueStateType.TargetingAtTargetBall;
                }
            }
            else if (mouseState == MouseState.Press || (uiController.shotOnUp && mouseState == MouseState.Move))
            {
                if (cueStateType == CueStateType.TargetingAtCueBall)
                {
                    Vector3 mouseScreenSpeed = targetingDisplacementSpeed * InputOutput.mouseScreenSpeed * Time.deltaTime;
                    cueDisplacementOnBall += new Vector3(mouseScreenSpeed.x, mouseScreenSpeed.y, 0.0f);
                    cueDisplacementOnBall = Vector3.ClampMagnitude(cueDisplacementOnBall, cueBallRadius);
                    cueDisplacement.localPosition = cueDisplacementOnBall;
                }
                else if (!isSimpleControl && cueStateType == CueStateType.TargetingFromTop)
                {
                    Vector3 mouseScreenSpeed = rotationSpeed3D * InputOutput.mouseScreenSpeed * Time.deltaTime;
                    cameraRotationZ += mouseScreenSpeed.y;
                    cameraRotationZ = Mathf.Clamp(cameraRotationZ, minVertical, maxVertical);
                    cueVertical.localRotation = Quaternion.Euler(cameraRotationZ, 0.0f, 0.0f);
                }
                else if (cueStateType == CueStateType.TargetingAtTargetBall)
                {
                    if (cueControlType == CueControlType.FirstPerson)
                    {
                        Vector3 screenRotateSpeed = InputOutput.mouseScreenSpeed * Time.deltaTime;
                        Vector3 screenRotateSpeedNormalized = screenRotateSpeed.normalized;

                        Vector3 screenSlideSpeed = 1.5f * cueSlideSpeed * InputOutput.mouseScreenSpeed * Time.deltaTime;

                        float k = Mathf.Abs(screenRotateSpeedNormalized.x);
                        if (k < 0.8f)
                        {
                            if (isSimpleControl)
                            {
                                if (!InputOutput.isMobilePlatform && uiController.shotOnUp && mouseState == MouseState.Press)
                                {
                                    Vector3 cueBallScreenPoint = InputOutput.WorldToScreenPoint(cueBall.position);
                                    if (cueBallScreenPoint.y > InputOutput.mouseScreenPosition.y || screenSlideSpeed.y > 0.0f)
                                    {
                                        cueSliderDisplacementZ += screenSlideSpeed.y;
                                        cueSliderDisplacementZ = Mathf.Clamp(cueSliderDisplacementZ, -cueSlidingMaxDisplacement, 0.0f);
                                        cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
                                        savedCueSliderLocalPosition = cueSlider.localPosition;
                                    }
                                    if (cueControlType == CueControlType.FirstPerson)
                                    {
                                        if (InputOutput.mouseScreenPosition.y < 0.4f * Screen.height)
                                        {
                                            cueCamera.transform.position = Vector3.Lerp(cueCamera.transform.position, cameraStandartPosition.position, 15.0f * Time.deltaTime);
                                            cueCamera.transform.rotation = Quaternion.Lerp(cueCamera.transform.rotation, cameraStandartPosition.rotation, 15.0f * Time.deltaTime);
                                        }
                                        else if (InputOutput.mouseScreenPosition.y > 0.5f * Screen.height)
                                        {
                                            cueCamera.transform.position = Vector3.Lerp(cueCamera.transform.position, cueTargetingIn3DModePosition.position, 5.0f * Time.deltaTime);
                                            cueCamera.transform.rotation = Quaternion.Lerp(cueCamera.transform.rotation, cueTargetingIn3DModePosition.rotation, 5.0f * Time.deltaTime);
                                        }
                                    }
                                }
                                else
                                {
                                    Vector3 mouseScreenSpeed = rotationSpeed3D * InputOutput.mouseScreenSpeed * Time.deltaTime;
                                    cameraRotationZ += mouseScreenSpeed.y;
                                    cameraRotationZ = Mathf.Clamp(cameraRotationZ, minVertical, maxVertical);
                                    cueVertical.localRotation = Quaternion.Euler(cameraRotationZ, 0.0f, 0.0f);
                                }
                            }
                            else
                            {
                                cueSliderDisplacementZ += screenSlideSpeed.y;
                                cueSliderDisplacementZ = Mathf.Clamp(cueSliderDisplacementZ, -cueSlidingMaxDisplacement, 0.0f);
                                cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
                                savedCueSliderLocalPosition = cueSlider.localPosition;
                            }
                        }
                        else if (mouseState == MouseState.Press)
                        {
                            if (InputOutput.mouseScreenPosition.y > 0.5f * Screen.height)
                            {
                                cueCamera.transform.position = Vector3.Lerp(cueCamera.transform.position, cueTargetingIn3DModePosition.position, 5.0f * Time.deltaTime);
                                cueCamera.transform.rotation = Quaternion.Lerp(cueCamera.transform.rotation, cueTargetingIn3DModePosition.rotation, 5.0f * Time.deltaTime);
                            }

                            float rSpeed = screenRotateSpeed.x;
                            cuePivot.Rotate(cuePivot.up, (InputOutput.isMobilePlatform ? rotationSpeed3D : mouseRotationSpeed3D) * 50.0f * rotationSpeedCurve.Evaluate(rSpeed / 50.0f));
                        }
                    }
                    else if (cueControlType == CueControlType.ThirdPerson)
                    {
                        TargetingAtTargetBallInThirdPerson();
                    }
                }
                else if (cueStateType == CueStateType.StretchCue)
                {
                    Vector3 mouseScreenSpeed = cueSlideSpeed * InputOutput.mouseScreenSpeed * Time.deltaTime;
                    cueSliderDisplacementZ += Vector3.Dot(mouseScreenSpeed, cueForwardInScreen);;
                    cueSliderDisplacementZ = Mathf.Clamp(cueSliderDisplacementZ, -cueSlidingMaxDisplacement, 0.0f);
                    cueSlider.localPosition = new Vector3(0.0f, 0.0f, cueSliderDisplacementZ);
                    savedCueSliderLocalPosition = cueSlider.localPosition;
                }
            }
        }
    }
}
