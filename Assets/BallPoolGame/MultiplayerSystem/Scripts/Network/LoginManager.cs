using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkManagement
{
    /// <summary>
    /// The login manager.
    /// </summary>
    public class LoginManager
    {
        private static bool _isLogined;
        public static bool logined
        {
            get 
            { 
                if (!_isLogined)
                {
                    _isLogined = DataManager.GetIntData("IsLogined") == 1;
                }
                return _isLogined;
            }
            set 
            { 
                _isLogined = value; 
                DataManager.SetIntData("IsLogined", _isLogined?1:0); 
            }
        }
        public static bool loginedFacebook
        {
            get 
            { 
                return FacebookManager.instance.IsLoggedIn;
            }
        }
    }
}
