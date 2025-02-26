using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NetworkManagement;

public class AightBallPoolProductsUIManager : ProductsUIManager
{
    void Awake()
    {
        if (!NetworkManager.initialized)
        {
            return;
        }
        ShowGroupUI(AightBallPoolNetworkGameAdapter.is3DGraphics ? 0 : 1);
    }
}