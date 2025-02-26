using System.Collections;
using System.Collections.Generic;

namespace NetworkManagement
{
    public class AdsExample: Ads
    {
        public void ShowAds(ViewAdsHandler handler)
        {
            //Imitating successful show.
            handler(AdsShowResult.Finished);
        }
    }
}
