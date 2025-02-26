using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;
using BallPool;

public class Load2DCue : MonoBehaviour 
{
    private ProductCue2D _productCue2D;
    private ProductCue2D productCue2D
    {
        get
        {
            if (!_productCue2D)
            {
                _productCue2D = ProductCue2D.FindObjectOfType<ProductCue2D>();
            }
            return _productCue2D;
        }
    }
    private Texture opponentCue2DTexture;
    private Texture mainCue2DTexture;

    public void OnStart()
    {
        NetworkManager.network.SendRemoteMessage("SendOpponentCueURL", productCue2D.cue2DURL);
    }
    void OnDisable()
    {
        if (mainCue2DTexture != null &&  productCue2D != null && productCue2D.cue2dMaterial != null)
        {
            productCue2D.cue2dMaterial.mainTexture = mainCue2DTexture;
        }
    }
    public IEnumerator SetOpponentCueURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            DownloadManager.DownloadParameters parameter = new DownloadManager.DownloadParameters(url, "");
            yield return DownloadManager.Download(parameter);
            if (parameter.texture)
            {
                opponentCue2DTexture = parameter.texture;
            }
        }
        else
        {
            while (!productCue2D.cueDefault2DTexture)
            {
                yield return null;
            }
            opponentCue2DTexture = productCue2D.cueDefault2DTexture;
        }
        while (!productCue2D.mainCue2DTexture)
        {
            yield return null;
        }
        mainCue2DTexture = productCue2D.mainCue2DTexture;
        yield return null;
    }
    public IEnumerator SetCue2DTextureOnChangeTurn(bool myTurn)
    {
        if (myTurn)
        {
            while (mainCue2DTexture == null)
            {
                yield return null;
            }
            productCue2D.cue2dMaterial.mainTexture = mainCue2DTexture;
        }
        else
        {
            while (opponentCue2DTexture == null)
            {
                yield return null;
            }
            productCue2D.cue2dMaterial.mainTexture = opponentCue2DTexture;
        }
    }
}
