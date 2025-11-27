using UnityEngine;

public class CardColor : MonoBehaviour
{
    public Color currentColor { get; private set; } = Color.white;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private Color debugColor = Color.white;
    
    [Header("Tracking Stability")]
    [SerializeField] private float minVisibleTime = 0.5f; // Card must be visible for this long before reacting
    
    private GameObject colorEffect;
    private GameObject debugCube;
    private Renderer debugRenderer;
    private bool allowDebugVisual;
    private float lastVisibilityChangeTime;
    private bool wasVisible;
    
    public void EnableDebugVisual() => allowDebugVisual = true;
    public void DisableDebugVisual() => allowDebugVisual = false;
    
    // Check if card has been colored (not white)
    public bool IsColored()
    {
        return true;
    }
    
    public void ResetColor()
    {
        currentColor = Color.white;
    
        if (colorEffect != null)
            Destroy(colorEffect);
        
        if (debugMode && debugRenderer != null)
        {
            debugRenderer.material.color = Color.white;
        }
    }
    
    public bool WouldChangeColor(Color newColor)
    {
        return currentColor != newColor;
    }
    
    // More stable version that checks if card has been stable for a while
    public bool IsStableAndColored()
    {
        // Only consider the card stable if it's been visible for a minimum time
        bool isCurrentlyVisible = gameObject.activeInHierarchy;
        if (isCurrentlyVisible && !wasVisible)
        {
            lastVisibilityChangeTime = Time.time;
        }
        wasVisible = isCurrentlyVisible;
        
        return isCurrentlyVisible && (Time.time - lastVisibilityChangeTime >= minVisibleTime);
    }
    
    // Apply a new color to the card and update visual effects
    public void ApplyColor(Color newColor)
    {
        if (currentColor != newColor)
        {
            Debug.Log($"Card color changing from {currentColor} to {newColor}");
        }
        
        currentColor = newColor;
        
        if (colorEffect != null)
            Destroy(colorEffect);
            
        colorEffect = CreateColorEffect(newColor);
        
        if (debugMode && debugRenderer != null)
        {
            debugRenderer.material.color = newColor;
        }
    }
    
    public void SetDebugColor(Color color)
    {
        debugColor = color;
        if (debugMode && debugRenderer != null)
        {
            debugRenderer.material.color = color;
        }
    }
    
    public void SetTrackingQuality(float quality)
    {
        if (debugMode && debugRenderer != null)
        {
            Color qualityColor = Color.Lerp(Color.red, Color.green, quality);
            debugRenderer.material.color = qualityColor;
        }
    }
    
    private GameObject CreateColorEffect(Color color)
    {
        if (debugMode)
        {
            Debug.Log($"Karte würde eingefärbt: {color}");
        }
        return null;
    }
    
    void CreateDebugVisual()
    {
        if (!allowDebugVisual) return;
        
        // Debug-Cube für Testing
        debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.SetParent(transform);
        debugCube.transform.localPosition = Vector3.zero;
        debugCube.transform.localRotation = Quaternion.identity;
        debugCube.transform.localScale = new Vector3(0.085f, 0.001f, 0.055f);
        
        debugRenderer = debugCube.GetComponent<Renderer>();
        debugRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        debugRenderer.material.color = debugColor;
        
        // Collider vom Debug-Cube entfernen (wir haben schon den BoxCollider)
        Collider debugCollider = debugCube.GetComponent<Collider>();
        if (debugCollider != null)
            Destroy(debugCollider);
    }
    
    void Start()
    {
        if (debugMode && allowDebugVisual)
        {
            CreateDebugVisual();
        }
        lastVisibilityChangeTime = Time.time;
        wasVisible = gameObject.activeInHierarchy;
    }
    
    void OnDestroy()
    {
        // Clean up objects
        if (debugCube != null)
            Destroy(debugCube);
        if (colorEffect != null) 
            Destroy(colorEffect);
    }
    
    
    public bool CanRecolor()
    {
        return IsStableAndColored();
    }
}