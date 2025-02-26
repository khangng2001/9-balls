using System.Collections;
using System.Collections.Generic;

#if Facebook
using Facebook.Unity;
#endif
using System;
using BallPool;

namespace NetworkManagement
{
    public delegate void ShareWithFriendHandler(int newFriends,int allFriends);

    public class FacebookManager
    {
        public event System.Action OnInitialized;
        public event System.Action<LoginedState> OnLoggedIn;
        public event System.Action OnUpdatedFriendsList;
        public static event ShareWithFriendHandler OnShareWithFriend;

        private static FacebookManager _instance;

        public static FacebookManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FacebookManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the main player identifier.
        /// </summary>
        public string mainUserId{ get; private set; }

        /// <summary>
        /// Gets the friends identifier.
        /// </summary>
        public string[] friendsId{ get; private set; }

        public void Init()
        {
            #if Facebook
            FB.Init(this.OnInitComplete, this.OnHideUnity);
            #else
            OnInitialized();
            #endif
        }

        public bool IsInitialized
        {
            get
            {
                #if Facebook
                return FB.IsInitialized;
                #else
                return true;
                #endif
            }
        }

        private bool forLoginWithPublishPermissions = false;

        public void LoginWithPublishPermissions()
        {
            #if Facebook
            if (!IsLoggedInWithPublishPermissions)
            {
                if (!FB.IsInitialized)
                {
                    forLoginWithPublishPermissions = true;
                    FB.Init(this.OnInitComplete, this.OnHideUnity);
                }
                else
                {
                    if (!tryLogInWithPublishPermissions)
                    {
                        tryLogInWithPublishPermissions = true;
                        FB.LogInWithPublishPermissions(new List<string> { "publish_actions" }, 
                            delegate(ILoginResult result)
                            {
                                tryLogInWithPublishPermissions = false;
                            }
                        );
                    }
                }
            }
            #endif
        }

        public void Login()
        {
            #if Facebook
            FB.LogInWithReadPermissions(new List<string> { "public_profile", "email", "user_friends" }, 
                delegate(ILoginResult result)
                {
                    if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
                    {
                        OnLoginCompleteSuccessful();
                    }
                    else
                    {
                        OnLoggedIn(LoginedState.Unsuccessful);
                    }
                }
            );

            return;
            if (FB.IsLoggedIn && !IsLoggedInWithPublishPermissions)
            {
                // LoginWithPublishPermissions();
            }
            else
            {
                FB.LogInWithReadPermissions(new List<string> { "public_profile", "email", "user_friends" }, 
                    delegate(ILoginResult result)
                    {
                        if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
                        {
                            OnLoginCompleteSuccessful();
                        }
                        else
                        {
                            OnLoggedIn(LoginedState.Unsuccessful);
                        }
                    }
                );
            }
            #else
            OnLoggedIn(LoginedState.Successful);
            DataManager.SetIntData("LogInImitate", 1);
            #endif
        }

        public void ShareLink()
        {
            #if Facebook
            List<string> currentAccessToken = ((List<string>)AccessToken.CurrentAccessToken.Permissions);

            if (!IsLoggedIn || !(currentAccessToken.Contains("public_profile") && currentAccessToken.Contains("user_friends")))
            {
                FB.LogInWithReadPermissions(new List<string> { "public_profile", "email", "user_friends" }, 
                    delegate(ILoginResult result)
                    {
                        if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
                        {
                            AppRequest();
                        }
                    }
                );
            }
            else
            {
                AppRequest();
            } 
            #else
            AppRequest();
            #endif
        }

        private void AppRequest()
        {
            #if Facebook
            string mesage = "New 8 Ball Pool Is Cool! Check it out.";
            string tittle = "New 8 Ball Pool.";
            string data = "I have a " + NetworkManager.mainPlayer.coins + " coins";

            FB.AppRequest(mesage, null, null, null, 0, data, tittle, 

                delegate (IAppRequestResult result)
                {
                    if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
                    {
                        int newFriendsCount = 0;
                        int allFriendsCount = 0;
                        foreach (var item in result.To)
                        {
                            bool newFriend = DataManager.GetIntData("SharedFriend_" + item) == 0;
                            DataManager.SetIntData("SharedFriend_" + item, 1);
                            if (newFriend)
                            {
                                newFriendsCount++;
                            }
                            allFriendsCount++;
                        }
                        if (OnShareWithFriend != null)
                        {
                            OnShareWithFriend(newFriendsCount, allFriendsCount);
                        }
                    }
                }
            );
            #else
            int friendsCount = UnityEngine.Random.Range(0, 10);
            OnShareWithFriend(UnityEngine.Random.Range(0, friendsCount), friendsCount);
            #endif
        }

        public bool IsLoggedIn
        {
            get
            {
                #if Facebook
                return FB.IsLoggedIn;
                #else
                return  DataManager.GetIntData("LogInImitate") == 1;
                #endif
            }
        }

        public bool IsLoggedInWithPublishPermissions
        {
            get
            {
                #if Facebook && !UNITY_EDITOR
                return FB.IsInitialized && ((List<string>)AccessToken.CurrentAccessToken.Permissions).Contains("publish_actions");
                #else
                return false;
                #endif
            }
        }

        public void Activate()
        {
            #if Facebook
            FB.ActivateApp();
            #endif
        }

        public void UpdateFriendsList()
        {
            #if Facebook
            if (FB.IsLoggedIn)
            {
                FB.API("/me/friends", HttpMethod.GET, OnFriendsInfo);
            }
            else
            {
                if (OnUpdatedFriendsList != null)
                {
                    OnUpdatedFriendsList();
                }
            }
            #else
            if (OnUpdatedFriendsList != null)
            {
                OnUpdatedFriendsList();
            }
            #endif
        }

        #if Facebook
        private void OnInitComplete()
        {
            if (FB.IsLoggedIn)
            {
                OnLoginCompleteSuccessful();
            }
            if (OnInitialized != null)
            {
                OnInitialized();
            }
        }

        private bool tryLogInWithPublishPermissions = false;

        private void OnHideUnity(bool isGameShown)
        {
            return;
            if (isGameShown && forLoginWithPublishPermissions)
            {
                DebugManager.DebugLog("forLoginWithPublishPermissions ");
                forLoginWithPublishPermissions = false;
                if (!tryLogInWithPublishPermissions)
                {
                    tryLogInWithPublishPermissions = true;
                    FB.LogInWithPublishPermissions(new List<string> { "publish_actions" },
                        delegate(ILoginResult result)
                        {
                            tryLogInWithPublishPermissions = false;
                        }
                    );
                }
            }
        }


        private void OnLoginCompleteSuccessful()
        {
            UnityEngine.Debug.Log("OnLoginComplete");

            if (!FB.IsLoggedIn)
            {
                return;
            }
            FB.API("/me?fields=id,first_name,picture.width(256).height(256),friends", HttpMethod.GET, OnSetMainPlayerInfo); 
        }

        private void OnFriendsInfo(IGraphResult result)
        {
            if (IsInitialized && IsLoggedIn)
            {
                object friendsH;
                if (result.ResultDictionary.TryGetValue("data", out friendsH) && friendsH != null)
                {
                    object[] friendsObj = ((List<object>)friendsH).ToArray();
                    friendsId = new string[friendsObj.Length];

                    for (int i = 0; i < friendsObj.Length; i++)
                    {
                        object friendObj = friendsObj[i];
                        Dictionary<string,object> friendDir = (Dictionary<string,object>)friendObj;

                        friendsId[i] = friendDir["id"].ToString();
                    }
                }
                if (OnUpdatedFriendsList != null)
                {
                    OnUpdatedFriendsList();
                }
            }
        }

        private void OnSetMainPlayerInfo(IGraphResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                return;
            }
            string outName;
            string outID;
            object outAvatar;
            //object outUser;
            object outFriends;
            // int cScore = 0;


            if (result.ResultDictionary.TryGetValue("first_name", out outName))
            {
                NetworkManager.social.SaveMainPlayerName(outName);
            }

            if (result.ResultDictionary.TryGetValue("id", out outID))
            {
                mainUserId = outID;
                DebugManager.DebugLog("mainUserId " + mainUserId);
            }

            if (result.ResultDictionary.TryGetValue("first_name", out outName))
            {
                NetworkManager.social.SaveMainPlayerName(outName);
            }

            if (result.ResultDictionary.TryGetValue("picture", out outAvatar) && outAvatar != null)
            {
                Dictionary<string, object> avatarData = (Dictionary<string, object>)(((Dictionary<string, object>)outAvatar)["data"]);
                string avatarURL = (string)avatarData["url"];
                NetworkManager.social.SaveAvatarURL(avatarURL);
            }
            // mainUserId = userId;


//            if (result.ResultDictionary.TryGetValue("scores", out outUser) && outUser != null)
//            {
//                object userH;
//                Dictionary<string, object> userData = (Dictionary<string, object>)outUser;
//                if (userData.TryGetValue("data", out userH))
//                {
//                    object userObj = ((List<object>)userH)[0];
//                    Dictionary<string,object> userDir = (Dictionary<string,object>)userObj;
//                    cScore = Convert.ToInt32(userDir["score"]);
//
//                    Dictionary<string, object> user = (Dictionary<string, object>)userDir["user"];
//
//                    //string userName = user["name"].ToString();
//                    string userId = user["id"].ToString();
//
//                    mainUserId = userId;
//                }
//            }

            if (result.ResultDictionary.TryGetValue("friends", out outFriends) && outFriends != null)
            {
                object friendsH;
                Dictionary<string, object> friendsData = (Dictionary<string, object>)outFriends;

                if (friendsData.TryGetValue("data", out friendsH))
                {
                    object[] friendsObj = ((List<object>)friendsH).ToArray();
                    friendsId = new string[friendsObj.Length];
                    for (int i = 0; i < friendsObj.Length; i++)
                    {
                        object friendObj = friendsObj[i];
                        Dictionary<string,object> friendDir = (Dictionary<string,object>)friendObj;
                        friendsId[i] = friendDir["id"].ToString();
                    }
                }
            }
//            if (cScore == 0)
//            {
//                cScore = NetworkManager.social.GetMainPlayerCoins();
//            }
//
//            NetworkManager.social.SaveMainPlayerCoins(cScore);

            if (OnLoggedIn != null)
            {
                OnLoggedIn(LoginedState.Successful);
            }
        }

       
        public void SendScoreToFacebook(int cScore)
        {
            return;
            if (IsInitialized && IsLoggedIn)
            {
                if (IsLoggedInWithPublishPermissions)
                {
                    SendScore(cScore);
                }
                else
                {
                    if (!tryLogInWithPublishPermissions)
                    {
                        tryLogInWithPublishPermissions = true;
                        FB.LogInWithPublishPermissions(new List<string> { "publish_actions" }, 
                            delegate(ILoginResult result)
                            {
                                if (IsLoggedInWithPublishPermissions)
                                {
                                    SendScore(cScore);
                                }
                                tryLogInWithPublishPermissions = false;
                            }
                        );
                    }
                }
            }
        }



        private void SendScore(int cScore)
        {
            var query = new Dictionary<string, string>();
            query.Add("score", cScore.ToString());
            FB.API("/me/scores", HttpMethod.POST,
                delegate(IGraphResult result)
                {
                    DebugManager.DebugLog("PostScore Result: " + result.RawResult + "  " + cScore);
                },
                query
            );
        }
        #endif
    }
}
