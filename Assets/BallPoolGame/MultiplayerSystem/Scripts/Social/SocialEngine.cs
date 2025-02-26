using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkManagement
{
   
    /// <summary>
    /// The social API, who works with social platforms.
    /// </summary>
    public abstract class SocialEngine
    {
        public SocialEngine()
        {
            mainPlayerId = "0";
        }
        public abstract void ShareOnFacebook();
        public abstract void ShareOnTwitter();
        public abstract void ShareOnGoogle();
        public abstract void ShareByEmail();

        public event LoginHandler OnSignUp;
        public event LoginHandler OnLogin;
        public event LoginHandler OnLoginWithFacebok;
        public event System.Action OnFacebokInitialized;

        public bool friendsListIsUpdated{ get; protected set; }

        public virtual void Disable()
        {
            OnSignUp = null;
            OnLogin = null;
            OnLoginWithFacebok = null;
        }

        protected void CallSignUpEvent(LoginedState state)
        {
            //Imitating successful Sign Up
            LoginManager.logined = state == LoginedState.Successful;
            if (OnSignUp != null)
            {
                OnSignUp(state);
            }
        }
        protected void CallLoginEvent(LoginedState state)
        {
            //Imitating successful login
            LoginManager.logined = state == LoginedState.Successful;
            if (OnLogin != null)
            {
                OnLogin(state);
            }
        }
        protected void CallLoginWithFacebokEvent(LoginedState state)
        {
            //Imitating successful login with Facebook
            if (OnLoginWithFacebok != null)
            {
                OnLoginWithFacebok(state);
            }
        }
        protected void CallFacebokInitialized()
        {
            if (OnFacebokInitialized != null)
            {
                OnFacebokInitialized();
            }
        }
        /// <summary>
        /// Gets the main player identifier.
        /// </summary>
        public string mainPlayerId{get;protected set;}
        /// <summary>
        /// Gets the friends identifier.
        /// </summary>
        /// 
        public int minCoinsCount{ get { return 10; } }
        public int minOnLinePrize{ get { return 20; } }

        public abstract void SignUp(string email, string password);
        public abstract void Login(string email, string password);
        public abstract void LoginWithFacebok();

        public abstract void SaveMainPlayerName(string playerName);
        public abstract string GetMainPlayerName();
        public abstract void SaveMainPlayerCoins(int playerCoins);
        public abstract int GetMainPlayerCoins();
        public abstract void SaveMainPlayerPrize(int prize);
        public abstract int GetMainPlayerPrize();
            
        public abstract void SaveAvatarURL(string url);
        public abstract string GetAvatarURL();
        public abstract string GetMainPlayerEmail();
        public abstract string GetPrivacyPolicyURL();
        public abstract string[] GetFriendsId();
        public abstract void UpdateFriendsList();
        public abstract bool AvatarDataIsLocal();
        public abstract void SetAvatarDataIsLocal(bool isLocal);
        public abstract void CallOnApplicationPause(bool pauseStatus);
        public abstract bool IsLoggedIn();
        public abstract bool IsLoggedInWithPublishPermissions();
    }
}
