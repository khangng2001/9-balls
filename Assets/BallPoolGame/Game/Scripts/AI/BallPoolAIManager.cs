using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool.Mechanics;
using NetworkManagement;


namespace BallPool.AI
{
    public delegate bool FindBestTargetBallException<Int>(int ballId);
    public delegate void CalculateAIHandler<BallPoolAIManager>(BallPoolAIManager aiManager);

    /// <summary>
    /// The "best target ball" info which is found in the calculation AI, for shot into the kinematic space.
    /// </summary>
    public struct BestTargetBallInfo
    {
        internal readonly Ball targetBall;
        internal readonly int pocketId;
        internal readonly Vector3 shotBallPosition;
        internal readonly Vector3 shotPoint;
        internal readonly Vector3 aimpoint;
        internal readonly float impulse;

        public BestTargetBallInfo(Ball targetBall, int pocketId, Vector3 shotBallPosition, Vector3 shotPoint, Vector3 aimpoint, float impulse)
        {
            this.targetBall = targetBall;
            this.pocketId = pocketId;
            this.shotBallPosition = shotBallPosition;
            this.shotPoint = shotPoint;
            this.aimpoint = aimpoint;
            this.impulse = impulse;
        }
        public override string ToString()
        {
            return "Target ball id: " + (this.targetBall? this.targetBall.id.ToString():"null").ToString() + ", pocket id: " + this.pocketId + ", Shot ball position: " + this.shotBallPosition + ", Shot point: " + this.shotPoint + ", Aimpoint: " + this.aimpoint + ", Impulse: " + this.impulse;
        }
    }

    public abstract class BallPoolAIManager : MonoBehaviour
    {
        public event CalculateAIHandler<BallPoolAIManager> OnStartCalculateAI;
        public event CalculateAIHandler<BallPoolAIManager> OnEndCalculateAI;

        [SerializeField] private ShotController shotController;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Transform pockets;
        private PocketListener[] targets;

        void Awake()
        {
            targets = pockets.GetComponentsInChildren<PocketListener>();
        }
        /// <summary>
        /// The "best target ball" info which is found in the calculation AI.
        /// </summary>
        public BestTargetBallInfo info
        {
            get;
            set;
        }

        /// <summary>
        /// Is AI calculation not completed successfully and do not found best target ball.
        /// </summary>
        public bool haveExaption
        {
            get;
            set;
        }
        public bool cancelCalculateAI
        {
            get;
            set;
        }
        public bool calculateAI
        {
            get;
            set;
        }
        public void CancelCalculateAI()
        {
            cancelCalculateAI = true;
            calculateAI = false;
        }

        public void CalculateAI()
        {
            if (calculateAI || shotController.physicsManager.inMove)
            {
                return;
            }
            if (ProductAI.aiCount == 0)
            {
                return;
            }
            Debug.Log("CalculateAI");

            calculateAI = true;
            haveExaption = false;
            cancelCalculateAI = false;

            if (OnStartCalculateAI != null)
            {
                OnStartCalculateAI(this);
            }

            CalculateAI(3, shotController.cueBall, shotController.physicsManager.ballMaxVelocity, FindException, AightBallPoolGameLogic.gameState.cueBallInHand && !AightBallPoolGameLogic.gameState.tableIsOpened);
        }
        private void CallEndCalculateAI(bool haveExaption)
        {
            this.haveExaption = haveExaption;
            calculateAI = false;
            if (OnEndCalculateAI != null)
            {
                OnEndCalculateAI(this);
            }
        }
   
        public abstract bool FindException(int ballId);

        /// <summary>
        /// Check AI for finds the best target ball for shot into the pockets.
        /// </summary>
        private void CalculateAI(int calculateMaxCount, Ball cueBall, float impulse, FindBestTargetBallException<int> FindException, bool changePosition)
        {
            if (AightBallPoolGameLogic.gameState.tableIsOpened)
            {
                Vector3 ballNewPosition = shotController.firstMoveSpace.position
                                          + Random.Range(-0.5f * shotController.firstMoveSpace.lossyScale.x, 0.5f * shotController.firstMoveSpace.lossyScale.y) * Vector3.right
                                          + Random.Range(-0.5f * shotController.firstMoveSpace.lossyScale.z, 0.5f * shotController.firstMoveSpace.lossyScale.z) * Vector3.forward;
                ballNewPosition = new Vector3(ballNewPosition.x, cueBall.position.y, ballNewPosition.z);
                cueBall.position = Geometry.ClampPositionInCube(ballNewPosition, cueBall.radius, shotController.firstMoveSpace);
            }

            float newImpulse = AightBallPoolGameLogic.gameState.tableIsOpened ? impulse : Random.Range(0.5f * impulse, 0.7f * impulse);
            info = FindBestTargetBall(cueBall, newImpulse, FindException, changePosition);
            haveExaption = !info.targetBall || FindException(info.targetBall.id);
            CallEndCalculateAI(haveExaption);
        }
        /// <summary>
        /// Finds the best target ball for shot into the pockets.
        /// </summary>
        private BestTargetBallInfo FindBestTargetBall (Ball cueBall, float impulse, FindBestTargetBallException<int> FindException, bool changePosition)
        {
            int pocketId = 0;
            Vector3 shotBallPosition = cueBall.position;
            Vector3 shotPoint = Vector3.zero;
            Vector3 aimpoint = Vector3.zero;
            Ball ball = BallAICalculator.FindBestTargetBall(cueBall, targets, gameManager.balls,  out pocketId, out shotBallPosition, out shotPoint, out aimpoint, ref impulse, FindException, changePosition, shotController.ballLayer, shotController.boardLayer);
            return new BestTargetBallInfo(ball, pocketId, shotBallPosition, shotPoint, aimpoint, impulse);
        }
    }
}
