using System.Collections;
using System.Collections.Generic;
using BallPool.AI;
using BallPool.Mechanics;
using UnityEngine;

namespace BallPool
{
    /// <summary>
    /// The game manager.
    /// </summary>
    public class BallPoolGameManager
    {
        public PlayAgainMenu _playAgainMenu;
        public PlayAgainMenu playAgainMenu
        {
            get
            {
                if (!_playAgainMenu)
                {
                    _playAgainMenu = PlayAgainMenu.FindObjectOfType<PlayAgainMenu>();
                }
                return _playAgainMenu;
            }
        }
        /// <summary>
        /// Occurs when enable control for current player.
        /// </summary>
        public event System.Action<bool> OnEnableControl;
        /// <summary>
        /// Occurs when game is complite.
        /// </summary>
        public event System.Action OnGameComplite;
        public event System.Action OnShotEnded;
        public event System.Action OnCalculateAI;
        /// <summary>
        /// Occurs when AI player shot.
        /// </summary>
        public event System.Action OnShotAI;
        /// <summary>
        /// Occurs when set game prize.
        /// </summary>
        public event System.Action<int> OnSetPrize;
        /// <summary>
        /// Occurs when set player.
        /// </summary>
        public event System.Action<BallPoolPlayer> OnSetPlayer;
        /// <summary>
        /// Occurs when set active player who will start playing.
        /// </summary>
        public event System.Action<BallPoolPlayer, bool> OnSetActivePlayer;
        /// <summary>
        /// Occurs when set player avatar.
        /// </summary>
        public event System.Action<BallPoolPlayer> OnSetAvatar;
        /// <summary>
        /// Occurs when set active balls identifiers, Which are not in the kinematic space.
        /// </summary>
        public event System.Action<BallPoolPlayer> OnSetActiveBallsIds;
        /// <summary>
        /// Occurs when update game time.
        /// </summary>
        public event System.Action<float> OnUpdateTime;
        /// <summary>
        /// Occurs when start time before the turn.
        /// </summary>
        public event System.Action OnStartTime;
        /// <summary>
        /// Occurs when start time during the turn.
        /// </summary>
        public event System.Action OnStopTime;
        /// <summary>
        /// Occurs when end time for turn.
        /// </summary>
        public event System.Action OnEndTime;
        /// <summary>
        /// Occurs when on set game info, (game result).
        /// </summary>
        public event System.Action<string> OnSetGameInfo;


        public Ball[] balls{ get; private set;}

        public static BallPoolGameManager instance
        {
            get;
            private set;
        }

        public void Initialize(PhysicsManager physicsManager, BallPoolAIManager aiManager, Ball[] balls)
        {
            instance = this;
            this.balls = balls;
            this.physicsManager = physicsManager;
            this.aiManager = aiManager;

            physicsManager.OnStartShot += (string data) => 
                {
                    OnStartShot(data);
                };
            physicsManager.OnEndShot += (string data) =>
                {
                    OnEndShot(data);
                };
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
            physicsManager.OnBallSleep += (int ballId, Vector3 position) => 
                {
                    balls[ballId].OnState(BallState.EndMove);
                };
        }

        public virtual void OnStartShot(string data)
        {
            for (int i = 0; i < balls.Length; i++) 
            {
                balls[i].OnState(BallState.SetState);
            }
            balls[0].OnState(BallState.StartMove);
            calculateTime = false;
            if(OnStopTime != null)
            {
                OnStopTime();
            }
        }
        public virtual void OnEndShot(string data)
        {
            calculateTime = true; 
            playTime = 0.0f;
            if(OnStartTime != null)
            {
                OnStartTime();
            }
        }
        private string GetBallsMechanicalsState()
        {
            return "";
        }
        public PhysicsManager physicsManager
        {
            get;
            private set;
        }

        public BallPoolAIManager aiManager
        {
            get;
            private set;
        }

        public float maxPlayTime
        {
            get;
            set;
        }
        public float playTime
        {
            get;
            set;
        }
        public bool calculateTime
        {
            get;
            private set;
        }
        public bool gameIsComplite
        {
            get;
            private set;
        }
        public string gameInfoText
        {
            get;
            private set;
        }

        public void SetGameInfo(string info)
        {
            gameInfoText = info;
            if (OnSetGameInfo != null)
            {
                OnSetGameInfo(info);
            }
        }
        protected void CallOnEnableControl(bool value)
        {
            calculateTime = !BallPoolGameLogic.controlFromNetwork; 
            playTime = 0.0f;
            if (OnStartTime != null)
            {
                OnStartTime();
            }
            if (OnEnableControl != null)
            {
                OnEnableControl(value && calculateTime);
            }
        }
        protected void CallOnShotAI()
        {
            if (OnShotAI != null)
            {
                OnShotAI();
            }
        }
        protected void CallOnCalculateAI()
        {
            if (aiManager.calculateAI)
            {
                return;
            }
            if (OnCalculateAI != null)
            {
                OnCalculateAI();
            }
        }
        protected void CallOnEndShot()
        {
            if (OnShotEnded != null)
            {
                OnShotEnded();
            }
        }
        protected void CallOnGameComplite()
        {
            gameIsComplite = true;
            if (OnGameComplite != null)
            {
                OnGameComplite();
            }
        }
        protected void CallOnSetPrize(int prize)
        {
            if (OnSetPrize != null)
            {
                OnSetPrize(prize);
            }
        }
        protected void CallOnSetActivePlayer(BallPoolPlayer player, bool value)
        {
            if (OnSetActivePlayer != null)
            {
                OnSetActivePlayer(player, value);
            }
        }
        protected void CallOnSetPlayer(BallPoolPlayer player)
        {
            if (OnSetPlayer != null)
            {
                OnSetPlayer(player);
            }
        }
        protected void CallOnSetAvatar(BallPoolPlayer player)
        {
            if (OnSetAvatar != null)
            {
                OnSetAvatar(player);
            }
        }
        protected void CallOnSetActiveBallsIds(BallPoolPlayer player)
        {
            if (OnSetActiveBallsIds != null)
            {
                OnSetActiveBallsIds(player);
            }
        }
        protected void CallOnUpdateTime(float deltaTime)
        {
            if (!gameIsComplite && calculateTime)
            {
                if (playTime < 1.0f)
                {
                    playTime += deltaTime / maxPlayTime;
                    if (OnUpdateTime != null)
                    {
                        OnUpdateTime(playTime);
                    }
                }
                else
                {
                    EndTime();
                }
            }
        }
        public void SetPlayTime(float time01)
        {
            if (time01 < 1.0f)
            {
                playTime = time01;
                if (OnUpdateTime != null)
                {
                    OnUpdateTime(playTime);
                }
            }
            else
            {
                EndTime();
            }
        }
        private void EndTime()
        {
            playTime = 1.0f;
            BallPoolGameLogic.instance.OnEndTime();
            if (OnEndTime != null)
            {
                OnEndTime();
            }
            BallPoolPlayer.ChangeTurn();
        }

        public virtual void OnForceGoHome(int winnerId)
        {

        }
        public virtual void OnDisable()
        {
            OnGameComplite = null;
            OnShotEnded = null;
            OnCalculateAI = null;
            OnShotAI = null;
            OnSetPrize = null;
            OnSetPlayer = null;
            OnSetAvatar = null;
            OnSetActiveBallsIds = null;
            OnUpdateTime = null;
            OnStartTime = null;
            OnStopTime = null;
            OnEndTime = null;
            OnSetGameInfo = null;
            physicsManager = null;
            aiManager = null;
            instance = null;
        }
    }
}
