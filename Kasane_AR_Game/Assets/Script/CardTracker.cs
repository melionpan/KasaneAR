using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CardTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject cardVisualPrefab;
    
    private Dictionary<ARTrackedImage, GameObject> cardVisuals = new Dictionary<ARTrackedImage, GameObject>();
    
    public System.Action OnFirstCardDetected;
    
    // Get all currently tracked cards with their visual representations
    public Dictionary<ARTrackedImage, GameObject> GetAllTrackedCards() => cardVisuals;
    
    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    // Handle AR tracked image changes (added, updated, removed)
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        ProcessAddedImages(eventArgs.added);
        ProcessUpdatedImages(eventArgs.updated);
        ProcessRemovedImages(eventArgs.removed);
    }
    
    // Process newly detected AR images
    void ProcessAddedImages(List<ARTrackedImage> addedImages)
    {
        foreach (var trackedImage in addedImages)
        {
            if (cardVisualPrefab != null && !cardVisuals.ContainsKey(trackedImage))
            {
                // Create visual representation for detected card
                GameObject cardVisual = Instantiate(cardVisualPrefab, trackedImage.transform);
                cardVisuals[trackedImage] = cardVisual;
                
                // Trigger event when first card is detected
                if (cardVisuals.Count == 1)
                    OnFirstCardDetected?.Invoke();
            }
        }
    }
    
    // Process updated AR images (tracking state changes)
    void ProcessUpdatedImages(List<ARTrackedImage> updatedImages)
    {
        foreach (var trackedImage in updatedImages)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual) && visual != null)
            {
                // Only show visual when image is actively being tracked
                visual.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            }
        }
    }
    
    // Process removed AR images (no longer detected)
    void ProcessRemovedImages(List<ARTrackedImage> removedImages)
    {
        foreach (var trackedImage in removedImages)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual))
            {
                // Clean up visual representation
                Destroy(visual);
                cardVisuals.Remove(trackedImage);
            }
        }
    }
}