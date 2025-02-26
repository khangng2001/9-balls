using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    public delegate void SelecProductHandler(NetworkManagement.ProductProfileUI productUI);
    /// <summary>
    /// Product list manager.
    /// </summary>
    public class ProductListManager : ContentListManager
    {
        public static event SelecProductHandler OnSelectProduct;

        public void UpdateProducts(NetworkManagement.ProductProfile[] products, bool hideContentButton = true)
        {
            RessetOldButtons(hideContentButton);

            if (products == null || products.Length == 0)
            {
                return;
            }
            for (int i = 0; i < products.Length; i++)
            {
                Button currentButton = GetCurrentButton(i);

                ProductProfileUI currentProductUI = currentButton.GetComponentInChildren<ProductProfileUI>();

                currentProductUI.SetProduct(products[i]);

                NetworkManagement.Pointer currentPointer = currentButton.GetComponent<Pointer>();

                if (currentPointer && Application.isMobilePlatform)
                {

                    currentPointer.OnSelecButton += (Pointer pointer) =>
                    {
                            if (OnSelectProduct != null)
                            {
                                OnSelectProduct(currentProductUI);
                            }
                    };
                }
                else
                {
                    currentButton.onClick.AddListener(() =>
                        {
                            if (OnSelectProduct != null)
                            {
                                OnSelectProduct(currentProductUI);
                            }
                        });
                }
                AddButton(currentButton);
            }
        }
    }
}
