using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class CardTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject cardVisualPrefab;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.01f, 0);
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0, 90, 0);

    
    private Dictionary<ARTrackedImage, GameObject> cardVisuals = new Dictionary<ARTrackedImage, GameObject>();
    
    public System.Action OnFirstCardDetected;
    
    public Dictionary<ARTrackedImage, GameObject> GetAllTrackedCards() => cardVisuals;

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Process newly detected AR images
        foreach (var trackedImage in eventArgs.added)
        {
            if (cardVisualPrefab != null && !cardVisuals.ContainsKey(trackedImage))
            {
                GameObject cardVisual = Instantiate(cardVisualPrefab);
                cardVisuals[trackedImage] = cardVisual;
                
                // Enable debug visual only for tracked cards
                CardColor cardColor = cardVisual.GetComponent<CardColor>();

                if (cardColor != null)
                {
                    cardColor.EnableDebugVisual();
                }

                // Set initial position to match the tracked image
                cardVisual.transform.position = trackedImage.transform.position;
                
                if (cardVisuals.Count == 1)
                    OnFirstCardDetected?.Invoke();
            }
        }

        // Process updated AR images (tracking state changes)
        foreach (var trackedImage in eventArgs.updated)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual) && visual != null)
            {
                // Only show visual when image is actively being tracked
                visual.SetActive(trackedImage.trackingState == TrackingState.Tracking);
                
                // Update position to match the tracked image
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    visual.transform.SetPositionAndRotation(
                        trackedImage.transform.TransformPoint(positionOffset),
                        trackedImage.transform.rotation * Quaternion.Euler(rotationOffsetEuler)
                    );
                }
            }
        }

        // Process removed AR images (no longer detected)
        foreach (var trackedImage in eventArgs.removed)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual))
            {
                Destroy(visual);
                cardVisuals.Remove(trackedImage);
            }
        }
    }
}