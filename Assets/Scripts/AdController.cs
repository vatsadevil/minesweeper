using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Minesweeper
{
    public class AdController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public static AdController Instance = null;
        [SerializeField] string _androidGameId;
        [SerializeField] string _iOsGameId;
        [SerializeField] bool _testMode = true;
        [SerializeField] bool _enablePerPlacementMode = true;
        private const string BANNER_AD_ID_ANDROID = "Banner_Android";
        private const string INTERSTITIAL_AD_ID_ANDROID = "Interstitial_Android";

        private string _gameId;
        private bool _initialized = false;
        private bool _isAdLoaded = false;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                InitializeAds();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeAds()
        {
            _gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
                ? _iOsGameId
                : _androidGameId;
            Advertisement.Initialize(_gameId, _testMode, _enablePerPlacementMode, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
            _initialized = true;
            LoadInterstitialAd();
        }

        
        public void LoadInterstitialAd()
        {
            if(!_initialized) return;
            _isAdLoaded = false;
            Advertisement.Load(INTERSTITIAL_AD_ID_ANDROID, this);
        }

        public void ShowInterstitialAd()
        { 
            if(!_initialized || !_isAdLoaded) return;
            Advertisement.Show(INTERSTITIAL_AD_ID_ANDROID, this);
        }   


        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log("Ad loaded "+placementId);
            _isAdLoaded = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.Log("Ad load failed");
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.Log("Ad Show failed: "+message);
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            // throw new System.NotImplementedException();
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            // throw new System.NotImplementedException();
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            // throw new System.NotImplementedException();
        }
    }
}