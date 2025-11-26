using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInteraction : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 1f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float minScale = 0.05f;
    [SerializeField] private float maxScale = 0.5f;
    [SerializeField] private float zoomSensitivity = 0.001f;
    
    private Camera arCamera;
    private bool isDragging;
    private Vector3 offset;
    private Vector3 initialScale;
    
    // Pinch zoom variables
    private bool wasPinching;
    private float initialPinchDistance;
    private Vector3 initialScaleAtPinchStart;
    
    private void Start()
    {
        arCamera = Camera.main;
        initialScale = transform.localScale;
        
        if (transform.localScale.magnitude < minScale)
        {
            transform.localScale = Vector3.one * minScale;
        }
    }
    
    private void Update()
    {
        HandleTouchInput();
        HandlePinchZoom();
    }
    
    private void HandleTouchInput()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (!isDragging)
            {
                if (!IsPinching() && IsTouched(touchPosition))
                {
                    StartDrag(touchPosition);
                }
            }
            else
            {
                if (!IsPinching())
                {
                    ContinueDrag(touchPosition);
                }
                else
                {
                    isDragging = false;
                }
            }
        }
        else
        {
            isDragging = false;
        }
    }
    
    private void HandlePinchZoom()
    {
        bool isPinching = IsPinching();
        
        if (isPinching && !wasPinching)
        {
            StartPinch();
        }
        else if (isPinching && wasPinching)
        {
            ContinuePinch();
        }
        
        wasPinching = isPinching;
    }
    
    private bool IsPinching()
    {
        if (Touchscreen.current.touches.Count < 2)
            return false;
            
        return Touchscreen.current.touches[0].press.isPressed && 
               Touchscreen.current.touches[1].press.isPressed;
    }
    
    private void StartPinch()
    {
        Vector2 touch1 = Touchscreen.current.touches[0].position.ReadValue();
        Vector2 touch2 = Touchscreen.current.touches[1].position.ReadValue();
        
        initialPinchDistance = Vector2.Distance(touch1, touch2);
        initialScaleAtPinchStart = transform.localScale;
        
        isDragging = false;
    }
    
    private void ContinuePinch()
    {
        Vector2 touch1 = Touchscreen.current.touches[0].position.ReadValue();
        Vector2 touch2 = Touchscreen.current.touches[1].position.ReadValue();
        
        float currentPinchDistance = Vector2.Distance(touch1, touch2);
        float pinchRatio = currentPinchDistance / initialPinchDistance;
        
        Vector3 newScale = initialScaleAtPinchStart * pinchRatio;
        newScale = Vector3.Max(Vector3.one * minScale, newScale);
        newScale = Vector3.Min(Vector3.one * maxScale, newScale);
        
        transform.localScale = newScale;
    }
    
    private bool IsTouched(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject;
    }
    
    private void StartDrag(Vector2 screenPosition)
    {
        isDragging = true;
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            offset = transform.position - hit.point;
        }
    }
    
    private void ContinueDrag(Vector2 screenPosition)
    {
        if (!isDragging) return;
        
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        Plane dragPlane = new Plane(Vector3.up, transform.position);
        
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = Vector3.Lerp(transform.position, hitPoint + offset, dragSpeed * Time.deltaTime);
        }
    }
    
    // Public method to reset scale if needed
    public void ResetScale()
    {
        transform.localScale = initialScale;
    }
}