using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

public class UnityIAP : PurchasingEngine 
{
    #if UNITY_PURCHASING
    private UnityIAPEngine _iapUnityEngine;
    private UnityIAPEngine iapUnityEngine
    {
        get
        {
            if (!_iapUnityEngine)
            {
                _iapUnityEngine = new GameObject("UnityIAPEngine").AddComponent<UnityIAPEngine>();
            }
            return _iapUnityEngine;
        }
    }
    #endif

    public UnityIAP():base()
    {
        #if UNITY_ADS
        ads = new UnityAds();
        #else
        ads = new AdsExample();
        #endif
    }
    public override bool PurchaseingInProcces()
    {
        #if UNITY_PURCHASING
        return iapUnityEngine.purchaseingInProcces;
        #else
        return false;
        #endif
    }
    public override void PurchaseWithRealMoney(int productCount, ProductProfile productProfile)
    {
        #if UNITY_PURCHASING

		//com.Vagho.My8BallPoolGame
		iapUnityEngine.Purchase(PurchasingEngine.StoreIndependentId + productProfile.data.name.Replace(" ",""), !productProfile.data.oneTimeBought, 
            delegate(NetworkManagement.PurchasedState result)
            { 
                DebugManager.DebugLog(productProfile.data.name + "  result: " + result);
                if(result == NetworkManagement.PurchasedState.Successful)
                {
                    if (productProfile.data.oneTimeBought)
                    {
                        SetPurchased(productProfile);
                    }
                    if(productProfile.data.type == "Coins")
                    {
                        NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins + productProfile.data.PriceFromName() * productCount);
                        NetworkManager.CallUpdatedCoins();
                    }
                    Debug.Log("Purchased: " + productCount + " count, name: " + productProfile.data.name + ", id " + productProfile.data.id + ", " + "with real money, price: $" +  productProfile.data.price * productCount);
                    DebugManager.DebugLog("Purchased: " + productCount + " count, name: " + productProfile.data.name + ", id " + productProfile.data.id + ", " + "with real money, price: $" +  productProfile.data.price * productCount);
                    CallPurchasedEvend(productProfile, PurchasedState.Successful);
                }
                else
                {
                    CallPurchasedEvend(productProfile, PurchasedState.Unsuccessful);
                }
            }
        );
        #endif

    }
    public override void PurchaseFromAds(int productCount, ProductProfile productProfile)
    {
        ads.ShowAds(
            delegate(AdsShowResult result)
            { 
                if (adsIsOpened)
                {
                    return;
                }
                adsIsOpened = true;
                if(result == AdsShowResult.Finished)
                {
                    if (productProfile.data.oneTimeBought)
                    {
                        DataManager.SetInt("Purchased: " + productProfile.data.id + "_" + productProfile.data.type + "_" + productProfile.data.name, 1);
                    }
                    NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins - productProfile.data.price * productCount);
                    NetworkManager.CallUpdatedCoins();
                    Debug.Log("Added: " + (-productProfile.data.price * productCount).ToString() + " coins with ad: " + productProfile.data.name + ", id " + productProfile.data.id + " count: " + productCount);
                    CallPurchasedEvend(productProfile, PurchasedState.Successful);
                }
                else
                {
                    CallPurchasedEvend(productProfile, PurchasedState.Unsuccessful);
                }
                adsIsOpened = false;
            }
        );
    }
}
