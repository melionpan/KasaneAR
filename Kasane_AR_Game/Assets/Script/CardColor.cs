using UnityEngine;

public class CardColor : MonoBehaviour
{
    public Color CurrentColor { get; private set; } = Color.white;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private Color debugColor = Color.white;
    
    private GameObject colorEffect;
    private GameObject debugCube;
    private Renderer debugRenderer;

    private bool allowDebugVisual = false;
    
    public void EnableDebugVisual() => allowDebugVisual = true;
    public void DisableDebugVisual() => allowDebugVisual = false;
    
    // Check if card has been colored (not white)
    public bool IsColored()
    {
        return CurrentColor != Color.white;
    }
    
    // Apply a new color to the card and update visual effects
    public void ApplyColor(Color newColor)
    {
        CurrentColor = newColor;
        
        // Remove old color effect object
        if (colorEffect != null)
            Destroy(colorEffect);
            
        // Create new color effect object
        colorEffect = CreateColorEffect(newColor);
        
        // Update debug color visualization
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
    
    // Update visual feedback based on tracking quality
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
        // Wird in Phase 2 implementiert - vorerst nur Debug
        if (debugMode)
        {
            Debug.Log($"🎨 Karte würde eingefärbt: {color}");
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
    }
    
    void OnDestroy()
    {
        // Clean up objects
        if (debugCube != null)
            Destroy(debugCube);
        if (colorEffect != null) 
            Destroy(colorEffect);
    }
}