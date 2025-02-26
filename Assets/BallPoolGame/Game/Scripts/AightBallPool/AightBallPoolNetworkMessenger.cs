using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool;

namespace NetworkManagement
{
    public interface AightBallPoolMessenger
    {
        void OnSendCueControl(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force);
    }
    public class AightBallPoolNetworkMessenger : NetworkMessenger, AightBallPoolMessenger
    {
        private ShotController _shotController;
        private ShotController shotController
        {
            get
            {
                if (!_shotController)
                {
                    _shotController = ShotController.FindObjectOfType<ShotController>();
                }
                return _shotController;
            }
        }
        private GameManager _gameManager;
        private GameManager gameManager
        {
            get
            {
                if (!_gameManager)
                {
                    _gameManager = GameManager.FindObjectOfType<GameManager>();
                }
                return _gameManager;
            }
        }
        #region sended from network
        public void SetTime(float time01)
        {
            if (BallPoolGameLogic.controlFromNetwork)
            {
                BallPoolGameManager.instance.SetPlayTime(time01);
            }
        }
        public void SetOpponentCueURL(string url)
        {
            shotController.SetOpponentCueURL(url);
        }
        public void SetOpponentTableURLs(string boardURL, string clothURL, string clothColor)
        {
            shotController.SetOpponentTableURLs(boardURL, clothURL, clothColor);
        }

        public IEnumerator OnOpponenInGameScene()
        {
            while (!shotController)
            {
                yield return null;
            }
            shotController.OpponenIsReadToPlay();
        }
        public void OnOpponentForceGoHome()
        {
            Debug.LogWarning("OnOpponentForceGoHome");
            BallPoolGameManager.instance.OnForceGoHome(AightBallPoolPlayer.mainPlayer.playerId);
        }
        public void OnSendCueControl(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
        {
            if (shotController)
            {
                shotController.CueControlFromNetwork(cuePivotLocalRotationY, cueVerticalLocalRotationX, cueDisplacementLocalPositionXY, cueSliderLocalPositionZ, force);
            }
        }
        public void OnForceSendCueControl(float cuePivotLocalRotationY, float cueVerticalLocalRotationX, Vector2 cueDisplacementLocalPositionXY, float cueSliderLocalPositionZ, float force)
        {
            if (shotController)
            {
                shotController.ForceCueControlFromNetwork(cuePivotLocalRotationY, cueVerticalLocalRotationX, cueDisplacementLocalPositionXY, cueSliderLocalPositionZ, force);
            }
        }
        public void OnMoveBall(Vector3 ballPosition)
        {
            if (shotController)
            {
                shotController.MoveBallFromNetwork(ballPosition);
            }
        }
        public void SelectBallPosition(Vector3 ballPosition)
        {
            shotController.SelectBallPositionFromNetwork(ballPosition);
        }
        public void SetBallPosition(Vector3 ballPosition)
        {
            shotController.SetBallPositionFromNetwork(ballPosition);
        }
 
        public void SetMechanicalStatesFromNetwork(int ballId, string mechanicalStateData)
        {
            StartCoroutine(gameManager.balls[ballId].SetMechanicalStatesFromNetwork(mechanicalStateData));
        }
        public void WaitAndStopMoveFromNetwork(float time)
        {
            StartCoroutine(shotController.physicsManager.WaitAndStopMoveFromNetwork(time));
        }
        public void StartSimulate(string impulse)
        {
            shotController.physicsManager.StarShotFromNetwork(impulse);
        }
        public void EndSimulate(string data)
        {
            shotController.physicsManager.CheckEndShot(data);
        }
        #endregion
    }
}
