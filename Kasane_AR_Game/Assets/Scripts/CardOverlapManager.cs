using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class CardOverlapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardTracker cardTracker;
    [SerializeField] private GameObject mixingEffectPrefab;
    [SerializeField] private AudioSource mixSoundEffect;
    
    [Header("Overlap Settings")]
    [SerializeField] private float overlapCheckInterval = 0.2f;
    [SerializeField] private float maxOverlapDistance = 0.03f;
    
    private readonly Dictionary<(ARTrackedImage, ARTrackedImage), GameObject> activeMixes = new();
    
    void Start()
    {
        // Start checking for overlaps
        InvokeRepeating(nameof(CheckCardOverlaps), 0f, overlapCheckInterval);
    }
    
    void CheckCardOverlaps()
    {
        var cards = cardTracker.GetAllTrackedCards();
        var processedPairs = new HashSet<(ARTrackedImage, ARTrackedImage)>();
        
        // Check all card combinations for overlaps
        foreach (var card1 in cards)
        {
            foreach (var card2 in cards)
            {
                if (card1.Key == card2.Key) continue;
                
                // Create ordered pair to avoid duplicates
                var pair = GetOrderedPair(card1.Key, card2.Key);
                if (processedPairs.Contains(pair)) continue;
                
                processedPairs.Add(pair);
                
                // Get card visuals
                GameObject visual1 = card1.Value;
                GameObject visual2 = card2.Value;
                
                if (visual1 == null || visual2 == null) continue;
                
                Vector3 visual1Pos = visual1.transform.position;
                Vector3 visual2Pos = visual2.transform.position;
                
                // Check if cards are close enough to overlap
                float distance = Vector3.Distance(visual1Pos, visual2Pos);                
                Debug.Log($"Checking overlap between {card1.Key.referenceImage.name} and {card2.Key.referenceImage.name}: {distance:F3}m");
            
                if (distance < maxOverlapDistance)
                {
                    HandleCardOverlap(card1.Key, card2.Key, visual1, visual2, distance);
                }
                else
                {
                    HandleCardSeparation(card1.Key, card2.Key);
                }
            }
        }
        
        // Clean up separated pairs
        CleanupSeparatedCards(processedPairs);
    }
    void HandleCardOverlap(ARTrackedImage card1, ARTrackedImage card2, GameObject visual1, GameObject visual2, float distance)
    {
        var pair = GetOrderedPair(card1, card2);
        
        // If mix already exists, update position
        if (activeMixes.ContainsKey(pair))
        {
            UpdateMixPosition(pair, visual1.transform.position, visual2.transform.position);
            return;
        }
        
        // Check if cards can be mixed
        CardColor color1 = visual1.GetComponent<CardColor>();
        CardColor color2 = visual2.GetComponent<CardColor>();
        
        if (color1 == null || color2 == null) return;
        if (!color1.IsColored() || !color2.IsColored()) return;
        if (!ColorMixingRules.CanMix(color1.currentColor, color2.currentColor)) return;
        
        // Create color mix
        CreateColorMix(card1, card2, color1.currentColor, color2.currentColor, 
                      visual1.transform.position, visual2.transform.position);
    }
    
    void CreateColorMix(ARTrackedImage card1, ARTrackedImage card2, Color color1, Color color2, Vector3 pos1, Vector3 pos2)
    {
        var pair = GetOrderedPair(card1, card2);
        Color mixedColor = ColorMixingRules.MixColors(color1, color2);
        
        // Calculate mix position (midpoint between cards)
        Vector3 mixPosition = (pos1 + pos2) * 0.5f;
        mixPosition.y += 0.02f; // Add vertical offset to appear above cards
        
        // Create mixing effect
        GameObject mixEffect = Instantiate(mixingEffectPrefab, mixPosition, Quaternion.identity);
        mixEffect.name = $"ColorMix_{color1}_{color2}";
        
        // Scale down the effect
        mixEffect.transform.localScale = Vector3.one * 0.5f; // Adjust this value as needed
        
        // Set mixed color by modifying the particle system
        SetParticleSystemColor(mixEffect, mixedColor);
        
        // Set mixed color
        //Renderer renderer = mixEffect.GetComponent<Renderer>();
        //if (renderer != null)
        //{
        //    renderer.material.color = mixedColor;
        //}
        
        // Add tap to spawn functionality
        MixedColorObject mixedObject = mixEffect.AddComponent<MixedColorObject>();
        mixedObject.Initialize(mixedColor, pair);
        
        // Store reference
        activeMixes[pair] = mixEffect;
        
        // Play sound effect
        if (mixSoundEffect != null)
        {
            mixSoundEffect.Play();
        }
        
        Debug.Log($"Color mix created: {color1} + {color2} = {mixedColor}");
    }
    
    void SetParticleSystemColor(GameObject particleObject, Color color)
    {
        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.startColor = color;
        
            // You can also adjust other particle properties here
            mainModule.startSize = 0.05f; // Smaller particles
            mainModule.startSpeed = 0.2f; // Slower movement
        }
    }
    
    void UpdateMixPosition((ARTrackedImage, ARTrackedImage) pair, Vector3 pos1, Vector3 pos2)
    {
        if (activeMixes.TryGetValue(pair, out GameObject mixEffect))
        {
            // Use card visual positions, not AR marker positions
            Vector3 mixPosition = (pos1 + pos2) * 0.5f;
            mixPosition.y += 0.02f; // Always keep above cards
            mixEffect.transform.position = mixPosition;
        }
    }
    
    void HandleCardSeparation(ARTrackedImage card1, ARTrackedImage card2)
    {
        var pair = GetOrderedPair(card1, card2);
        
        if (activeMixes.TryGetValue(pair, out GameObject mixEffect))
        {
            // Cards separated - remove mix effect
            Destroy(mixEffect);
            activeMixes.Remove(pair);
        }
    }
    
    void CleanupSeparatedCards(HashSet<(ARTrackedImage, ARTrackedImage)> currentPairs)
    {
        var pairsToRemove = new List<(ARTrackedImage, ARTrackedImage)>();
        
        foreach (var pair in activeMixes.Keys)
        {
            if (!currentPairs.Contains(pair))
            {
                pairsToRemove.Add(pair);
            }
        }
        
        foreach (var pair in pairsToRemove)
        {
            if (activeMixes.TryGetValue(pair, out GameObject mixEffect))
            {
                Destroy(mixEffect);
                activeMixes.Remove(pair);
            }
        }
    }
    
    (ARTrackedImage, ARTrackedImage) GetOrderedPair(ARTrackedImage card1, ARTrackedImage card2)
    {
        // Ensure consistent ordering for dictionary keys
        return card1.GetInstanceID() < card2.GetInstanceID() 
            ? (card1, card2) 
            : (card2, card1);
    }
}