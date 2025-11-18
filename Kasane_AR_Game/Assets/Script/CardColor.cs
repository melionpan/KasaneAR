using UnityEngine;

public class CardColor : MonoBehaviour
{
    public Color CurrentColor { get; private set; } = Color.white;
    private GameObject colorEffect;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private Color debugColor = Color.white;
    
    private GameObject debugCube;
    private Renderer debugRenderer;
    
    public bool IsColored()
    {
        return CurrentColor != Color.white;
    }
    
    public void ApplyColor(Color newColor)
    {
        CurrentColor = newColor;
        
        // Altes Farb-Objekt entfernen
        if (colorEffect != null)
            Destroy(colorEffect);
            
        // Neues farbiges Objekt spawnen (für Phase 2)
        colorEffect = CreateColorEffect(newColor);
        
        // Debug-Farbe aktualisieren
        if (debugMode && debugRenderer != null)
        {
            debugRenderer.material.color = newColor;
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
    
    void Start()
    {
        if (debugMode)
        {
            CreateDebugVisual();
        }
    }
    
    void CreateDebugVisual()
    {
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
    
    public void SetDebugColor(Color color)
    {
        debugColor = color;
        if (debugMode && debugRenderer != null)
        {
            debugRenderer.material.color = color;
        }
    }
    
    // Für besseres Tracking-Feedback
    public void SetTrackingQuality(float quality)
    {
        if (debugMode && debugRenderer != null)
        {
            // Farbe basierend auf Tracking-Qualität
            Color qualityColor = Color.Lerp(Color.red, Color.green, quality);
            debugRenderer.material.color = qualityColor;
        }
    }
    
    void OnDestroy()
    {
        // Aufräumen
        if (debugCube != null)
            Destroy(debugCube);
        if (colorEffect != null) 
            Destroy(colorEffect);
    }
}