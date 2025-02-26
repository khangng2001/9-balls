#if UNITY_PURCHASING
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Purchasing;

public delegate void PurchaseResultHandler(NetworkManagement.PurchasedState result);

public class UnityIAPEngine : MonoBehaviour, IStoreListener
{
    private string storeIndependentId;
    private PurchaseResultHandler handler;
    private Product currentProduct;

    public bool purchaseingInProcces { get; private set; }

    private ConfigurationBuilder builder;
    private List<string> ids;

    void OnEnable()
    {
        purchaseingInProcces = false;
        ids = new List<string>(0);
        builder = null;
    }

    public void Purchase(string storeIndependentId, bool consumable, PurchaseResultHandler handler)
    {
        if (purchaseingInProcces)
        {
            return;
        }
        purchaseingInProcces = true;
        this.storeIndependentId = storeIndependentId;
        this.handler = handler;
        InitializeProduct(storeIndependentId, consumable ? ProductType.Consumable : ProductType.NonConsumable);
    }

    private void InitializeProduct(string storeIndependentId, ProductType productType)
    {
        if (builder == null)
        {
            builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        }
        if (!ids.Contains(storeIndependentId))
        {
            ids.Add(storeIndependentId);
            builder.AddProduct(storeIndependentId, productType);
        }
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Product product = controller.products.WithID(storeIndependentId);
        currentProduct = product;
        bool availableToPurchase = product != null && product.availableToPurchase;
        if (availableToPurchase)
        {
            controller.InitiatePurchase(product);    
        }
        else
        {
            handler(NetworkManagement.PurchasedState.Unsuccessful);
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        purchaseingInProcces = false;
        NetworkManagement.DebugManager.DebugLog("error " + error.ToString());
        if (handler != null)
        {
            handler(NetworkManagement.PurchasedState.Unsuccessful);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        purchaseingInProcces = false;
        NetworkManagement.DebugManager.DebugLog("PurchaseFailureReason " + product.definition.id + "  " + failureReason.ToString());
        if (handler != null)
        {
            handler(NetworkManagement.PurchasedState.Unsuccessful);
        }
       
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    { 
        purchaseingInProcces = false;
        if (currentProduct == args.purchasedProduct)
        {
            if (String.Equals(currentProduct.definition.id, storeIndependentId, StringComparison.Ordinal))
            {
                handler(NetworkManagement.PurchasedState.Successful);
            }
            else
            {
                handler(NetworkManagement.PurchasedState.Unsuccessful);
            }
        }
        return PurchaseProcessingResult.Complete;
    }
    void OnApplicationPause(bool pauseStatus)
    {

    }
}
#endif