using System.Collections;
using System.Collections.Generic;
using BallPool.Mechanics;
using UnityEngine;

namespace BallPool
{
    public class AightBallPoolGameState : GameState
    {
        /// <summary>
        /// The table will close after first shot.
        /// </summary>
        public bool tableIsOpened = true;
        /// <summary>
        /// The type of the players is e stripes or solids.
        /// </summary>
        public bool playersHasBallType = false;
        /// <summary>
        /// Balls hit board count in first shot.
        /// </summary>
        public int ballsHitBoardCount = 0;
        /// <summary>
        /// The cue ball has hit right ball, stripes, solids or black.
        /// </summary>
        public bool cueBallHasHitRightBall = false;
        /// <summary>
        /// The cue ball has hit some ball at first hit.
        /// </summary>
        public bool cueBallHasHitSomeBall = false;
        /// <summary>
        /// The cue ball has right ball in pocket, stripes, solids or black.
        /// </summary>
        public bool hasRightBallInPocket = false;
        /// <summary>
        /// The current player has cue ball in his hand
        /// </summary>
        public bool cueBallInHand = true;
        /// <summary>
        /// The cue ball in pocket.
        /// </summary>
        public bool cueBallInPocket = false;

        public AightBallPoolGameState()
            : base()
        {
            tableIsOpened = true;
            playersHasBallType = false;
            ballsHitBoardCount = 0;
            cueBallHasHitRightBall = false;
            cueBallHasHitSomeBall = false;
            hasRightBallInPocket = false;
            cueBallInHand = true;
            cueBallInPocket = false;
        }
    }

    public class AightBallPoolGameLogic : BallPoolGameLogic
    {
        private static AightBallPoolGameState _gameState;

        public static AightBallPoolGameState gameState
        {
            get
            {
                if (_gameState == null)
                {
                    _gameState = new AightBallPoolGameState();
                }
                return _gameState;
            }
        }

        public void RessetState()
        {
            gameState.gameIsComplete = false;
            gameState.needToChangeTurn = false;
            gameState.cueBallHasHitRightBall = false;
            gameState.cueBallHasHitSomeBall = false;
            gameState.hasRightBallInPocket = false;
            gameState.ballsHitBoardCount = 0;
            gameState.cueBallInHand = false;
            gameState.cueBallInPocket = false;

            AightBallPoolPlayer.mainPlayer.isCueBall = false;
            AightBallPoolPlayer.otherPlayer.isCueBall = false;
        }

        public void OnBallHitBoard(int ballId)
        {
            if (gameState.tableIsOpened)
            {
                gameState.ballsHitBoardCount++;
            }
        }

        public void OnCueBallHitBall(int cueBallId, int ballId)
        {
            if (gameState.cueBallHasHitSomeBall)
            {
                return;
            }
            gameState.cueBallHasHitSomeBall = true;

            if (isSmallestBall(ballId))
            {
                gameState.cueBallHasHitRightBall = true;
            }
            else if (gameState.tableIsOpened)
            {
                gameState.cueBallHasHitRightBall = true;
            }

            //if (isBlackBall(ballId))
            //{
            //    if (((AightBallPoolPlayer)BallPoolPlayer.currentPlayer).isBlack)
            //    {
            //        gameState.cueBallHasHitRightBall = true;
            //    }
            //}
            //else if (gameState.tableIsOpened)
            //{
            //    gameState.cueBallHasHitRightBall = true;
            //}
            //else if (!gameState.playersHasBallType)
            //{
            //    gameState.cueBallHasHitRightBall = true;
            //}
            //else if (AightBallPoolPlayer.PlayerHasSomeBallType((AightBallPoolPlayer)BallPoolPlayer.currentPlayer, ballId))
            //{
            //    gameState.cueBallHasHitRightBall = true;
            //}
            if (!gameState.cueBallHasHitRightBall)
            {
                gameState.cueBallInHand = true;
                gameState.needToChangeTurn = true;
            }
        }

        public void OnBallInPocket(int ballId, ref bool cueBallInPocket)
        {
            if (isCueBall(ballId))
            {
                if (AightBallPoolPlayer.mainPlayer.myTurn)
                {
                    AightBallPoolPlayer.mainPlayer.isCueBall = true;
                }
                else if (AightBallPoolPlayer.otherPlayer.myTurn)
                {
                    AightBallPoolPlayer.otherPlayer.isCueBall = true;
                }
                gameState.needToChangeTurn = true;
                gameState.cueBallInHand = true;
                cueBallInPocket = true;
                gameState.cueBallInPocket = true;
                return;
            }
            if (gameState.tableIsOpened)
            {
                if (!isBlackBall(ballId))
                {
                    gameState.hasRightBallInPocket = true;
                }
            }
            else
            {
                #region New Change
                GameManager.Instance.DeactiveBall(ballId);
                BallsUIManager.Instance.NullOneBall(ballId);
                gameState.hasRightBallInPocket = true;
                #endregion

                //if (!gameState.playersHasBallType)
                //{
                //    gameState.playersHasBallType = true;
                //    if (!isBlackBall(ballId))
                //    {
                //        gameState.hasRightBallInPocket = true;
                //    }
                //    if (AightBallPoolPlayer.mainPlayer.myTurn)
                //    {
                //        if (AightBallPoolGameLogic.isStripesBall(ballId))
                //        {
                //            AightBallPoolPlayer.mainPlayer.isStripes = true;
                //            AightBallPoolPlayer.otherPlayer.isSolids = true;
                //        }
                //        else if (AightBallPoolGameLogic.isSolidsBall(ballId))
                //        {
                //            AightBallPoolPlayer.mainPlayer.isSolids = true;
                //            AightBallPoolPlayer.otherPlayer.isStripes = true;
                //        }
                //    }
                //    else if (AightBallPoolPlayer.otherPlayer.myTurn)
                //    {
                //        if (AightBallPoolGameLogic.isStripesBall(ballId))
                //        {
                //            AightBallPoolPlayer.otherPlayer.isStripes = true;
                //            AightBallPoolPlayer.mainPlayer.isSolids = true;
                //        }
                //        else if (AightBallPoolGameLogic.isSolidsBall(ballId))
                //        {
                //            AightBallPoolPlayer.otherPlayer.isSolids = true;
                //            AightBallPoolPlayer.mainPlayer.isStripes = true;
                //        }
                //    }
                //}
                //else if (AightBallPoolPlayer.PlayerHasSomeBallType((AightBallPoolPlayer)BallPoolPlayer.currentPlayer, ballId))
                //{
                //    gameState.hasRightBallInPocket = true;
                //}
            }
        }

        public void OnEndShot(bool blackBallInPocket, out bool gameIsEnd)
        {
            gameIsEnd = false;
            if (BallPoolGameManager.instance == null)
            {
                return;
            }
            string info = "";
           
            bool canSetInfo = true;

            if (gameState.cueBallInPocket && !blackBallInPocket)
            {
                if (BallPoolPlayer.mainPlayer.myTurn)
                {
                    info = "You pocket the cue ball, \n" + AightBallPoolPlayer.otherPlayer.name + " has cue ball in hand";
                }
                else
                {
                    info = AightBallPoolPlayer.otherPlayer.name + " pocket the cue ball, \n" + " You have cue ball in hand";
                }
                BallPoolGameManager.instance.SetGameInfo(info);
                canSetInfo = false;
            }
            if (gameState.tableIsOpened)
            {
                info = " ";
                if (!(gameState.cueBallHasHitRightBall && gameState.hasRightBallInPocket))
                {
                    gameState.needToChangeTurn = true;
                    if (gameState.ballsHitBoardCount < 4)
                    {
                        gameState.cueBallInHand = true;
                        info = "Break up of balls was weak, \n" + (BallPoolPlayer.mainPlayer.myTurn ? AightBallPoolPlayer.otherPlayer.name + " has cue ball in hand" : "You have cue ball in hand");
                    }
                }
            }
            gameState.tableIsOpened = false;
            if (blackBallInPocket)
            {
                if (AightBallPoolPlayer.mainPlayer.myTurn)
                {
                    //AightBallPoolPlayer.mainPlayer.isWinner = AightBallPoolPlayer.mainPlayer.isBlack && !AightBallPoolPlayer.mainPlayer.isCueBall;
                    AightBallPoolPlayer.mainPlayer.isWinner = gameState.cueBallHasHitRightBall && !gameState.cueBallInPocket;
                    AightBallPoolPlayer.otherPlayer.isWinner = !AightBallPoolPlayer.mainPlayer.isWinner;
                    info = AightBallPoolPlayer.otherPlayer.isWinner ? ("You poked the black ball " + (AightBallPoolPlayer.mainPlayer.isCueBall ? "with cue ball" : "")) : "";
                }
                else if (AightBallPoolPlayer.otherPlayer.myTurn)
                {
                    AightBallPoolPlayer.otherPlayer.isWinner = gameState.cueBallHasHitRightBall && !gameState.cueBallInPocket;
                    AightBallPoolPlayer.mainPlayer.isWinner = !AightBallPoolPlayer.otherPlayer.isWinner;
                    info = AightBallPoolPlayer.mainPlayer.isWinner ? (AightBallPoolPlayer.otherPlayer.name + " pocket the black ball " + (AightBallPoolPlayer.otherPlayer.isCueBall ? "with cue ball" : "")) : "";
                } 
                gameState.needToChangeTurn = false;
                gameIsEnd = true;
                BallPoolGameManager.instance.SetGameInfo(info);
                return;
            }

            if (AightBallPoolPlayer.mainPlayer.checkIsBlackInEnd)
            {
                AightBallPoolPlayer.mainPlayer.isBlack = true;
            }
            if (AightBallPoolPlayer.otherPlayer.checkIsBlackInEnd)
            {
                AightBallPoolPlayer.otherPlayer.isBlack = true;
            }

            if (!gameState.cueBallHasHitRightBall)
            {
                gameState.cueBallInHand = true;
                gameState.needToChangeTurn = true;

                if (AightBallPoolPlayer.mainPlayer.myTurn)
                {
                    if (info == "")
                        info = AightBallPoolPlayer.mainPlayer.isBlack ? "You need to hit black ball" : (AightBallPoolPlayer.mainPlayer.isSolids ? "You need to hit solids ball" : (AightBallPoolPlayer.mainPlayer.isStripes ? "You need to hit stripes ball" : "You need to hit solids or stripes ball")) +
                        "\n" + AightBallPoolPlayer.otherPlayer.name + " has cue ball in hand";
                }
                else
                {
                    if (info == "")
                        info = AightBallPoolPlayer.otherPlayer.name + ((AightBallPoolPlayer.otherPlayer.isBlack ? " need to hit black ball" : (AightBallPoolPlayer.otherPlayer.isSolids ? " need to hit solids ball" : (AightBallPoolPlayer.otherPlayer.isStripes ? " need to hit stripes ball" : " need to hit solids or stripes ball")))) +
                        ", \nYou have cue ball in hand";
                }
            }
            else if (!gameState.hasRightBallInPocket)
            {
                gameState.needToChangeTurn = true;
                //gameState.cueBallInHand = true;
                if (AightBallPoolPlayer.mainPlayer.myTurn)
                {
                    if (info == "")
                        info = AightBallPoolPlayer.mainPlayer.isBlack ? "You need to pocket solids ball" : (AightBallPoolPlayer.mainPlayer.isSolids ? "You need to pocket solids ball" : (AightBallPoolPlayer.mainPlayer.isStripes ? "You need to pocket stripes ball" : "You need to pocket solids or stripes ball"));
                }
                else
                {
                    if (info == "")
                        info = AightBallPoolPlayer.otherPlayer.name + (AightBallPoolPlayer.otherPlayer.isBlack ? " need to pocket black ball" : ((AightBallPoolPlayer.otherPlayer.isSolids ? " need to pocket solids ball" : (AightBallPoolPlayer.otherPlayer.isStripes ? " need to pocket stripes ball" : " need to pocket solids or stripes ball"))));
                }
            }
            if (canSetInfo)
            {
                BallPoolGameManager.instance.SetGameInfo(info);
            }
        }

        public override void OnEndTime()
        {
            UnityEngine.Debug.Log("OnEndPlayTime");
            gameState.cueBallInHand = true;
            string info = AightBallPoolPlayer.mainPlayer.myTurn ? "You run out of time\n" + AightBallPoolPlayer.otherPlayer.name + " has cue ball in hand" : AightBallPoolPlayer.otherPlayer.name + " run out of time, \nYou have cue ball in hand ";
            BallPoolGameManager.instance.SetGameInfo(info);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _gameState = null;
        }

        public static Ball GetCueBall(Ball[] balls)
        {
            foreach (Ball ball in balls)
            {
                if (isCueBall(ball.id))
                {
                    return ball;
                }
            }
            return null;
        }

        /// <summary>
        /// Is the ball in pocket?, not for the cue ball, the cue ball will be resetted
        /// </summary>
        public static bool ballInPocket(Ball ball)
        {
            return ball.inPocket;
        }

        public static Ball GetBlackBall(Ball[] balls)
        {
            foreach (Ball ball in balls)
            {
                if (isBlackBall(ball.id))
                {
                    return ball;
                }
            }
            return null;
        }

        public static bool isCueBall(int id)
        {
            return id == 0;
        }

        public static bool isBlackBall(int id)
        {
            //return id == 8;
            return id == 9;
        }

        public static bool isStripesBall(int id)
        {
            return id > 8 && id < 16;
        }

        public static bool isSolidsBall(int id)
        {
            return id > 0 && id < 8;
        }

        public static bool isSmallestBall(int id)
        {
            if (GameManager.Instance.ActiveBalls.Count == 1)
            {
                return true;
            }

            foreach (Ball ball in GameManager.Instance.ActiveBalls)
            {
                if (id > ball.id)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool isLastBall(int id)
        {
            return id == 9;
        }
    }
}
