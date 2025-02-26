using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkManagement
{
    /// <summary>
    /// The login menu manager.
    /// </summary>
    public class LoginMenuManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform loginPanel;

        [SerializeField]
        private InputField email;
        [SerializeField]
        private InputField password;
        [SerializeField]
        private Toggle saveEmail;
     
        void OnEnable()
        {
            NetworkManager.social.OnSignUp += NetworkManage_OnLogin;
            NetworkManager.social.OnLogin += NetworkManage_OnLogin;
            NetworkManager.social.OnLoginWithFacebok += NetworkManager_OnLoginWithFacebok;
           
            CloseLoginWindow();
            saveEmail.isOn = DataManager.GetIntData("SaveEmail") == 1;
            if (saveEmail.isOn && LoginManager.logined)
            {
                email.text = DataManager.GetString("PlayerEmail");
                password.text = DataManager.GetString("PlayerPassword");
                password.text = string.Format("Password", DataManager.GetString("PlayerPassword"));
                Login();
            }
            saveEmail.isOn = true;
        }
      
        void NetworkManager_OnLoginWithFacebok (LoginedState state)
        {
            if (state == LoginedState.Successful)
            {
                CloseLoginWindow();
                StartCoroutine(NetworkManager.LoadMainPlayer(null));
            }
        }
        void NetworkManage_OnLogin(LoginedState state)
        {
            if (state == LoginedState.Successful)
            {
                if (saveEmail.isOn)
                {
                    SaveLogin();
                }
                CloseLoginWindow();
                StartCoroutine(NetworkManager.LoadMainPlayer(null));
            }
        }
        void SaveLogin()
        {
            DataManager.SetIntData("SaveEmail", 1);
            DataManager.SetString("PlayerEmail", email.text);
            DataManager.SetString("PlayerPassword", password.text);
        }
     
        public void OpenLoginWindow()
        {
            loginPanel.gameObject.SetActive(true);
        }

        public void CloseLoginWindow()
        {
            loginPanel.gameObject.SetActive(false);
        }

        public void SignUp()
        {
            NetworkManager.SignUp(email.text, password.text);
        }

        public void Login()
        {
            NetworkManager.Login(email.text, password.text);
        }

        public void LoginWithFacebook()
        {
            NetworkManager.LoginWithFacebook();
        }


        public void OpenPrivacyPolicyURL()
        {
            Application.OpenURL(NetworkManager.privacyPolicyURL);
        }
    }
}
