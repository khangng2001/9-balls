using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NetworkManagement;
using System.Linq;

/// <summary>
/// Home menu manager, managed all logic dependent in home UI, network and player settings.
/// </summary>
public class HomeMenuManager : MonoBehaviour
{
	/// <summary>
	/// The product store independent default identifier. + for example 1000coins
	/// </summary>
	[SerializeField] private string storeIndependentId = "com.CompanyName.GameName";
    [SerializeField] private bool requestLoginWhenCreatingroom;
    [SerializeField] private PlayerProfileUI mainPlayerUI;
    [SerializeField] private PlayerProfileUI opponentUI;
    [SerializeField] private Text waitingOpponent;
    private Color waitingOpponentColor;
    [SerializeField] private Color opponentIsReadyColor = Color.green;
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField prizeInput;

    [SerializeField] private InputField findNameInput;
    [SerializeField] private InputField findPrizeInput;

    [SerializeField] private Texture2D aiImage;
    [SerializeField] private Texture2D opponentImage;
    [SerializeField] private LoginMenuManager loginManager;

    [SerializeField] private RoomsListManager roomsListManager;
    [SerializeField] private string playScene;
    [SerializeField] private string upgradeScene;
    [SerializeField] private Toggle onlinePlayersToggle;
    [SerializeField] private Toggle onlyFriendsToggle;

    [SerializeField] private RectTransform[] networkPanels;
    [SerializeField] private RectTransform friendsListContent;

    [SerializeField] private RectTransform[] hideOnRoomUIObjects;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button leftRoomButton;
    [SerializeField] private Button graphicInput;
    //[SerializeField] private GraphicMode graphicMode;
    [SerializeField] private GameObject graphicModeButton;
    [SerializeField] private HomeAdsManager homeAdsManager;
    public Texture2D[] defaultAvatars;
    [SerializeField] private Texture2D[] opponentsAvatars;
    [SerializeField] private Text playersInfoText;
    [SerializeField] private Text shareWithFriendsCoinsText;

    private bool roomCreated;
    private NetworkGameAdapter networkGameAdapter;
    private bool adsIsOpened = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void Start()
    {
        Resources.UnloadUnusedAssets();
    }
    void Awake()
    {    
       
        DataManager.SaveGameData();
		PurchasingEngine.StoreIndependentId = storeIndependentId;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

       

		graphicModeButton.SetActive(false);

        waitingOpponentColor = waitingOpponent.color;
        NetworkManager.initialized = true;
//        if (NetworkManager.absoluteURL.Contains("?data="))
//        {
//            networkGameAdapter = new AightBallPoolNetworkGameAdapter (this);
//            networkGameAdapter.GoToReplayFromSharedData();
//            return;
//        }
       

//        graphicModeButton.SetActive(graphicMode == GraphicMode.Universal);
//        if (graphicMode == GraphicMode.Universal)
//        {
//            AightBallPoolNetworkGameAdapter.is3DGraphics = DataManager.GetIntData("Is3DGraphics") == 0;
//            graphicInput.GetComponentInChildren<Text>().text = AightBallPoolNetworkGameAdapter.is3DGraphics ? "3D Graphics" : "2D Graphics";
//            graphicInput.onClick.AddListener(OnGraphicModeChanged);
//        }
//        else if (graphicMode == GraphicMode.ThreeD)
//        {
//            DataManager.SetIntData("Is3DGraphics", 0);
//            AightBallPoolNetworkGameAdapter.is3DGraphics = true;
//        }
//        else if (graphicMode == GraphicMode.TwoD)
//        {
//            DataManager.SetIntData("Is3DGraphics", 1);
//            AightBallPoolNetworkGameAdapter.is3DGraphics = false;
//        }

		DataManager.SetIntData ("Is3DGraphics", AightBallPoolNetworkGameAdapter.is3DGraphics ? 0 : 1);

            
        ProductsManagement productsManagement = ProductsManagement.FindObjectOfType<ProductsManagement>();
        productsManagement.transform.Find("Products3D").gameObject.SetActive(AightBallPoolNetworkGameAdapter.is3DGraphics);
        productsManagement.transform.Find("Products2D").gameObject.SetActive(!AightBallPoolNetworkGameAdapter.is3DGraphics);


        waitingOpponent.gameObject.SetActive(false);
        roomCreated = false;
       
        networkGameAdapter = new AightBallPoolNetworkGameAdapter(this);
        NetworkManager.network.SetAdapter(networkGameAdapter);
        #region event

        NetworkManager.OnMainPlayerLoaded += NetworkManager_OnMainPlayerLoaded;
        NetworkManager.OnRandomPlayerLoaded += NetworkManager_OnRandomPlayerLoaded;
        NetworkManager.OnFriendsAndRandomPlayersLoaded += NetworkManager_OnFriendsAndRandomPlayersLoaded;

        RoomsListManager.OnSelecRoom += RoomsListManager_OnSelecPlayerProfile;
        nameInput.onEndEdit.AddListener((string playerName) =>
            {
                if (playerName != "Guest")
                {
                    networkGameAdapter.OnUpdateMainPlayerName(playerName);
                    NetworkManager.mainPlayer.UpdateName(playerName);
                    mainPlayerUI.SetPlayer(NetworkManager.mainPlayer);
                }
            });
        prizeInput.onEndEdit.AddListener((string playerPrize) =>
            {
                int prize = NetworkManager.social.minOnLinePrize;
                if (string.IsNullOrEmpty(playerPrize))
                {
                    prizeInput.text = prize + "";
                }
                else
                {
                    if (NetworkManager.mainPlayer != null)
                    {
                        if (int.TryParse(playerPrize, out prize) && prize != 0)
                        {
                            if (prize < NetworkManager.social.minOnLinePrize)
                            {
                                prize = NetworkManager.social.minOnLinePrize;
                                prizeInput.text = prize + "";
                            }
                            else if (prize > NetworkManager.mainPlayer.coins)
                            {
                                prize = NetworkManager.mainPlayer.coins;
                                prizeInput.text = prize + "";
                            }
                            UpdatePrize(prize);
                            NetworkManager.social.SaveMainPlayerPrize(prize);
                        }
                        else
                        {
                            prize = NetworkManager.social.minOnLinePrize;
                            prizeInput.text = prize + "";
                        }
                    }
                    else
                    {
                        prizeInput.text = prize + "";
                    }
                }
            });


        NetworkManager.network.OnNetwork += NetworkManager_network_OnNetwork;
        ProductAvatar productAvatar = productsManagement.GetComponentInChildren<ProductAvatar>();

        string avatarName = productAvatar.GetIconName();
        string avatarURL = productAvatar.GetIconURL();

        if (!NetworkManager.social.AvatarDataIsLocal())
        {
            ProductAvatar.OnSetParameters += ProductAvatar_OnSetParameters;
        }

        NetworkManager.social.OnFacebokInitialized += ChackFacebooLoginedState1;
        #endregion

       
        TriggerNetworkPanels(NetworkManager.network.state);
       
        if ((!string.IsNullOrEmpty(avatarName) || !string.IsNullOrEmpty(avatarURL)) && NetworkManager.social.AvatarDataIsLocal())
        {
            OnAvatarInitialized(avatarName, avatarURL);
        }
        else
        {
            StartCoroutine(NetworkManager.LoadMainPlayer((Texture2D)mainPlayerUI.avatarImage.texture));
        }
        NetworkManager.network.Resset();
        UpdatePlayersList();
        adsIsOpened = false;


    }

    void OnEnable()
    {
        FacebookManager.OnShareWithFriend += ShareWithFriendHandler;
    }

    void OnDisable()
    {
        FacebookManager.OnShareWithFriend -= ShareWithFriendHandler;
        RoomsListManager.OnSelecRoom -= RoomsListManager_OnSelecPlayerProfile;
        ProductAvatar.OnSetParameters -= ProductAvatar_OnSetParameters;
        NetworkManager.Disable();
    }

    private void ShareWithFriendHandler(int newFriends, int allFriends)
    {
        if (newFriends != 0 && NetworkManager.mainPlayer != null)
        {
            int prize = newFriends * int.Parse(shareWithFriendsCoinsText.text);
            NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins + prize);
            mainPlayerUI.UpdateCoinsFromPlayer();
            string info = "You shared with \n" +
                          newFriends + " new friends \n" +
                          "You got " + prize + " coins.";
            InfoManager.Open("Rewarding", null, "", info);
        }
    }

    void ProductAvatar_OnSetParameters(System.Type type, object[] parameters)
    {
        if (type == typeof(ProductAvatar))
        {
            OnAvatarInitialized(parameters[0].ToString(), parameters[1].ToString());
        }
    }

    private void OnAvatarInitialized(string avatarName, string avatarURL)
    {
        //ProductsManagement productsManagement = ProductsManagement.FindObjectOfType<ProductsManagement>();
        ProductAvatar.OnSetParameters -= ProductAvatar_OnSetParameters;
        if (!string.IsNullOrEmpty(avatarName))
        {
            StartCoroutine(NetworkManager.SetMainPlayerImage(FindAvatarImageByName(avatarName)));
        }
        else if (!string.IsNullOrEmpty(avatarURL))
        {
            StartCoroutine(NetworkManager.SetMainPlayerImage(avatarURL));
        }
    }

    public Texture2D FindAvatarImageByName(string avatarName)
    {
        foreach (var item in defaultAvatars)
        {
            if (item.name == avatarName)
            {
                return item;
            }
        }
        return null;
    }

    public void OnGraphicModeChanged()
    {
        AightBallPoolNetworkGameAdapter.is3DGraphics = !AightBallPoolNetworkGameAdapter.is3DGraphics;
        graphicInput.GetComponentInChildren<Text>().text = AightBallPoolNetworkGameAdapter.is3DGraphics ? "3D Graphics" : "2D Graphics";
        DataManager.SetIntData("Is3DGraphics", AightBallPoolNetworkGameAdapter.is3DGraphics ? 0 : 1);
    }

    public void UpdatePrize(int prize)
    {
        networkGameAdapter.OnUpdatePrize(prize);
        NetworkManager.mainPlayer.prize = prize;
        mainPlayerUI.SetPlayer(NetworkManager.mainPlayer);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (Application.isEditor)
        {
            return;
        }
        if (!pauseStatus)
        {
            adsIsOpened = false;
            NetworkManager.social.CallOnApplicationPause(pauseStatus);
        }
    }

    private void NetworkManager_network_OnNetwork(NetworkState state)
    {
        if (state == NetworkState.Connected)
        {
            Debug.Log("LoadMainPlayer in Connected");
            StartCoroutine(NetworkManager.LoadMainPlayer((Texture2D)mainPlayerUI.avatarImage.texture));
            UpdatePlayersList();
        }
        else if (state == NetworkState.OpponentReadToPlay)
        {
            CheckOpponentImageByName(NetworkManager.opponentPlayer);
            opponentUI.SetPlayer(NetworkManager.opponentPlayer);
            waitingOpponent.gameObject.SetActive(true);
            waitingOpponent.text = "Your Opponent is ready...";
            waitingOpponent.color = opponentIsReadyColor;


            if (opponentUI.player != null)
            {
                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
                #endif
                GetComponent<AudioSource>().Play();
                //GoToPLayWithPlayer(opponentUI);
            }
        }
        else if (state == NetworkState.JoinedToRoom || state == NetworkState.CreatedRoom)
        {
            waitingOpponent.gameObject.SetActive(true);
            waitingOpponent.text = "Waiting for your Opponent...";
            waitingOpponent.color = waitingOpponentColor;
        }
        else
        {
            waitingOpponent.gameObject.SetActive(false);
        }
        TriggerNetworkPanels(state);
    }


    private void TriggerNetworkPanels(NetworkManagement.NetworkState state)
    {
        if (state == NetworkState.OpponentReadToPlay)
        {
            return;
        }
        UnityEngine.Debug.Log("state " + state);
       
        foreach (var item in hideOnRoomUIObjects)
        {
            item.gameObject.SetActive(!(state == NetworkState.CreatedRoom || state == NetworkState.JoinedToRoom));
        }
        foreach (var item in networkPanels)
        {
            item.gameObject.SetActive(!(state == NetworkState.Disconnected || state == NetworkState.LostConnection));
        }

        if (state == NetworkState.JoinedToRoom || state == NetworkState.CreatedRoom)
        {
            roomCreated = true;
            createRoomButton.gameObject.SetActive(false);
            leftRoomButton.gameObject.SetActive(true);
        }
        else if (state == NetworkState.LeftRoom)
        {
            roomCreated = false;
            createRoomButton.gameObject.SetActive(true);
            leftRoomButton.gameObject.SetActive(false);
        }
    }

    void NetworkManager_OnFriendsAndRandomPlayersLoaded(PlayerProfile[] players)
    {
        FindPlayersByNamePrizeIsOnlineAndIsFriend();
    }

    void RoomsListManager_OnSelecPlayerProfile(NetworkManagement.Room room)
    {
        PlayerProfile player = room.mainPlayer;
        if (player.prize <= NetworkManager.mainPlayer.coins && player.state == PlayerState.Online)
        {
            CheckOpponentImageByName(player);
            opponentUI.SetPlayer(player);
        }
    }

    void NetworkManager_OnRandomPlayerLoaded(NetworkManagement.PlayerProfile player)
    {
        CheckOpponentImageByName(player);
        opponentUI.SetPlayer(player);
    }

    void NetworkManager_OnMainPlayerLoaded(NetworkManagement.PlayerProfile player)
    {
        int prize = NetworkManager.social.GetMainPlayerPrize();
        if (prize < NetworkManager.social.minOnLinePrize)
        {
            prize = NetworkManager.social.minOnLinePrize;
            NetworkManager.social.SaveMainPlayerPrize(prize);
        }
        if (prize > NetworkManager.mainPlayer.coins)
        {
            prize = NetworkManager.mainPlayer.coins;
            NetworkManager.social.SaveMainPlayerPrize(prize);
        }
        UpdatePrize(prize);

        networkGameAdapter.OnMainPlayerLoaded(0, player.userName, player.coins, player.image, player.imageURL, player.prize);
        mainPlayerUI.SetPlayer(player);
        nameInput.text = player.userName;
        prizeInput.text = player.prize + "";
    }

    public void GoToReplay()
    {
        networkGameAdapter.GoToReplay();
    }

    public void GoToPlayWithAI()
    {
        if (DataManager.GetIntData("FirstTimeGoToPLayWithAIorHotSeat") == 0)
        {
            DataManager.SetIntData("FirstTimeGoToPLayWithAIorHotSeat", 1);
            InfoManager.Open("FirstTimePlayInFunMode", this, "GoToPlayWithAI");
            return;
        }
        if (!NetworkManager.mainPlayer.canPlayOffline)
        {
            UpdatePrize(NetworkManager.mainPlayer.coins);
        }
        string opponentrName = "AI Player";
        Texture2D opponentImage = aiImage;
        int opponentCoins = Random.Range(2 * NetworkManager.mainPlayer.prize, 10 * NetworkManager.mainPlayer.prize);

        if (opponentsAvatars != null && opponentsAvatars.Length > 0)
        {
            opponentImage = opponentsAvatars[Random.Range(0, opponentsAvatars.Length)];
            while (opponentsAvatars.Length > 1 && opponentImage == NetworkManager.mainPlayer.image)
            {
                opponentImage = opponentsAvatars[Random.Range(0, opponentsAvatars.Length)];
            }
            opponentrName = opponentImage.name;
        }
        networkGameAdapter.OnGoToPlayWithAI(1, opponentrName, opponentCoins, opponentImage, "");
    }

    public void GoToPlayHotSeatMode()
    {
        if (DataManager.GetIntData("FirstTimeGoToPLayWithAIorHotSeat") == 0)
        {
            DataManager.SetIntData("FirstTimeGoToPLayWithAIorHotSeat", 1);
            InfoManager.Open("FirstTimePlayInFunMode", this, "GoToPlayHotSeatMode");
            return;
        }

        if (!NetworkManager.mainPlayer.canPlayOffline)
        {
            UpdatePrize(NetworkManager.mainPlayer.coins);
        }
        string opponentrName = "Other Player";
        Texture2D randomImage = opponentImage;
        int opponentCoins = NetworkManager.mainPlayer.coins;
        if (opponentsAvatars != null && opponentsAvatars.Length > 0)
        {
            randomImage = opponentsAvatars[Random.Range(0, opponentsAvatars.Length)];
            while (opponentsAvatars.Length > 1 && randomImage == NetworkManager.mainPlayer.image)
            {
                randomImage = opponentsAvatars[Random.Range(0, opponentsAvatars.Length)];
            }
            opponentrName = randomImage.name;
        }
        networkGameAdapter.OnGoToPlayHotSeatMode(1, opponentrName, opponentCoins, randomImage, "");
    }

    public void GoToPLayWithPlayer(PlayerProfileUI playerUI)
    {
//        if (NetworkManager.social.IsLoggedIn() && !NetworkManager.social.IsLoggedInWithPublishPermissions())
//        {
//            if (!ChackFacebooLoginedState(2))
//            {
//                return;
//            }
//        }
        if (opponentUI.player != null)
        {
            if (!requestLoginWhenCreatingroom || LoginManager.logined || NetworkManager.social.IsLoggedInWithPublishPermissions() || NetworkManager.social.IsLoggedIn())
            {
                if (playerUI && playerUI.player != null)
                {
                    if (NetworkManager.mainPlayer.canPlayOnLine)
                    {
                        networkGameAdapter.OnGoToPLayWithPlayer(playerUI.player);
                    }
                    else
                    {
                        OfferAd();
                    }
                }
            }
            else
            {
                loginManager.OpenLoginWindow();
            }
        }
        else if (NetworkManager.players != null && NetworkManager.players.Length > 0)
        {
            OfferAd();
        }
    }

    public void GoToPlay()
    {
        if (NetworkManager.mainPlayer != null)
        {
            NetworkManager.mainPlayer.state = PlayerState.Playing;
            mainPlayerUI.SetPlayer(NetworkManager.mainPlayer);
        }
        SceneManager.LoadScene(playScene);
    }

    public void CancleGoToPlay(PlayerState opponentState, string playerId)
    {
        NetworkManager.FindPlayer(playerId).state = opponentState;
        NetworkManager.UpdatePlayers();
        StartCoroutine(NetworkManager.LoadRandomPlayer());
    }

    public void ChackFacebooLoginedState1()
    {
        //ChackFacebooLoginedState(1);
        NetworkManager.social.OnFacebokInitialized -= ChackFacebooLoginedState1;
    }

    public bool ChackFacebooLoginedState(int step)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (DataManager.GetIntData("FirstTimeGoToPLayWithPlayer" + step) == 0 && (!NetworkManager.social.IsLoggedIn() || !NetworkManager.social.IsLoggedInWithPublishPermissions()))
            {
                DataManager.SetIntData("FirstTimeGoToPLayWithPlayer" + step, 1);
                loginManager.OpenLoginWindow();
                return false;
            }
        }
        return true;
    }

    public void GoToUpgrade()
    {
//        if (NetworkManager.social.IsLoggedIn() && !NetworkManager.social.IsLoggedInWithPublishPermissions())
//        {
//            if (!ChackFacebooLoginedState(2))
//            {
//                return;
//            }
//        }
        SceneManager.LoadScene(upgradeScene);
    }

    public void OpenAds(Text prize)
    {
        if (adsIsOpened)
        {
            return;
        }
        adsIsOpened = true;
        NetworkManager.purchasing.ads.ShowAds(
            delegate (AdsShowResult result)
            {
                if (result == AdsShowResult.Finished)
                {
                    int coins = int.Parse(prize.text);
                    NetworkManager.mainPlayer.prize = Mathf.Clamp(NetworkManager.mainPlayer.prize, NetworkManager.social.minOnLinePrize, NetworkManager.mainPlayer.prize);
                    prizeInput.text = NetworkManager.mainPlayer.prize + "";

                    NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins + coins);
                    mainPlayerUI.UpdateCoinsFromPlayer();
                    mainPlayerUI.UpdatePrizeFromPlayer();

                    string info = "You watched one video \n" +
                                  "You got " + coins + " coins.";
                    InfoManager.Open("Rewarding", null, "", info);
                }
                adsIsOpened = false;
            }
        );
    }

    public void CreateRoom()
    {
//        if (NetworkManager.social.IsLoggedIn() && !NetworkManager.social.IsLoggedInWithPublishPermissions())
//        {
//            if (!ChackFacebooLoginedState(2))
//            {
//                return;
//            }
//        }
        if (!requestLoginWhenCreatingroom || LoginManager.logined || LoginManager.loginedFacebook)
        {
            if (roomCreated)
            {
                NetworkManager.network.LeftRoom();
            }
            else
            {
                if (NetworkManager.mainPlayer.canPlayOnLine)
                {
                    NetworkManager.mainPlayer.prize = Mathf.Clamp(NetworkManager.mainPlayer.prize, NetworkManager.social.minOnLinePrize, NetworkManager.mainPlayer.prize);
                    prizeInput.text = NetworkManager.mainPlayer.prize + "";
                        
                    NetworkManager.network.CreateRoom();
                }
                else
                {
                    OfferAd();
                }
            }
        }
        else
        {
            loginManager.OpenLoginWindow();
        }
    }

    private void OfferAd()
    {
        homeAdsManager.OfferAd();
        StartCoroutine(RedPrizeTextAndReturn());
    }

    IEnumerator RedPrizeTextAndReturn()
    {
        prizeInput.transform.Find("Text").GetComponent<Text>().color = Color.red;
        mainPlayerUI.SetPrizeColor(Color.red);
        yield return new WaitForSeconds(3.0f);
        prizeInput.transform.Find("Text").GetComponent<Text>().color = Color.black;
        mainPlayerUI.SetPrizeColor(Color.black);
    }

    public void ShareOnFacebook()
    {
        if (NetworkManager.social.IsLoggedIn())
        {
            NetworkManager.social.ShareOnFacebook();
        }
        else
        {
            loginManager.OpenLoginWindow();
        }
    }

    public void ShareOnTwitter()
    {
        if (LoginManager.logined || LoginManager.loginedFacebook)
        {
            NetworkManager.social.ShareOnTwitter();
        }
        else
        {
            loginManager.OpenLoginWindow();
        }
    }

    public void ShareOnGoogle()
    {
        if (LoginManager.logined || LoginManager.loginedFacebook)
        {
            NetworkManager.social.ShareOnGoogle();
        }
        else
        {
            loginManager.OpenLoginWindow();
        }
    }

    public void ShareByEmail()
    {
        if (LoginManager.logined || LoginManager.loginedFacebook)
        {
            NetworkManager.social.ShareByEmail();
        }
        else
        {
            loginManager.OpenLoginWindow();
        }
    }

    void CheckOpponentImageByName(PlayerProfile player)
    {
        if (player != null && !string.IsNullOrEmpty(player.imageName))
        {
            Texture2D playerImage = FindAvatarImageByName(player.imageName);
            if (playerImage)
            {
                player.SetImage(playerImage);
            }
        }
    }

    public void FindPlayersByNamePrizeIsOnlineAndIsFriend()
    {
        friendsListContent.localPosition = Vector3.zero;
        onlinePlayersToggle.image.color = onlinePlayersToggle.isOn ? Color.green : Color.Lerp(Color.black, Color.white, 0.8f);
        onlyFriendsToggle.GetComponentInChildren<Text>().text = onlyFriendsToggle.isOn ? "Friends" : "Players";
        playersInfoText.text = onlyFriendsToggle.GetComponentInChildren<Text>().text + " online";
        int prize;
        int.TryParse(findPrizeInput.text, out prize);
        NetworkManagement.PlayerProfile[] playerProfiles = NetworkManager.FindPlayers(findNameInput.text, prize, onlinePlayersToggle.isOn, onlyFriendsToggle.isOn);
        NetworkManagement.Room[] rooms = new NetworkManagement.Room[playerProfiles.Length];
        for (int i = 0; i < rooms.Length; i++)
        {
            List<PlayerProfile> players = new List<PlayerProfile>(0);
            players.Add(playerProfiles[i]);
            CheckOpponentImageByName(playerProfiles[i]);
            rooms[i] = new NetworkManagement.Room(i, prize, players);
        }

        roomsListManager.UpdateRooms(rooms);

        //roomsListManager.UpdatePlayers(playerProfiles);
    }

    public void UpdatePlayersList()
    {
        NetworkManager.UpdatePlayers();
        StartCoroutine(NetworkManager.LoadRandomPlayer());
        StartCoroutine(NetworkManager.LoadFriendsAndRandomPlayers(50));
    }
}
