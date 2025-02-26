using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NetworkManagement;
using System.Text;
using System.IO;

/// <summary>
/// Upgrade menu manager. This class is meneg all IAP and Upgrade UI system
/// </summary>
public class UpgradeMenuManager : MonoBehaviour
{
    [SerializeField] private string updatePath = "https://www.dropbox.com/s/axbucv0mxt4juh9/Update.txt?dl=1";
    [SerializeField] private string homeScene;
    [SerializeField] private ProductTypeList productsTypeListIOS;
    [SerializeField] private ProductTypeList productsTypeListAndroid;

    public ProductTypeList productsTypeList
    {
        get
        {
            #if UNITY_ANDROID
            return productsTypeListAndroid;
            #else  
            return productsTypeListIOS;
            #endif
        }
    }

    [SerializeField] private ProductTypeListManager productTypeListManager;
    [SerializeField] private ProductListManager productListManager;
    [SerializeField] private GameObject view3D;
    [SerializeField] private GameObject view2D;
    [SerializeField] private RectTransform productTypeListContent;
    [SerializeField] private RectTransform productListContent;
    [SerializeField] private BuyUI buyUI;
    [SerializeField] private ProductsUIManager productsUIManager;
    [SerializeField] private Text coinsText;
    [SerializeField] private RectTransform downloadProgress;
    [SerializeField] private Image progress;
    [SerializeField] private Text downloadProduct;
    [SerializeField] private RectTransform firstTimeUpdateWindow;

    private NetworkManagement.Product[] products;
    private List<NetworkManagement.ProductProfile> productsProfile;
    private List<NetworkManagement.ProductProfile> coinsProfile;
    private List<NetworkManagement.ProductProfile> adsProfile;


    private string productsMainPath;
    private bool downloadProductsInProcess = false;
    private  XmlNode productsXml;
    private ProductType currentProductType;
    private bool listInUpdateProcess = false;
    private bool updateProductsList;
    private bool selectProductInProcces = false;
    private bool updateList;
    private int localIpdateId;
    private int updateId;

    void Awake()
    {
        if (!NetworkManager.initialized)
        {
            GoToHome();
            return;
        }
        DataManager.SaveGameData();
        listInUpdateProcess = false;
        downloadProductsInProcess = false;

        view3D.gameObject.SetActive(AightBallPoolNetworkGameAdapter.is3DGraphics);
        view2D.gameObject.SetActive(!AightBallPoolNetworkGameAdapter.is3DGraphics);

        ProductTypeListManager.OnSelectProductType += ProductTypeListManager_OnSelectProductType;
        ProductListManager.OnSelectProduct += ProductListManager_OnSelectProduct;
        NetworkManager_OnCoinsUpdated(NetworkManager.mainPlayer.coins);
        products = AightBallPoolNetworkGameAdapter.is3DGraphics ? ProductsManagement.GetProductsById(0) : ProductsManagement.GetProductsById(1);
    }

    void OnEnable()
    {
        NetworkManagement.Product.update = false;
        NetworkManager.OnCoinsUpdated += NetworkManager_OnCoinsUpdated;
        DownloadManager.OnStartDownload += DownloadManager_OnStartDownload;
        DownloadManager.OnDownloading += DownloadManager_OnDownloading;
        DownloadManager.OnEndDownload += DownloadManager_OnEndDownload;
        downloadProgress.gameObject.SetActive(false);
    }


    void OnDisable()
    {
        NetworkManagement.Product.update = false;
        ProductTypeListManager.OnSelectProductType -= ProductTypeListManager_OnSelectProductType;
        ProductListManager.OnSelectProduct -= ProductListManager_OnSelectProduct;
        NetworkManager.OnCoinsUpdated -= NetworkManager_OnCoinsUpdated;
        DownloadManager.OnStartDownload -= DownloadManager_OnStartDownload;
        DownloadManager.OnDownloading -= DownloadManager_OnDownloading;
        DownloadManager.OnEndDownload -= DownloadManager_OnEndDownload;
    }

   
    private IEnumerator Start()
    {
        firstTimeUpdateWindow.gameObject.SetActive(false);
        updateId = DataManager.GetIntData("ListUptadedId");
        updateList = updateId == 0;
        localIpdateId = updateId;

        bool saveUpdateList = updateList;
        firstTimeUpdateWindow.gameObject.SetActive(updateList);

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            StartCoroutine(CheckForUpdate());
        }

        if (!saveUpdateList)
        {
            yield return StartCoroutine(UpdateListCoroutine(false));
            yield return StartCoroutine(UpdateProducts());
        }
    }

    private IEnumerator CheckForUpdate()
    {
        DownloadManager.DownloadParameters updateParameter = new DownloadManager.DownloadParameters(updatePath, "", DownloadManager.DownloadType.AlwaysDownload);
        yield return DownloadManager.Download(updateParameter);
        if (!string.IsNullOrEmpty(updateParameter.text))
        {
            string str = "";
            foreach (var item in updateParameter.text)
            {
                int result;
                if (int.TryParse(item.ToString(), out result))
                {
                    str += result;
                }
            }
            int uId = int.Parse(str);
            if (updateId < uId)
            {
                updateList = true;
            }
            updateId = uId;
        }
        if (localIpdateId != 0)
        {
            firstTimeUpdateWindow.gameObject.SetActive(updateList);
        }
    }

    public void Skip()
    {
        if (localIpdateId != 0)
        {
            DataManager.SetIntData("ListUptadedId", updateId);
        }
        firstTimeUpdateWindow.gameObject.SetActive(false);
    }

    public void UpdateList()
    {
        if (!listInUpdateProcess)
        {
            if (updateId == 0)
            {
                updateId = 1;
            }
            DataManager.SetIntData("ListUptadedId", updateId);
            firstTimeUpdateWindow.gameObject.SetActive(false);
            listInUpdateProcess = true;
            StartCoroutine(UpdateListCoroutine(true));
            if (updateList)
            {
                StartCoroutine(UpdateProducts());
            }
        }
    }

    private IEnumerator UpdateProducts()
    {
        foreach (NetworkManagement.Product product in products)
        {
            yield return StartCoroutine(product.ShowSavedProduct());
        }
    }

    void DownloadManager_OnDownloading(DownloadManager.DownloadParameters parameters)
    {
        this.downloadProduct.text = (parameters.downloading ? "Downloading: " : "Loading: ") + parameters.name;
        this.progress.fillAmount = parameters.progress;
    }

    void DownloadManager_OnEndDownload(DownloadManager.DownloadParameters parameters)
    {
        downloadProgress.gameObject.SetActive(false);
    }

    void DownloadManager_OnStartDownload(DownloadManager.DownloadParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.name))
        {
            downloadProgress.gameObject.SetActive(true);
        }
    }


    IEnumerator UpdateListCoroutine(bool update)
    {
        NetworkManagement.Product.update = update;
        updateProductsList = update;
        this.productsProfile = new List<ProductProfile>(0);
        this.coinsProfile = new List<ProductProfile>(0);
        this.adsProfile = new List<ProductProfile>(0);

        foreach (NetworkManagement.Product product in Product.FindObjectsOfType<NetworkManagement.Product>())
        {
            product.SetForUpdate();
        }
        listInUpdateProcess = true;
        ProductType[] productTypeList = productsTypeList.GetProductTypeListByName(AightBallPoolNetworkGameAdapter.is3DGraphics ? "3D" : "2D");
        foreach (ProductType productType in productTypeList)
        {
            productType.updateIcon = update;
        }
        productTypeListContent.localPosition = Vector3.zero;
        productTypeListManager.UpdateProductsType(productTypeList);
        DownloadManager.DownloadParameters productsParameters = new DownloadManager.DownloadParameters(ProductsManagement.productsPath, "list", updateProductsList ? DownloadManager.DownloadType.Update : DownloadManager.DownloadType.DownloadOrLoadFromDisc);
        yield return DownloadManager.Download(productsParameters);
        if (!productsParameters.isNull)
        {
            XmlDocument productsDockument = new XmlDocument();
            productsDockument.LoadXml(productsParameters.text);
            productsXml = productsDockument.SelectSingleNode("Products");
        }
        else
        {
            Debug.LogWarning("Products is not available in the url " + ProductsManagement.productsPath);
        }

        if (productTypeList != null && productTypeList.Length > 0)
        {
            for (int i = 1; i < productTypeList.Length; i++)
            {
                ForceSelecProductType(productTypeList[i]);
            }
            if (currentProductType == null)
            {
                currentProductType = productTypeList[0];
            }
            ForceSelecProductType(currentProductType);
        }
        else
        {
            Debug.LogWarning("List is empty");
        }
        updateProductsList = false;
        listInUpdateProcess = false;
    }

    void NetworkManager_OnCoinsUpdated(int coins)
    {
        coinsText.text = coins + "";
    }

    public void GoToHome()
    {
        SceneManager.LoadScene(homeScene);
    }

    public ProductProfile GetAdProfile()
    {
        return adsProfile[Random.Range(0, adsProfile.Count)];
    }
    public ProductProfile GetCoinProfileByPrice(int price)
    {
        ProductProfile coinProfile = null;
        foreach (var item in coinsProfile)
        {
            if (item.data.PriceFromName() >= price)
            {
                coinProfile = item;
                break;
            }
        }
        return coinProfile;
    }

    void ProductListManager_OnSelectProduct(ProductProfileUI productUI)
    {
        if (!productUI.product.data.isRealMoney && !productUI.product.data.purchased && NetworkManager.mainPlayer.coins < productUI.product.data.price)
        {
            ProductProfile coinProfile = GetCoinProfileByPrice(productUI.product.data.price);
            if (coinProfile == null)
            {
                Debug.LogWarning("There is not coin product with coins more than " + productUI.product.data.price +
                    ",\n please set the less prize for current product or create new coin product with coins more than " + productUI.product.data.price);
                ProductProfile adProfile = GetAdProfile();
                if (adProfile == null)
                {
                    buyUI.CloseWindow();
                }
                else
                {
                    buyUI.OpenAdWindow(adProfile);
                }
            }
            else
            {
                buyUI.OpenCoinsWindow(coinProfile, productUI, 1);
            }
        }
        else if (!productUI.product.data.oneTimeBought || !NetworkManager.purchasing.Purchased(productUI.product.data.id, productUI.product.data.type, productUI.product.data.name))
        {
            buyUI.OpenWindow(productUI);
        }
        else
        {
            OnSelecProduct(productUI, 1);
        }

    }

    private int currentProductCount = 1;

    public void OnSelecProduct(ProductProfileUI productUI, int productCount)
    {
        if (!selectProductInProcces)
        {
            currentProductCount = productCount;
            selectProductInProcces = true;
            StartCoroutine(SelecProduct(productUI));
        }
    }

    IEnumerator SelecProduct(ProductProfileUI productUI)
    {
        NetworkManagement.Product product = NetworkManagement.Product.FindProductByType(productUI.product.data.type, products);
        if (product)
        {
            product.currentProductCount = this.currentProductCount;
            yield return StartCoroutine(product.InitializeProduct(productUI.product));
        }
        else
        {
            yield return null;
        }
        selectProductInProcces = false;
        Resources.UnloadUnusedAssets();
    }

    void ForceSelecProductType(ProductType productType)
    {
        productListContent.localPosition = Vector3.zero;
        StartCoroutine(DownloadProducts(productType));
    }

    void ProductTypeListManager_OnSelectProductType(ProductTypeUI productTypeUI)
    {
        productListContent.localPosition = Vector3.zero;
        if (currentProductType == productTypeUI.productType)
        {
            //return;
        }
        if (!downloadProductsInProcess)
        {
            downloadProductsInProcess = true;
            productsUIManager.ShowProductUI(productTypeUI.productType.type);
            currentProductType = productTypeUI.productType;
            StartCoroutine(DownloadProducts(productTypeUI.productType));
        }
    }

    IEnumerator DownloadProducts(ProductType productType)
    {
        List<ProductProfile> productsProfile = new List<ProductProfile>(0);

        int id = 0;

        foreach (var defaultProduct in productType.defaultProducts)
        {
            id++;
            if (defaultProduct.price == 0)
            {
                NetworkManager.purchasing.ForceSetPurchased(id, productType.type, defaultProduct.name);
            }

            ProductData productData = new ProductData(id, productType.type, "", defaultProduct.name, defaultProduct.price, productType.isRealMoney, productType.oneTimeBought, productType.maxCount, null, defaultProduct);
            ProductProfile productProfile = new ProductProfile(productData, defaultProduct.icon);
            productsProfile.Add(productProfile);
            this.productsProfile.Add(productProfile);
            if (productType.isRealMoney && productType.type == "Coins")
            {
                this.coinsProfile.Add(productProfile);
            }
            if (!productType.isRealMoney && productType.type == "Ad")
            {
                this.adsProfile.Add(productProfile);
            }
        }
        if (productsProfile.Count > 0)
        {
            productListManager.UpdateProducts(productsProfile.ToArray(), false);
        }

        if (productsXml == null)
        {
            yield return null;
        }
        else
        {
            //Table2D, Cue2D, ... etc
            XmlNode concreteProducts = productsXml.SelectSingleNode(productType.nameInВatabase);

            if (concreteProducts != null)
            {
                
                foreach (XmlNode product in concreteProducts.ChildNodes)
                {
                    id++;
                    //Table, Cue,... etc
                    ProductData productData = ProductData.GetFromXmlNode(id, productType.type, product);

                    DownloadManager.DownloadParameters iconParameters = new DownloadManager.DownloadParameters(productData.iconURL, "products", productType.updateIcon ? DownloadManager.DownloadType.Update : DownloadManager.DownloadType.DownloadOrLoadFromDisc);

                    Debug.Log("iconParameters");
                    yield return DownloadManager.Download(iconParameters);

                    if (iconParameters.isNull)
                    {
                        Debug.LogWarning("There is no icon in this url  " + productData.iconURL);
                    }
                    else
                    {
                        ProductProfile productProfile = new ProductProfile(productData, iconParameters.texture);
                        productsProfile.Add(productProfile);
                        this.productsProfile.Add(productProfile);
                        if (productProfile.data.isRealMoney && productType.type == "Coins")
                        {
                            this.coinsProfile.Add(productProfile);
                        }
                        if (!productType.isRealMoney && productType.type == "Ad")
                        {
                            this.adsProfile.Add(productProfile);
                        }
                    }
                }
            }
        }
        if (productType.isRealMoney && productType.type == "Coins")
        {
            this.coinsProfile.Sort(
                delegate (ProductProfile productProfile1, ProductProfile productProfile2)
                {
                    return productProfile1.data.PriceFromName().CompareTo(productProfile2.data.PriceFromName());
                }
            );
      
        }

        productListManager.UpdateProducts(productsProfile.ToArray(), false);

        productType.updateIcon = false;
        downloadProductsInProcess = false;
    }
}
