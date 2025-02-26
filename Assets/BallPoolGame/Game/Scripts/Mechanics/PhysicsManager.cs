using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

namespace BallPool.Mechanics
{
    public enum ShapeType
    {
        Non = 0,
        Ball,
        Board,
        Cloth,
    }

    /// <summary>
    /// Ball exit type from kinematic space.
    /// </summary>
    public enum BallExitType
    {
        Sleep = 0,
        Reactivate}
    ;

    /// <summary>
    /// ball state on table.
    /// </summary>
    public enum BallState
    {
        Non = 0,
        SetState,
        StartMove,
        Move,
        HitBall,
        HitBoard,
        EnterInPocket,
        MoveInPocket,
        ExitFromPocket,
        EndMove}
    ;
    public delegate void BallShotHandler<String>(string impulse);
    public delegate void BallMoveHandler<Int,Vector3>(int ballId,Vector3 position,Vector3 velocity,Vector3 angularVelocity);
    public delegate void BallSleepHandler<Int,Vector3>(int ballId,Vector3 position);
    public delegate void SetStateHandler();
    public delegate void BallHitBallHandler<Ball,Boolean>(Ball ball,Ball hitBall,bool inMove);
    public delegate void BallHitBoardHandler<Ball,Boolean>(Ball ball,bool inMove);
    public delegate void BallHitPocketHandler<Ball,Pocket,Boolean>(Ball ball,Pocket pocket,bool inMove);
    public delegate void BallExitFromPocketHandler<Ball,Pocket,BallExitType,Boolean>(Ball ball,Pocket pocket,BallExitType exitType,bool inMove);

    public struct Impulse
    {
        public readonly Vector3 point;
        public readonly Vector3 impulse;

        public Impulse(Vector3 point, Vector3 impulse)
        {
            this.point = point;
            this.impulse = impulse;
        }
    }

    /// <summary>
    /// The ball hit info.
    /// </summary>
    public struct HitInfo
    {
        public Vector3 point { get; private set; }

        public Vector3 normal { get; private set; }

        public Vector3 positionInHit { get; private set; }

        public ShapeType shapeType { get; private set; }

        public HitInfo(Vector3 point, Vector3 normal, Vector3 positionInHit, ShapeType shapeType)
        {
            this.point = point;
            this.normal = normal;
            this.positionInHit = positionInHit;
            this.shapeType = shapeType;
        }
    }

    /// <summary>
    /// Physics manager.
    /// </summary>
    public class PhysicsManager : MonoBehaviour
    {
        public event BallMoveHandler<int, Vector3> OnBallMove;
        public event BallSleepHandler<int, Vector3> OnBallSleep;
        public event BallShotHandler<string> OnStartShot;
        public event BallShotHandler<string> OnSaveEndStartReplay;
        public event BallShotHandler<string> OnEndShot;
        public event SetStateHandler OnSetState;
        public event BallHitBallHandler<BallListener, bool> OnBallHitBall;
        public event BallHitBoardHandler<BallListener, bool> OnBallHitBoard;
        public event BallHitPocketHandler<BallListener, PocketListener, bool> OnBallHitPocket;
        public event BallExitFromPocketHandler<BallListener, PocketListener, BallExitType, bool> OnBallExitFromPocket;

        public float moveTime{ get; set; }

        [SerializeField] private float _ballMass;
        [SerializeField] private float _ballMaxVelocity;
        [SerializeField] private float _ballMaxAngularVelocity;

        public float ballMass{ get { return _ballMass; } }

        public float ballMaxVelocity{ get { return _ballMaxVelocity; } }

        public float ballMaxAngularVelocity{ get { return _ballMaxAngularVelocity; } }

        public Transform clothSpace;
        public BallListener[] ballsListener;
        public PocketListener[] pocketListeners;
        public ReplayManager replayManager;
        private Impulse impulse;

        public bool inMove{ get; private set; }
        public bool endFromNetwork{ get; set; }

        private bool checkInProgress = false;

        void Awake()
        {
            if (!NetworkManager.initialized)
            {
                enabled = false;
                return;
            }
            Physics.autoSimulation = false;
            Debug.Log("autoSyncTransforms " + Physics.autoSyncTransforms);
            inMove = false;

            Time.fixedDeltaTime = 0.005f;
            Physics.bounceThreshold = 0.01f;
            Physics.sleepThreshold = 0.01f;
            Physics.defaultContactOffset = 0.0005f;
            Physics.defaultSolverIterations = 1;
            Physics.defaultSolverVelocityIterations = 1;

//            Time.fixedDeltaTime = 0.005f;
//            Physics.bounceThreshold = 0.01f;
//            Physics.sleepThreshold = 0.01f;
//            Physics.defaultContactOffset = 0.0005f;
//            Physics.defaultSolverIterations = 1;
//            Physics.defaultSolverVelocityIterations = 1;
           
            foreach (var listener in ballsListener)
            {
                listener.body.linearDamping = 0.5f;
                listener.body.angularDamping = 0.7f;
                listener.body.mass = _ballMass;
                listener.body.maxDepenetrationVelocity = _ballMaxVelocity;
                listener.body.maxAngularVelocity = _ballMaxAngularVelocity;
                listener.body.Sleep();
            }
            replayManager = new ReplayManager();
            if (BallPoolGameLogic.playMode != PlayMode.Replay)
            {
                replayManager.DeleteReplayData();
            }
        }

        void FixedUpdate()
        {
            if (inMove)
            {
                Physics.Simulate(Time.fixedDeltaTime);
//                moveTime += Time.fixedDeltaTime;
//                if (!BallPoolGameLogic.controlFromNetwork)
//                {
//                    if (!checkInProgress && CheckIsSleeping())
//                    {
//                        checkInProgress = true;
//                        StartCoroutine(WaitAndCheckMove());
//                    }
//                }
            }
        }
          
        IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (inMove)
                {
                    moveTime += Time.fixedDeltaTime;
                    if (!BallPoolGameLogic.controlFromNetwork)
                    {
                        if (CheckIsSleeping(false))
                        {
                            yield return new WaitForSeconds(0.1f);

                            if (CheckIsSleeping(true))
                            {
                                yield return StartCoroutine("StopMove");
                            }
                            else
                            {
                                StopCoroutine("StopMove");
                            }
                            checkInProgress = false;
                            //yield return StartCoroutine(WaitAndCheckMove());
                        }
                    }
                }
            }
        }
        public IEnumerator WaitAndStopMoveFromNetwork(float time)
        {
            while (moveTime < time)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return StartCoroutine(StopMove());
            endFromNetwork = true;
        }
        private IEnumerator StopMove()
        {
            inMove = false;
            foreach (BallListener ball in ballsListener)
            {
                if (!ball.body.isKinematic)
                {
                    ball.body.linearVelocity = Vector3.zero;
                    ball.body.angularVelocity = Vector3.zero;
                    ball.body.Sleep();
                }

                CallBallSleep(ball.id, ball.body.position);
            }


            if (BallPoolGameLogic.controlInNetwork)
            {
                yield return new WaitForSeconds(0.2f);
                NetworkManager.network.SendRemoteMessage("WaitAndStopMoveFromNetwork", moveTime);
            }
            if (BallPoolGameLogic.playMode != PlayMode.Replay)
            {
                replayManager.AddReplayDataCount();
            }
            if (OnEndShot != null)
            {
                OnEndShot("");
            }
            moveTime = 0.0f;
        }

        bool CheckIsSleeping(bool forceSleep)
        {
            float minEnergy = 0.1f;
            bool isSleep = true;
            foreach (BallListener ball in ballsListener)
            {
                
                bool ballIsSleeping = (!Geometry.SphereInCube(ball.body.position, ball.radius, clothSpace) || ball.body.isKinematic || (ball.body.linearVelocity.magnitude < minEnergy && ball.radius * ball.body.angularVelocity.magnitude < minEnergy));
                if (!ballIsSleeping)
                {
                    isSleep = false;
                }
            }
            return isSleep;
        }

        public void CallBallSleep(int ballId, Vector3 position)
        {
            if (OnBallSleep != null)
            {
                OnBallSleep(ballId, position);
            }
        }

        public void CallBallMove(int ballId, Vector3 position, Vector3 velocity, Vector3 angularVelocity)
        {
            if (OnBallMove != null)
            {
                OnBallMove(ballId, position, velocity, angularVelocity);
            }
        }

        public void CallBallHitBall(BallListener ball, BallListener hitBall, bool inReplay)
        {
            if (OnBallHitBall != null)
            {
                OnBallHitBall(ball, hitBall, inReplay);
            }
        }

        public void CallBallHitBoard(BallListener ball, bool inReplay)
        {
            if (OnBallHitBoard != null)
            {
                OnBallHitBoard(ball, inReplay);
            }
        }

        public void CallOnBallHitPocket(BallListener ball, PocketListener pocket, bool inReplay)
        {
            if (OnBallHitPocket != null)
            {
                OnBallHitPocket(ball, pocket, inReplay);
            }
        }

        public void CallOnBallExitFromPocket(BallListener ball, PocketListener pocket, bool inReplay)
        {
            if (OnBallExitFromPocket != null)
            {
                OnBallExitFromPocket(ball, pocket, BallExitType.Reactivate, inReplay);
            }
        }

        /// <summary>
        /// Reactivates the ball in cube from pocket.
        /// </summary>
        public void ReactivateBallInCube(float ballRadius, Transform cube, int clothMask, int ballsMask, ref bool canReactivate, ref Vector3 ballNewPosition)
        {
            RaycastHit clothHit;
            Vector3 origin = cube.position + 0.5f * cube.lossyScale.y * cube.up;
            Vector3 direction = -cube.up;
            canReactivate = false;
            ballNewPosition = cube.position;

            for (float x = 0.0f; x < 0.5f * cube.lossyScale.x - 3.0f * ballRadius; x += 3.0f * ballRadius)
            {
                for (float z = 0.0f; z < 0.5f * cube.lossyScale.z - 3.0f * ballRadius; z += 3.0f * ballRadius)
                {
                    origin = cube.position + 0.5f * cube.lossyScale.y * cube.up + x * cube.right + z * cube.forward;

                    if (Physics.Raycast(origin, direction, out clothHit, cube.lossyScale.y, clothMask))
                    {
                        RaycastHit ballHit;
                        if (!Physics.SphereCast(origin, ballRadius, direction, out ballHit, cube.lossyScale.y, ballsMask))
                        {
                            canReactivate = true;
                            ballNewPosition = clothHit.point + ballRadius * clothHit.normal;
                            break;
                        }
                    }
                }
                if (canReactivate)
                {
                    break;
                }
            }
        }

        public void HideBallsLine()
        {

        }

        public void SetImpulse(Impulse impulse)
        {
            this.impulse = impulse;
        }

        /// <summary>
        /// Set state from network and starts the shot.
        /// </summary>
        public void StarShotFromNetwork(string impulse)
        {
            //data
            StartRaplayShot(impulse);
        }

        public void StartRaplayShot(string impulse)
        {
            //string ballsState
            Debug.Log("StartRaplayShot");
            if (OnSaveEndStartReplay != null)
            {
                OnSaveEndStartReplay(impulse);
            }
        }

        public void StartShot(BallListener ball)
        {
            inMove = true;
            moveTime = 0.0f;
           
            ball.body.AddForceAtPosition(impulse.impulse, impulse.point, ForceMode.Impulse);
           
            if (OnStartShot != null)
            {
                OnStartShot("");
            }
        }

        public void CheckEndShot(string data)
        {

        }

        public void SetState(int state)
        {

        }

        public void Disable()
        {

        }
    }
}
