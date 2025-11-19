using UnityEngine;
using System.Collections.Generic;

// Manages interactions between cards and color pots.
// Detects when cards enter pot activation zones and applies colors.
public class ColorInteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardTracker cardDetection;
    [SerializeField] private ColorPotSpawner potSpawner;
    
    void Update()
    {
        CheckCardPotInteractions();
    }
    
    void CheckCardPotInteractions()
    { 
		var cards = cardDetection.GetAllTrackedCards();
        var pots = potSpawner.GetColorPots();
        
        foreach (var cardPair in cards)
        {
            GameObject cardVisual = cardPair.Value;
            
            if (cardVisual == null) continue;
            
            CardColor cardColor = cardVisual.GetComponent<CardColor>();
            if (cardColor == null || cardColor.IsColored()) continue;
            
            // CRITICAL FIX: Use CARD VISUAL POSITION instead of AR position
            Vector3 cardWorldPosition = cardVisual.transform.position;
            
            Debug.Log($"Card Visual at: {cardWorldPosition}");
            
            // Check distance to each pot
            foreach (var pot in pots)
            {
                if (pot.potObject == null) continue;
                
                // USE CARD VISUAL POSITION for distance calculation
                float distance = Vector3.Distance(cardWorldPosition, pot.potObject.transform.position);
                
                // Debug when close
                if (distance < 0.3f) // Log if within 30cm
                {
                    Debug.Log($"Card-Pot distance: {distance:F3}m | Card: {cardWorldPosition} | Pot: {pot.potObject.transform.position}");
                }
                
                // Check if card is within activation radius of pot
                if (distance < pot.activationRadius)
                {
                    Debug.Log($"SUCCESS! Card touching {pot.color} pot! Distance: {distance:F3}m");
                    cardColor.ApplyColor(pot.color);
                    
                    // Visual feedback
                    StartCoroutine(PotInteractionFeedback(pot.potObject));
                    break;
                }
            }
        }
    }
    
    private System.Collections.IEnumerator PotInteractionFeedback(GameObject pot)
    {
        // Visual feedback when interaction happens
        Vector3 originalScale = pot.transform.localScale;
        pot.transform.localScale = originalScale * 1.3f;
        
        yield return new WaitForSeconds(0.3f);
        
        pot.transform.localScale = originalScale;
    }
}