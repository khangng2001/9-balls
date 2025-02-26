using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

public class SocialExample : SocialEngine
{
    private bool requestForLogin = false;
    public SocialExample():base()
    {
        FacebookManager.instance.OnLoggedIn += FacebookManager_instance_OnLoggedIn;
        FacebookManager.instance.OnInitialized += () => 
            {
                CallFacebokInitialized();
            };
        if (!FacebookManager.instance.IsInitialized)
        {
            FacebookManager.instance.Init();
        }
    }

    public override void ShareOnFacebook()
    {
        Debug.Log("ShareOnFacebook");
        FacebookManager.instance.ShareLink();
    }
    public override void ShareOnTwitter()
    {
        Debug.Log("ShareOnTwitter");
    }
    public override void ShareOnGoogle()
    {
        Debug.Log("ShareOnGoogle");
    }
    public override void ShareByEmail()
    {
        Debug.Log("ShareByEmail");
    }
    public override void SaveMainPlayerName(string playerName)
    {
        DataManager.SetStringData("MainPlayerName", playerName);
    }
    public override string GetMainPlayerName()
    {
        string savedName = DataManager.GetStringData("MainPlayerName");
        if (string.IsNullOrEmpty(savedName))
        {
            savedName = "Guest " + Random.Range(1, 1000);
        }
        return savedName;
    }
    public override void SaveMainPlayerCoins(int playerCoins)
    {
        int pCoins = Mathf.Clamp(playerCoins, minCoinsCount, playerCoins);
        DataManager.SetInt("MainPlayerCoins", pCoins);
        #if Facebook
        FacebookManager.instance.SendScoreToFacebook(pCoins);
        #endif
    }
    public override int GetMainPlayerCoins()
    {
        int savedCoins = DataManager.GetInt("MainPlayerCoins");
        if (savedCoins == 0)
        {
            savedCoins = 1500;
            DataManager.SetInt("MainPlayerCoins", savedCoins);
        }
        return savedCoins;
    }

    public override void SaveMainPlayerPrize(int prize)
    {
        DataManager.SetIntData("MainPlayerPrize", prize);
    }
    public override int GetMainPlayerPrize()
    {
        int prize = DataManager.GetIntData("MainPlayerPrize");
        if (prize == 0)
        {
            prize = minOnLinePrize;
        }
        return prize;
    }

    public override void SaveAvatarURL(string url)
    {
        DataManager.SetStringData("MainPlayerAvatarURL", url);
    }
    public override string GetAvatarURL()
    {
        return DataManager.GetStringData("MainPlayerAvatarURL");
    }
    public override string GetMainPlayerEmail()
    {
        return "vagho.srapyan@gmail.com";
    }
    public override string GetPrivacyPolicyURL()
    {
        return "https://www.linkedin.com/in/vaghinak-srapyan-95016773";
    }
    public override string[] GetFriendsId()
    {
        return FacebookManager.instance.friendsId;
    }
    public override void SignUp(string email, string password)
    {
        CallSignUpEvent(LoginedState.Successful);
    }
    public override void Login(string email, string password)
    {
        CallLoginEvent(LoginedState.Successful);
    }
    public override void LoginWithFacebok()
    {
        if(FacebookManager.instance.IsInitialized)
        {
            requestForLogin = true;
            FacebookManager.instance.Login();
        }
    }
    public override void UpdateFriendsList()
    {
        friendsListIsUpdated = false;
        FacebookManager.instance.OnUpdatedFriendsList += FacebookManager_instance_OnUpdatedFriendsList;
        FacebookManager.instance.UpdateFriendsList();
    }
    void FacebookManager_instance_OnLoggedIn (LoginedState info)
    {
        if (info == LoginedState.Successful)
        {
            if (!FacebookManager.instance.IsLoggedInWithPublishPermissions)
            {
                //FacebookManager.instance.LoginWithPublishPermissions();

            }
            if (requestForLogin)
            {
                SetAvatarDataIsLocal(false);
            }
            mainPlayerId = FacebookManager.instance.mainUserId;
            CallLoginWithFacebokEvent(info);
            NetworkManager.network.StartUpdatePlayers();
        }
    }

    void FacebookManager_instance_OnUpdatedFriendsList ()
    {
        FacebookManager.instance.OnUpdatedFriendsList -= FacebookManager_instance_OnUpdatedFriendsList;
        friendsListIsUpdated = true;
    }
    public override bool AvatarDataIsLocal()
    {
        return DataManager.GetIntData("AvatarDataIsLocal") == 1;
    }
    public override void SetAvatarDataIsLocal(bool isLocal)
    {
        DataManager.SetIntData("AvatarDataIsLocal", isLocal ? 1 : 0);
        if (!isLocal)
        {
            ProductAvatar productAvatar = ProductAvatar.FindObjectOfType<ProductAvatar>();
            productAvatar.SaveIconURL("");
            productAvatar.SaveIconName("");
        }
    }
    public override void CallOnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            if (FacebookManager.instance.IsInitialized)
            {
                FacebookManager.instance.Activate();
            }
            else
            {
                FacebookManager.instance.Init();
            }
        }
    }
    public override bool IsLoggedIn()
    {
        return FacebookManager.instance.IsLoggedIn;
    }
    public override bool IsLoggedInWithPublishPermissions()
    {
        return FacebookManager.instance.IsLoggedInWithPublishPermissions;
    }
}
