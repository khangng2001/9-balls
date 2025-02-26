using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkManagement
{
    public delegate void ViewAdsHandler(AdsShowResult result);
    public enum AdsShowResult {Finished, Skipped , Failed};

    public interface Ads
    {
        void ShowAds(ViewAdsHandler handler);
    }
    /// <summary>
    /// The engine used to buy
    /// </summary>
    public abstract class PurchasingEngine
    {
		public static string StoreIndependentId;
        public event PurchasedHandler OnPurchased;
        public Ads ads;
        protected bool adsIsOpened = false;

        public PurchasingEngine()
        {

        }
        public abstract bool PurchaseingInProcces();

        public abstract void PurchaseWithRealMoney(int productCount, NetworkManagement.ProductProfile productProfile);
        public abstract void PurchaseFromAds(int productCount, NetworkManagement.ProductProfile productProfile);

        public virtual bool Purchased(int productId, string productType, string productName)
        {
            return DataManager.GetInt("Purchased: " + productId + "_" + productType + "_" + productName) == 1;
        }
        public virtual void SetPurchased(ProductProfile productProfile)
        {
            DataManager.SetInt("Purchased: " + productProfile.data.id + "_" + productProfile.data.type + "_" + productProfile.data.name, 1);
        }
        public virtual void ForceSetPurchased(int productId, string productType, string productName)
        {
            DataManager.SetInt("Purchased: " + productId + "_" + productType + "_" + productName, 1);
        }

        public void Purchase(int productCount, NetworkManagement.ProductProfile productProfile)
        {
            if (productProfile.data.isRealMoney)
            {
                PurchaseWithRealMoney(productCount, productProfile);
                return;
            }
            if (productProfile.data.price < 0)
            {
                PurchaseFromAds(productCount, productProfile);
                return;
            }

            if (NetworkManager.mainPlayer.coins < productProfile.data.price * productCount)
            {
                CallPurchasedEvend(productProfile, PurchasedState.Unsuccessful);
                Debug.LogWarning("You have a little money to buy this product");
                return;
            }
            //Successful purchase with coins.
            if (productProfile.data.oneTimeBought)
            {
                DataManager.SetInt("Purchased: " + productProfile.data.id + "_" + productProfile.data.type + "_" + productProfile.data.name, 1);
            }
            int currentCoins = NetworkManager.mainPlayer.coins - productProfile.data.price * productCount;
            NetworkManager.mainPlayer.UpdateCoins(currentCoins);

            NetworkManager.CallUpdatedCoins();
            Debug.Log("Purchased: " + productCount + " count, name " + productProfile.data.name + ", id " + productProfile.data.id + ", " + "with coins, price: " + productProfile.data.price * productCount);
            if (productProfile.data.defaultProductProfile == null && productProfile.data.oneTimeBought && Application.internetReachability == NetworkReachability.NotReachable)
            {
                InfoManager.Open("NoInternetForDownload", null, "");
            }
            CallPurchasedEvend(productProfile, PurchasedState.Successful);
        }
        protected void CallPurchasedEvend(NetworkManagement.ProductProfile productProfile, PurchasedState state)
        {
            Debug.Log("CallPurchasedEvend");
            if (OnPurchased != null)
            {
                OnPurchased(productProfile, state);
            }
        }
    }
}
