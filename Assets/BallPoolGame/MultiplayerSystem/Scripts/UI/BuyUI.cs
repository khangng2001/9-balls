using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
   /// <summary>
   ///Buy UI, This window is used for purchase
   /// </summary>
    public class BuyUI : MonoBehaviour
    {
        [SerializeField] private ProductProfileUI productProfileUI;
        [SerializeField] private UpgradeMenuManager upgradeMenuManager;
        [SerializeField] private Text productCountText;
        [SerializeField] private Text buyOrAddText;
        private ProductProfileUI currentProductProfileUI;
        private ProductProfile currentProductProfile;
        private int productCount = 1;
        private int coinsCount = 1;
        [SerializeField] private Image priceImage;
        [SerializeField] private Text priceText;
        [SerializeField] private Sprite coinsSprite;
        [SerializeField] private Sprite dollarSprite;
        [SerializeField] private RectTransform countPanel;
        private bool windowIsOpened = false;

        void Awake()
        {
            if (!windowIsOpened)
            {
                CloseWindow();
            }
        }
        void OnEnable()
        {
            NetworkManager.purchasing.OnPurchased += NetworkManager_purchasing_OnPurchased;
        }
        void OnDisable()
        {
            NetworkManager.purchasing.OnPurchased -= NetworkManager_purchasing_OnPurchased;
        }
        public void OpenAdWindow(ProductProfile adProfile)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                InfoManager.Open("NoInternet", null, "");
                CloseWindow();
                return;
            }
            windowIsOpened = true;
            buyOrAddText.text = "Add";
            countPanel.gameObject.SetActive(false);
            priceImage.sprite = coinsSprite;
            currentProductProfile = null;
            currentProductProfileUI = this.productProfileUI;
            this.productProfileUI.SetProduct(adProfile);
            gameObject.SetActive(true);
            SetProductCount(1, productProfileUI.product);
        }
        public void OpenCoinsWindow(ProductProfile coinsProfile, ProductProfileUI productProfileUI, int productCount)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                InfoManager.Open("NoInternet", null, "");
                CloseWindow();
                return;
            }
            windowIsOpened = true;
            buyOrAddText.text = "Buy";
            countPanel.gameObject.SetActive(!coinsProfile.data.oneTimeBought && coinsProfile.data.maxCount != 1);
            int price = productProfileUI.product.data.price;
            int coins = coinsProfile.data.PriceFromName();
            int pCount = 1;
            while (coins * pCount + NetworkManager.mainPlayer.coins < price * productCount)
            {
                pCount++;
            }
            SetCoinsCount(pCount, coinsProfile);
            priceImage.sprite = dollarSprite;
 
            currentProductProfileUI = productProfileUI;
            currentProductProfile = coinsProfile;

            this.productProfileUI.SetProduct(currentProductProfile);
            gameObject.SetActive(true);
        }
        public void OpenWindow(ProductProfileUI productProfileUI)
        {
            if ((productProfileUI.product.data.isRealMoney || productProfileUI.product.data.price > NetworkManager.mainPlayer.coins)  && Application.internetReachability == NetworkReachability.NotReachable)
            {
                InfoManager.Open("NoInternet", null, "");
                CloseWindow();
                return;
            }
            windowIsOpened = true;

            buyOrAddText.text = productProfileUI.product.data.price > 0? "Buy":"Add";
            countPanel.gameObject.SetActive(!productProfileUI.product.data.oneTimeBought && productProfileUI.product.data.maxCount != 1);
           
            if (productProfileUI.product.data.isRealMoney)
            {
                priceImage.sprite = dollarSprite;
            }
            else
            {
                priceImage.sprite = coinsSprite;
            }
            currentProductProfile = null;
            currentProductProfileUI = productProfileUI;
            this.productProfileUI.SetProduct(currentProductProfileUI.product);
            gameObject.SetActive(true);

            if (productProfileUI.product.data.isRealMoney)
            {
                SetCoinsCount(1, productProfileUI.product);
            }
            else
            {
                SetProductCount(productCount, productProfileUI.product);
            }
           
        }
        void SetCoinsCount(int coinsCount, ProductProfile productProfile)
        {
            this.coinsCount = coinsCount;
            productCountText.text = coinsCount + "";
            if (productProfile.data.price > 0)
            {
                priceText.text = "$" + (productProfile.data.price * coinsCount).ToString() + "";
            }
        }
        void SetProductCount(int productCount, ProductProfile productProfile)
        {
            this.productCount = productCount;
            productCountText.text = productCount + "";
            if (productProfile.data.price != 0)
            {
                priceText.text = Mathf.Abs(productProfile.data.price * productCount).ToString() + "";
            }
        }
        public void ChangeProductCount(int value)
        {
            ProductProfile pp = currentProductProfile != null ? currentProductProfile : currentProductProfileUI.product;
            if (pp.data.isRealMoney)
            {
                coinsCount += value;
                coinsCount = Mathf.Clamp(coinsCount, 1, pp.data.maxCount == 0 ? coinsCount : pp.data.maxCount);
                SetCoinsCount(coinsCount, pp);
            }
            else
            {
                productCount += value;
                int maxCount = Mathf.Min(pp.data.maxCount == 0 ? productCount : pp.data.maxCount, (int)(10000.0 / (float)pp.data.price));
                Debug.Log(productCount + "   maxCount " + maxCount + "  " + pp.data.price + "  " + pp.data.name);
                productCount = Mathf.Clamp(productCount, 1, maxCount);
                SetProductCount(productCount, pp);
            }
        }
        public void CloseWindow()
        {
            if (NetworkManager.purchasing.PurchaseingInProcces())
            {
                return;
            }
            Debug.Log("CloseWindow ");
            gameObject.SetActive(false);
            productCount = 1;
            coinsCount = 1;
        }
        private ProductProfile pp;
        private int count;

		private void OpenFirstTimeBuy()
		{
			string infoText = "";
			#if UNITY_ANDROID
			infoText = "Recommended\n to enable Google Sync\n for saving purchasing info\n and coins.";
			#elif UNITY_IOS
			infoText = "Recommended\n to enable iCloud Drive\n for saving purchasing info\n and coins.";
			#endif

			InfoManager.Open("FirstTimeBuy", this, "PurchaseCoins", infoText);
		}
        public void Buy()
        {
            if (NetworkManager.purchasing.PurchaseingInProcces())
            {
                return;
            }
            if (productProfileUI.product != null && !NetworkManager.purchasing.Purchased(productProfileUI.product.data.id, productProfileUI.product.data.type, productProfileUI.product.data.name))
            {
                pp = currentProductProfile == null ? currentProductProfileUI.product : currentProductProfile;

                if (pp.data.isRealMoney)
                {
                    if (DataManager.GetIntData("FirsTimePurchase") == 0)
                    {
                        DataManager.SetIntData("FirsTimePurchase", 1);
                        count = coinsCount;
						OpenFirstTimeBuy ();
                    }
                    else
                    {
                        if (Application.internetReachability != NetworkReachability.NotReachable)
                        {
                            NetworkManager.Purchase(coinsCount, pp);
                        }
                        else
                        {
                            InfoManager.Open("NoInternet", null, "");
                        }
                    }
                }
                else
                {
                    if (DataManager.GetIntData("FirsTimePurchase") == 0)
                    {
                        DataManager.SetIntData("FirsTimePurchase", 1);
                        count = productCount;
						OpenFirstTimeBuy ();
                    }
                    else
                    {
                        NetworkManager.Purchase(productCount, pp);
                    }
                }
            }
        }

        public void PurchaseCoins()
        {
            NetworkManager.Purchase(count, pp);
        }
        void NetworkManager_purchasing_OnPurchased (ProductProfile productProfile, PurchasedState state)
        {
            if (state == PurchasedState.Successful)
            {
                if (productProfile == currentProductProfileUI.product)
                {
                    currentProductProfileUI.SetProduct(productProfile);
                    upgradeMenuManager.OnSelecProduct(currentProductProfileUI, productCount);
                    CloseWindow();
                }
                else
                {
                    OpenWindow(currentProductProfileUI);
                    Buy();
                }
            }
            else 
            {
                if (!productProfile.data.isRealMoney)
                {
                    ProductProfile coinProfile = upgradeMenuManager.GetCoinProfileByPrice(currentProductProfileUI.product.data.price * productCount);
                    if (coinProfile != null)
                    {
                        OpenCoinsWindow(coinProfile, currentProductProfileUI, productCount);
                    }
                    else
                    {
                        ProductProfile adProfile = upgradeMenuManager.GetAdProfile();
                        if (adProfile != null)
                        {
                            OpenAdWindow(adProfile);
                        }
                    }
                }
                else
                {
                    CloseWindow();
                }
            }
        }
    }
}
