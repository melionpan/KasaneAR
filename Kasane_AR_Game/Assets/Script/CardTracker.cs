using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CardTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject cardVisualPrefab;
    
    private Dictionary<ARTrackedImage, GameObject> cardVisuals = new Dictionary<ARTrackedImage, GameObject>();
    
    // Event für andere Klassen
    public System.Action OnFirstCardDetected;
    
    void OnEnable() => trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    void OnDisable() => trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (cardVisualPrefab != null && !cardVisuals.ContainsKey(trackedImage))
            {
                GameObject cardVisual = Instantiate(cardVisualPrefab, trackedImage.transform);
                cardVisuals[trackedImage] = cardVisual;
                
                // Event auslösen wenn erste Karte erkannt
                if (cardVisuals.Count == 1)
                    OnFirstCardDetected?.Invoke();
            }
        }
        
        foreach (var trackedImage in eventArgs.updated)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual) && visual != null)
            {
                visual.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            }
        }
        
        foreach (var trackedImage in eventArgs.removed)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual))
            {
                Destroy(visual);
                cardVisuals.Remove(trackedImage);
            }
        }
    }
    
    public Dictionary<ARTrackedImage, GameObject> GetAllTrackedCards() => cardVisuals;
}