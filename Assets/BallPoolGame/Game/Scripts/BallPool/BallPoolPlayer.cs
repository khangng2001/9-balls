using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool.Mechanics;
using NetworkManagement;

namespace BallPool
{
    public delegate void TurnChangedHandler();
    public delegate void PlayerActionHandler(BallPoolPlayer player);
    /// <summary>
    /// The player.
    /// </summary>
    public abstract class BallPoolPlayer
    {
        /// <summary>
        /// The main player.
        /// </summary>
        public static BallPoolPlayer mainPlayer
        {
            get{ return (players == null || players.Length < 1)? null: players[0]; }
        }
        /// <summary>
        /// Gets the player identifier in game, not social id.
        /// </summary>
        /// <value>The player identifier.</value>
        public int playerId
        {
            get;
            private set;
        }
        public string name 
        {
            get;
            set;
        }
        public int coins
        {
            get;
            private set;
        }
        public void SetCoins(int coins)
        {
            this.coins = coins;
        }
        public static BallPoolPlayer GetWinner()
        {
            foreach (BallPoolPlayer player in players) 
            {
                if (player.isWinner)
                {
                    return player;
                }
            }
            return null;
        }
        public static void SetWinner(int playerId)
        {
            foreach (BallPoolPlayer player in players) 
            {
                player.isWinner = player.playerId == playerId;
            }
        }
        /// <summary>
        /// The current player, whose turn to play at the moment.
        /// </summary>
        public static BallPoolPlayer currentPlayer
        {
            get
            {
                foreach (BallPoolPlayer player in players) 
                {
                    if(player.playerId == turnId)
                    {
                        return player;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Updates the player coins when game is completed.
        /// </summary>
        public static void UpdateCouns(bool startGame)
        {
            foreach (BallPoolPlayer player in players)
            {
                if (startGame)
                {
                    player.coins -= BallPoolPlayer.prize;
                }
                else if (player.isWinner)
                {
                    player.coins += 2 * BallPoolPlayer.prize;
                }
                player.coins = Mathf.Clamp(player.coins, NetworkManager.social.minCoinsCount, player.coins);
            }
        }
        public static void SetMainPlayerWinnerCouns()
        {
            ReturnMainPlayerCouns();
            ReturnMainPlayerCouns();
        }
        public static void ReturnMainPlayerCouns()
        {
            BallPoolPlayer.mainPlayer.coins += BallPoolPlayer.prize;
        }
        /// <summary>
        /// Maybe Avatar texture
        /// </summary>
        public object avatar 
        {
            get;
            protected set;
        }
        public string avatarURL
        {
            get;
            private set;
        }

        public List<Ball> balls
        {
            get;
            protected set;
        }

        public static int playersCount
        {
            get;
            set;
        }

        public static BallPoolPlayer[] players
        {
            get;
            set;
        }
        /// <summary>
        /// if 1: Main Player turn els , if 2: Other Player turn.
        /// </summary>
        public static int turnId
        {
            get;
            set;
        }
        public bool isWinner
        {
            get;
            set;
        }
        public bool myTurn
        {
            get;
            private set;
        }

        public static int prize
        {
            get
            {
                if(players == null || players.Length == 0)
                {
                    return 20;
                }
                int prize = players[0].GetPrize();
                if (prize == 0)
                {
                    prize = 20;
                }
                return prize;
            }
            set
            {
                if (players != null && players.Length != 0)
                {

                    int prize = value;
                    if (prize >= 0)
                    {
                        players[0].SavePrize(prize);
                    }
                }
            }
        }
        public static void Deactivate()
        {
            OnPlayerInitialized = null;
            OnTurnChanged = null;
            if (players != null)
            {
                foreach (BallPoolPlayer player in players)
                {
                    player.OnDeactivate();
                    player.isWinner = false;
                    player.myTurn = false;
                    player.balls = null;
                }
            }
        }
        public static bool initialized
        {
            get
            {
                return players != null;
            }
        }
        public static event TurnChangedHandler OnTurnChanged;
        public static event PlayerActionHandler OnPlayerInitialized;

        /// <summary>
        /// Change the players turn.
        /// </summary>
        public static void ChangeTurn()
        {
            UnityEngine.Debug.Log("ChangeTurn " + BallPoolPlayer.turnId);
            if (BallPoolPlayer.turnId < players.Length - 1)
            {
                BallPoolPlayer.turnId++;
            }
            else
            {
                BallPoolPlayer.turnId = 0;
            }
            for (int i = 0; i < players.Length; i++)
            {
                players[i].myTurn = BallPoolPlayer.turnId == i;
            }
            if (OnTurnChanged != null)
            {
                OnTurnChanged();
            }
        }
        /// <summary>
        /// Set the players turn.
        /// </summary>
        public static void SetTurn(int turnId)
        {
            BallPoolPlayer.turnId = turnId;
            for (int i = 0; i < players.Length; i++)
            {
                players[i].myTurn = turnId == i;
            }
            if (OnTurnChanged != null)
            {
                OnTurnChanged();
            }
        }
        /// <summary>
        /// Gets the activ balls Ides array.
        /// </summary>
        public string[] GetActiveBallsIds()
        {
            if (balls == null)
            {
                return null;
            }
            string[] data = new string[balls.Count];
            for (int i = 0; i < balls.Count; i++)
            {
                data[i] = balls[i].id + "";
            }
            return data;
        }
        public BallPoolPlayer (int playerId, string name, int coins, object avatar, string avatarURL)
        {
            this.playerId = playerId;
            this.name = name;
            this.coins = coins;
            this.avatar = avatar;
            this.avatarURL = avatarURL;
            if (OnPlayerInitialized != null)
            {
                OnPlayerInitialized(this);
            }
        }

        public abstract void OnDeactivate();

        public virtual void SetActiveBalls(Ball[] balls)
        {
            this.balls = new List<Ball>(0);
            foreach (Ball ball in balls)
            {
                if (!ball.inPocket)
                {
                    this.balls.Add(ball);
                }
            }
        }
        public static void SaveCoins()
        {
            if (BallPoolGameLogic.playMode == PlayMode.OnLine)
            {
                NetworkManager.mainPlayer.UpdateCoins(mainPlayer.coins);
            }
            else if (BallPoolGameLogic.playMode == PlayMode.PlayerAI || BallPoolGameLogic.playMode == PlayMode.HotSeat)
            {
                foreach (BallPoolPlayer player in BallPoolPlayer.players)
                {
                    if (player == mainPlayer)
                    {
                        if (BallPoolGameLogic.playMode != PlayMode.HotSeat && BallPoolGameLogic.playMode != PlayMode.PlayerAI)
                        {
                            NetworkManager.mainPlayer.UpdateCoins(mainPlayer.coins);
                        }
                        else
                        {
                            NetworkManager.mainPlayer.UpdateCoinsWithoutSave(mainPlayer.coins);
                        }
                    }
                    else
                    {
                       DataManager.SetInt("PlayerCoins" + player.playerId, player.coins);
                    }
                }
            }
        }

        protected virtual void SavePrize(int prize)
        {
            NetworkManager.social.SaveMainPlayerPrize(prize);
        }
        protected virtual int GetPrize()
        {
            return  NetworkManager.social.GetMainPlayerPrize();
        }
        public IEnumerator DownloadAvatar()
        {
            DownloadManager.DownloadParameters parameters = new DownloadManager.DownloadParameters(avatarURL, "Avatar", DownloadManager.DownloadType.Update);
            yield return DownloadManager.Download(parameters);
            if (!parameters.isNull)
            {
                avatar = parameters.texture;
            }
        }
    }
}
