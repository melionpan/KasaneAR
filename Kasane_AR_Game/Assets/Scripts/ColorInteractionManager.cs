using UnityEngine;
using System.Collections.Generic;

// Manages interactions between cards and color pots.
// Detects when cards enter pot activation zones and applies colors.
public class ColorInteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardTracker cardDetection;
    [SerializeField] private ColorPotSpawner potSpawner;
    [SerializeField] private AudioSource cardInteractionColoredSound;

    [Header("Recoloring Settings")]
    [SerializeField] private bool allowMultipleRecoloring = true;
    [SerializeField] private bool resetToWhiteFirst;
    
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
            if (cardColor == null) continue;
            
            Vector3 cardWorldPosition = cardVisual.transform.position;
            
            foreach (var pot in pots)
            {
                if (pot.potObject == null) continue;
                
                float distance = Vector3.Distance(cardWorldPosition, pot.potObject.transform.position);
                
                if (distance < pot.activationRadius)
                {
                    HandleCardRecoloring(cardColor, pot);
                    break;
                }
            }
        }
    }
    
    private void HandleCardRecoloring(CardColor cardColor, ColorPotSpawner.ColorPot pot)
    {
        Debug.Log($"SUCCESS! Card touching {pot.color} pot! Current card color: {cardColor.currentColor}");
        
        // Check if color would actually change
        if (cardColor.WouldChangeColor(pot.color))
        {
            // Option A: Reset to white first
            if (resetToWhiteFirst && cardColor.IsColored())
            {
                cardColor.ResetColor();
                Debug.Log("Card reset to white before applying new color");
            }
            
            // Apply the new color
            cardColor.ApplyColor(pot.color);
            
            // Play sound effect
            if (cardInteractionColoredSound != null)
            {
                Debug.Log("PLAY SOUND NOW!");
                cardInteractionColoredSound.Play();
            }
            else
            {
                Debug.Log("NO AUDIOSOURCE ASSIGNED!");
            }
            
            // Visual feedback
            StartCoroutine(PotInteractionFeedback(pot.potObject));
        }
        else
        {
            Debug.Log($"Card already has color {pot.color} - no change needed");
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
    
    public void SetAllowMultipleRecoloring(bool allow)
    {
        allowMultipleRecoloring = allow;
        Debug.Log($"Multiple recoloring: {(allow ? "ENABLED" : "DISABLED")}");
    }
    
    public void SetResetToWhiteFirst(bool resetFirst)
    {
        resetToWhiteFirst = resetFirst;
        Debug.Log($"Reset to white first: {(resetFirst ? "ENABLED" : "DISABLED")}");
    }
}