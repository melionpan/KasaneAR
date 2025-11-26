using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

public class CardTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject cardVisualPrefab;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.01f, 0);
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0, 90, 0);
    
    [Header("Tracking Stability Settings")]
    [SerializeField] private float trackingSmoothing = 0.3f; // Reduced for better responsiveness
    [SerializeField] private bool enableTrackingSmoothing = true;
    
    private Dictionary<ARTrackedImage, GameObject> cardVisuals = new();
    private Dictionary<ARTrackedImage, Vector3> previousPositions = new();
    
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
        Debug.Log($"CardTracker: Added={eventArgs.added.Count}, Updated={eventArgs.updated.Count}, Removed={eventArgs.removed.Count}");

        // Process newly detected AR images
        foreach (var trackedImage in eventArgs.added)
        {
            if (cardVisualPrefab != null && !cardVisuals.ContainsKey(trackedImage))
            {
                GameObject cardVisual = Instantiate(cardVisualPrefab);
                cardVisuals[trackedImage] = cardVisual;
                
                CardColor cardColor = cardVisual.GetComponent<CardColor>();
                if (cardColor != null)
                {
                    cardColor.EnableDebugVisual();
                }

                UpdateCardPositionImmediate(trackedImage, cardVisual);
                
                if (cardVisuals.Count == 1)
                {
                    OnFirstCardDetected?.Invoke();
                    Debug.Log("First card detected - pots should spawn");
                }
                
                Debug.Log($"Card ADDED: {trackedImage.referenceImage.name}");
            }
        }

        // Process updated AR images (tracking state changes)
        foreach (var trackedImage in eventArgs.updated)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual) && visual != null)
            {
                // Only show when actively tracked
                bool shouldBeVisible = trackedImage.trackingState == TrackingState.Tracking;
                visual.SetActive(shouldBeVisible);
                
                // Update position only when actively tracked
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    UpdateCardPosition(trackedImage, visual);
                }
                
                Debug.Log($"Card UPDATED: {trackedImage.referenceImage.name} - State: {trackedImage.trackingState}, Visible: {shouldBeVisible}");
            }
        }

        // Process removed AR images (no longer detected)
        foreach (var trackedImage in eventArgs.removed)
        {
            if (cardVisuals.TryGetValue(trackedImage, out GameObject visual))
            {
                Debug.Log($"Card REMOVED: {trackedImage.referenceImage.name}");
                Destroy(visual);
                cardVisuals.Remove(trackedImage);
                previousPositions.Remove(trackedImage);
            }
        }
    }
    
    void UpdateCardPositionImmediate(ARTrackedImage trackedImage, GameObject visual)
    {
        Vector3 targetPosition = trackedImage.transform.TransformPoint(positionOffset);
        Quaternion targetRotation = trackedImage.transform.rotation * Quaternion.Euler(rotationOffsetEuler);
        
        visual.transform.SetPositionAndRotation(targetPosition, targetRotation);
        previousPositions[trackedImage] = targetPosition;
    }
    
    void UpdateCardPosition(ARTrackedImage trackedImage, GameObject visual)
    {
        Vector3 targetPosition = trackedImage.transform.TransformPoint(positionOffset);
        Quaternion targetRotation = trackedImage.transform.rotation * Quaternion.Euler(rotationOffsetEuler);
        
        if (enableTrackingSmoothing && previousPositions.ContainsKey(trackedImage))
        {
            // Smooth position to reduce flickering
            Vector3 smoothedPosition = Vector3.Lerp(previousPositions[trackedImage], targetPosition, trackingSmoothing);
            visual.transform.position = smoothedPosition;
            previousPositions[trackedImage] = smoothedPosition;
            
            // Smooth rotation as well
            visual.transform.rotation = Quaternion.Slerp(visual.transform.rotation, targetRotation, trackingSmoothing);
        }
        else
        {
            // Direct positioning
            visual.transform.SetPositionAndRotation(targetPosition, targetRotation);
            previousPositions[trackedImage] = targetPosition;
        }
    }
}