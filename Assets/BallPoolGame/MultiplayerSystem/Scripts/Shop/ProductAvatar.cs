using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkManagement
{
    public class ProductAvatar : NetworkManagement.Product
    {
        public string avatarURL{ get; private set; }
        public string avatarName{ get; private set; }

        private HomeMenuManager _homeMenuManager;
        private HomeMenuManager homeMenuManager
        {
            get
            {
                if (!_homeMenuManager)
                {
                    _homeMenuManager = HomeMenuManager.FindObjectOfType<HomeMenuManager>();
                }
                return _homeMenuManager;
            }
        }

        protected override IEnumerator FirstInitializeProduct()
        {
            yield return StartCoroutine(base.FirstInitializeProduct());
        }
        public override IEnumerator InitializeProduct(ProductProfile productProfile)
        {
            yield return StartCoroutine(base.InitializeProduct(productProfile));
            yield return StartCoroutine(SetSources());
        }

        protected override IEnumerator SetSources()
        {
            if (this.productProfile != null)
            {
                if (!string.IsNullOrEmpty(this.productProfile.data.iconURL))
                {
                    iconeURL = this.productProfile.data.iconURL;
                    avatarName = "";
                }
                else
                {
                    iconeURL = "";
                    avatarName = productProfile.data.name;
                }
            }
            else
            {
                avatarURL = GetIconURL();
                avatarName = GetIconName();
            }
            avatarURL = iconeURL;

            SaveIconURL(iconeURL);
            SaveIconName(avatarName);

            if (!string.IsNullOrEmpty(avatarURL) || !string.IsNullOrEmpty(avatarName))
            {
                NetworkManager.social.SetAvatarDataIsLocal(true);
            }
            if (!homeMenuManager)
            {
                CallSetParameters(typeof(ProductAvatar), avatarName, avatarURL);
                StartCoroutine(WaitForHomeMenuManager());
            }
            yield return null;
        }

        private IEnumerator WaitForHomeMenuManager()
        {
            while (!homeMenuManager)
            {
                yield return null;
            }
            yield return new WaitForEndOfFrame();
            if (homeMenuManager)
            {
                CallSetParameters(typeof(ProductAvatar), avatarName, avatarURL);
            }
        }


        public override void ResetWhenBackToEditor()
        {

        }
       
    }
}
