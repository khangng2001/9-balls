using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallPool;
using NetworkManagement;

public class Load3DCue : MonoBehaviour 
{
    private Cue3DSet cue;
    private ProductCue3D _productCue3D;
    private ProductCue3D productCue3D
    {
        get
        {
            if (!_productCue3D)
            {
                _productCue3D = ProductCue3D.FindObjectOfType<ProductCue3D>();
            }
            return _productCue3D;
        }
    }
    private GameObject opponentCue3DPrefab;
    private GameObject mainCue3DPrefab;
    private AssetBundle opponentCue;

    void Awake()
    {
        if (!AightBallPoolNetworkGameAdapter.is3DGraphics || BallPoolGameLogic.playMode == BallPool.PlayMode.Replay)
        {
            enabled = false;
            return;
        }
    }
    IEnumerator Start()
    {
        while (productCue3D.mainCue3DPrefab == null)
        {
            yield return null;
        }
        mainCue3DPrefab = productCue3D.mainCue3DPrefab;
        if (!BallPoolGameLogic.isOnLine || !AightBallPoolNetworkGameAdapter.isSameGraphicsMode)
        {
            CreateCue(mainCue3DPrefab);
        }
    }
    private void CreateCue(GameObject cuePrefab)
    {
        if (cue)
        {
            Destroy(cue.gameObject);
        }
        cue = GameObject.Instantiate(cuePrefab).GetComponent<Cue3DSet>();
        if (cue)
        {
            cue.Set();
        }
    }
    public void OnStart()
    {
        NetworkManager.network.SendRemoteMessage("SendOpponentCueURL", productCue3D.cue3DURL);
    }
    void OnDisable()
    {
        if (productCue3D && opponentCue)
        {
            opponentCue.Unload(true);
        }
    }
    public IEnumerator SetOpponentCueURL(string cueUrl)
    {
        ProductsManagement.ProductInfo productInfo = new ProductsManagement.ProductInfo("Cue3D");
        yield return StartCoroutine(ProductsManagement.LoadProducts(productInfo));
            
        string url = productCue3D.FindUrlForCurrentPlatform(cueUrl, productInfo.sourceURLInAllPlatform);
        while (!productCue3D.mainCue3DPrefab)
        {
            yield return null;
        }
        mainCue3DPrefab = productCue3D.mainCue3DPrefab;
        if (!string.IsNullOrEmpty(url) && productCue3D.sourcesURL != null && productCue3D.sourcesURL.Length > 0 && productCue3D.GetNameFromSourceURL(productCue3D.sourcesURL[0]) == productCue3D.GetNameFromSourceURL(cueUrl))
        {
            opponentCue3DPrefab = mainCue3DPrefab;
        }
        else
        {
            if (!string.IsNullOrEmpty(url))
            {
                DownloadManager.DownloadParameters parameter = new DownloadManager.DownloadParameters(url, "");
                yield return DownloadManager.Download(parameter, true);
                if (parameter.assetBundle)
                {
                    opponentCue = parameter.assetBundle;
                    opponentCue3DPrefab = (GameObject)opponentCue.LoadAsset(parameter.assetBundle.GetAllAssetNames()[0]);
                }
            }
            else 
            {
                while (!productCue3D.defaultCuePrefab)
                {
                    yield return null;
                }
                opponentCue3DPrefab = productCue3D.defaultCuePrefab;
            }
        }
        yield return null;
    }
    public IEnumerator SetCue2DTextureOnChangeTurn(bool myTurn)
    {
        if (myTurn)
        {
            while (mainCue3DPrefab == null)
            {
                yield return null;
            }
            CreateCue(mainCue3DPrefab);
        }
        else
        {
            while (opponentCue3DPrefab == null)
            {
                yield return null;
            }
            CreateCue(opponentCue3DPrefab);
        }
        yield return null;
    }
}
