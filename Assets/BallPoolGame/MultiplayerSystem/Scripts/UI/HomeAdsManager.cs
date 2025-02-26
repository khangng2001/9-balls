using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeAdsManager : MonoBehaviour 
{
    [SerializeField] private Button adsButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private float animateTime;
    private Vector3 startAdsButtonSqale;
    private Vector3 startUpgradeButtonSqale;

    void Awake()
    {
        startAdsButtonSqale = adsButton.targetGraphic.rectTransform.localScale;
        startUpgradeButtonSqale = upgradeButton.targetGraphic.rectTransform.localScale;
    }
    public void OfferAd()
    {
        StartCoroutine(AnimateAdsButton());
    }
    private IEnumerator AnimateAdsButton()
    {
        float time = animateTime;
        while (time > 0)
        {
            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            float cTime = Mathf.Sin(time * 20.0f);

            adsButton.targetGraphic.rectTransform.localScale = startAdsButtonSqale * (1.0f + 0.1f * cTime);
            upgradeButton.targetGraphic.rectTransform.localScale = startUpgradeButtonSqale * (1.0f - 0.1f * cTime);
        }
        adsButton.targetGraphic.rectTransform.localScale = startAdsButtonSqale;
    }
}
