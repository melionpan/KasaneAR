using UnityEngine;
using System.Collections.Generic;

public class ColorPotSpawner : MonoBehaviour
{
    
    // Represents a color pot with its visual object and properties
    [System.Serializable]
    public class ColorPot
    {
        public GameObject potObject;
        public Color color;
        public float activationRadius = 0.05f;
    }
    
    [Header("Prefab References")]
    [SerializeField] private GameObject colorPotPrefab;
    
    [Header("Spawn Configuration")]
    [SerializeField] private Color[] potColors = {
        Color.white, Color.red, Color.blue, Color.green, 
        Color.yellow
    };
    [SerializeField] private float potSpacing = 0.08f;
    [SerializeField] private float zOffsetFromCards = 0.2f;
    
    private List<ColorPot> colorPots = new();    
    
    // Spawn color pots in a row relative to the cards' position
    public List<ColorPot> SpawnPots(Vector3 cardsCenter, float tableHeight)
    {
        // Clear existing pots before spawning new ones
        ClearPots();
        
        // Calculate spawn position
        Vector3 startPosition = cardsCenter;
        startPosition.y = tableHeight;          // Set to table height
        startPosition.z += zOffsetFromCards;    // Offset in front of cards
        
        // Calculate horizontal layout
        int totalPots = potColors.Length;
        float totalWidth = (totalPots - 1) * potSpacing;
        float startOffset = -totalWidth / 2f;   // Center the pots
        
        foreach (var pot in colorPots) 
		{
			pot.potObject.transform.SetParent(null); // detach from AR anchor
		}

        
        // Create each color pot
        for (int i = 0; i < potColors.Length; i++)
        {
            Vector3 potPosition = startPosition + Vector3.right * (startOffset + i * potSpacing);
            CreateColorPot(potColors[i], i, potPosition);
        }
        
        return colorPots;
    }
    
    // Remove all existing color pots
    public void ClearPots()
    {
        foreach (var pot in colorPots)
        {
            if (pot.potObject != null)
                Destroy(pot.potObject);
        }
        colorPots.Clear();
    }
    
    // Get list of all active color pots
    public List<ColorPot> GetColorPots() => colorPots;
        
    // Create an individual color pot with the specified properties
    void CreateColorPot(Color color, int index, Vector3 position)
    {
        if (colorPotPrefab == null) return;
        
        // Instantiate and position pot
        GameObject pot = Instantiate(colorPotPrefab, position, Quaternion.identity);
        pot.name = $"ColorPot_{color}_{index}";
        pot.transform.localScale = new Vector3(0.03f, 0.005f, 0.03f);
        
        // Set up visual appearance
        SetupPotVisual(pot, color);
        
        // Add interaction collider
        SetupPotCollider(pot);
        
        // Add to managed list
        colorPots.Add(new ColorPot {
            potObject = pot,
            color = color,
            activationRadius = 0.045f
        });
    }
    
    // Configure the visual appearance of a pot
    void SetupPotVisual(GameObject pot, Color color)
    {
        Renderer renderer = pot.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material newMat = new Material(renderer.material);
            newMat.color = color;
            newMat.EnableKeyword("_EMISSION");
            newMat.SetColor("_EmissionColor", color * 0.5f); // Add glow effect
            renderer.material = newMat;
        }
    }
    
    // Add trigger collider for interaction detection
    void SetupPotCollider(GameObject pot)
    {
        SphereCollider collider = pot.AddComponent<SphereCollider>();
        collider.radius = 0.5f;
        collider.isTrigger = true;
    }
}