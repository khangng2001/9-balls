using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;
using BallPool;

public class Load2DTable : MonoBehaviour
{
    private ProductTable2D _productTable2D;
    private ProductTable2D productTable2D
    {
        get
        {
            if (!_productTable2D)
            {
                _productTable2D = ProductTable2D.FindObjectOfType<ProductTable2D>();
            }
            return _productTable2D;
        }
    }

    private Texture opponentTableBoard2DTexture;
    private Texture opponentTableCloth2DTexture;

    private Texture mainTableBoard2DTexture;
    private Texture mainTableCloth2DTexture;

    private Color mainTable2DColor;
    private Color opponentTable2DColor;
    private bool isStarted;

    public void OnStart()
    {
        NetworkManager.network.SendRemoteMessage("SendOpponentTableURLs", productTable2D.table2DBoardURL, productTable2D.table2DClothURL, DataManager.ColorToString(productTable2D.mainTable2DColor));
    }
    void OnDisable()
    {
        if (productTable2D && mainTableBoard2DTexture != null && mainTableCloth2DTexture != null && mainTable2DColor != null)
        {
            if (productTable2D.boardMaterial && productTable2D.clothMaterial && productTable2D.clothColorMaterial)
            {
                productTable2D.boardMaterial.mainTexture = mainTableBoard2DTexture;
                productTable2D.clothMaterial.mainTexture = mainTableCloth2DTexture;
                productTable2D.clothColorMaterial.color = mainTable2DColor;
            }
        }
    }
    public IEnumerator SetOpponentTableURLs(string boardURL, string clothURL, string clothColor)
    {
        if (!string.IsNullOrEmpty(boardURL) && !string.IsNullOrEmpty(clothURL))
        {
            DownloadManager.DownloadParameters parameter1 = new DownloadManager.DownloadParameters(boardURL, "");
            yield return DownloadManager.Download(parameter1);
            if (parameter1.texture)
            {
                opponentTableBoard2DTexture = parameter1.texture;
            }
            DownloadManager.DownloadParameters parameter2 = new DownloadManager.DownloadParameters(clothURL, "");
            yield return DownloadManager.Download(parameter2);
            if (parameter2.texture)
            {
                opponentTableCloth2DTexture = parameter2.texture;
            }
        }
        else
        {
            while (!productTable2D.tableBoardDefault2DTexture || !productTable2D.tableClothDefault2DTexture)
            {
                yield return null;
            }

            opponentTableBoard2DTexture = productTable2D.tableBoardDefault2DTexture;
            opponentTableCloth2DTexture = productTable2D.tableClothDefault2DTexture;
        }

        opponentTable2DColor = DataManager.ColorFromString(clothColor);

        while (!productTable2D.mainTableBoard2DTexture || !productTable2D.mainTableCloth2DTexture || productTable2D.mainTable2DColor == null)
        {
            yield return null;
        }
        mainTableBoard2DTexture = productTable2D.mainTableBoard2DTexture;
        mainTableCloth2DTexture = productTable2D.mainTableCloth2DTexture;
        mainTable2DColor = productTable2D.mainTable2DColor;
        yield return null;
        int number = (AightBallPoolPlayer.mainPlayer.coins == AightBallPoolPlayer.otherPlayer.coins) ? 0 : (AightBallPoolPlayer.mainPlayer.coins > AightBallPoolPlayer.otherPlayer.coins ? 1 : 2);
        StartCoroutine(SetTable2DTextureOnStartGame(number));
    }
    public IEnumerator SetTable2DTextureOnStartGame(int number)
    {
        if (number != 0)
        {
            if (number == 1)
            {
                while (mainTableBoard2DTexture == null || mainTableCloth2DTexture == null || mainTable2DColor == null)
                {
                    yield return null;
                }
                productTable2D.boardMaterial.mainTexture = mainTableBoard2DTexture;
                productTable2D.clothMaterial.mainTexture = mainTableCloth2DTexture;
                productTable2D.clothColorMaterial.color = mainTable2DColor;

                BallPoolGameManager.instance.SetGameInfo("You have more coins than your opponent \n You played with your table");
            }
            else
            {
                while (opponentTableBoard2DTexture == null || opponentTableCloth2DTexture == null || opponentTable2DColor == null)
                {
                    yield return null;
                }
                productTable2D.boardMaterial.mainTexture = opponentTableBoard2DTexture;
                productTable2D.clothMaterial.mainTexture = opponentTableCloth2DTexture;
                productTable2D.clothColorMaterial.color = opponentTable2DColor;

                BallPoolGameManager.instance.SetGameInfo("You have less coins than your opponent \n You played with your opponent table");
            }
        }
        yield return null;
    }
}
