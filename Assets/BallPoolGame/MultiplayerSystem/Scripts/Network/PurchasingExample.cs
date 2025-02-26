using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

public class PurchasingExample : PurchasingEngine 
{
    public PurchasingExample():base()
    {
        #if UNITY_ADS
        ads = new UnityAds();
        #else
        ads = new AdsExample();
        #endif
    }
    public override bool PurchaseingInProcces()
    {
        return false;
    }
    public override void PurchaseWithRealMoney(int productCount, ProductProfile productProfile)
    {
        //Imitating successful purchase with real money.
        if (productProfile.data.oneTimeBought)
        {
            SetPurchased(productProfile);
        }
        if (productProfile.data.type == "Coins")
        {
            NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins + productProfile.data.PriceFromName() * productCount);
            NetworkManager.CallUpdatedCoins();
        }
        Debug.Log("Purchased: " + productCount + " count, name: " + productProfile.data.name + ", id " + productProfile.data.id + ", " + "with real money, price: $" +  productProfile.data.price * productCount);
        CallPurchasedEvend(productProfile, PurchasedState.Successful);
    }
    public override void PurchaseFromAds(int productCount, ProductProfile productProfile)
    {
        //Imitating successful purchase with ads.
        ads.ShowAds(
            delegate(AdsShowResult result)
            {
                if (productProfile.data.oneTimeBought)
                {
                    DataManager.SetInt("Purchased: " + productProfile.data.id + "_" + productProfile.data.type + "_" + productProfile.data.name, 1);
                }
                NetworkManager.mainPlayer.UpdateCoins(NetworkManager.mainPlayer.coins - productProfile.data.price * productCount);
                NetworkManager.CallUpdatedCoins();
                Debug.Log("Purchased: " + productCount + " count, name: " + productProfile.data.name + ", id " + productProfile.data.id + ", " + "with ads, prize: " +  -productProfile.data.price * productCount);
                CallPurchasedEvend(productProfile, PurchasedState.Successful);
            }
        );
    }
}
