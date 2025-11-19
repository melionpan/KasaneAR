using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CardTracker cardDetection;
    [SerializeField] private ColorPotSpawner potSpawner;
    [SerializeField] private ColorInteractionManager interactionManager;
    [SerializeField] private CardOverlapManager overlapManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject mixingEffectPrefab;
    
    void Start()
    {
        cardDetection.OnFirstCardDetected += OnFirstCardDetected;
        
        // Setup overlap manager
        if (overlapManager != null)
        {
            // Configure additional settings here
        }
    }
    
    void OnFirstCardDetected()
    {
        Debug.Log("First card detected - spawning pots");
        
        // Calculate spawn position based on detected card
        Vector3 cardsCenter = CalculateCardsCenter();
        float tableHeight = CalculateTableHeight();
        
        // Spawn color pots at calculated position
        potSpawner.SpawnPots(cardsCenter, tableHeight);
    }
    
    Vector3 CalculateCardsCenter()
    {
        var cards = cardDetection.GetAllTrackedCards();
        Vector3 center = Vector3.zero;
        int count = 0;
        
        foreach (var cardPair in cards)
        {
            center += cardPair.Key.transform.position;
            count++;
        }
        
        return count > 0 ? center / count : Vector3.zero;
    }
    
    float CalculateTableHeight()
    {
        var cards = cardDetection.GetAllTrackedCards();
        float height = 0f;
        int count = 0;
        
        foreach (var cardPair in cards)
        {
            height += cardPair.Key.transform.position.y;
            count++;
        }
        
        return count > 0 ? height / count : 0f;
    }
}