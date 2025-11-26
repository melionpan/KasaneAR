using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class CardOverlapManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardTracker cardTracker;
    [SerializeField] private GameObject mixingEffectPrefab;
    
    [Header("Spawnable Objects")]
    [SerializeField] private GameObject greenObjectPrefab;
    [SerializeField] private GameObject brownObjectPrefab;
    [SerializeField] private GameObject orangeObjectPrefab;
    [SerializeField] private GameObject purpleObjectPrefab;
    
    [Header("Settings")]
    [SerializeField] private float overlapDistance = 0.05f;
    [SerializeField] private float mixDuration = 1.5f;
    [SerializeField] private float effectHeightOffset = 0.02f;
    
    private Dictionary<(ARTrackedImage, ARTrackedImage), MixSession> activeMixes = new();
    
    void Update()
    {
        CheckCardOverlaps();
    }
    
    void CheckCardOverlaps()
    {
        var cards = cardTracker.GetAllTrackedCards();
        var processedPairs = new HashSet<(ARTrackedImage, ARTrackedImage)>();
        
        foreach (var card1 in cards)
        {
            foreach (var card2 in cards)
            {
                if (card1.Key == card2.Key) continue;
                
                var pair = GetOrderedPair(card1.Key, card2.Key);
                if (processedPairs.Contains(pair)) continue;
                processedPairs.Add(pair);
                
                if (card1.Value == null || card2.Value == null) continue;
                
                float distance = Vector3.Distance(card1.Value.transform.position, card2.Value.transform.position);
                
                if (distance < overlapDistance)
                {
                    HandleCardOverlap(pair, card1.Value, card2.Value);
                }
                else
                {
                    HandleCardSeparation(pair);
                }
            }
        }
        
        CleanupSeparatedCards(processedPairs);
    }
    
    void HandleCardOverlap((ARTrackedImage, ARTrackedImage) pair, GameObject visual1, GameObject visual2)
    {
        if (!activeMixes.ContainsKey(pair))
        {
            CardColor color1 = visual1.GetComponent<CardColor>();
            CardColor color2 = visual2.GetComponent<CardColor>();
            
            if (color1 != null && color2 != null && color1.IsColored() && color2.IsColored())
            {
                CreateMixSession(pair, color1.currentColor, color2.currentColor, 
                    visual1.transform.position, visual2.transform.position);
            }
        }
        else
        {
            UpdateMixPosition(pair, visual1.transform.position, visual2.transform.position);
        }
    }
    
    void CreateMixSession((ARTrackedImage, ARTrackedImage) pair, Color color1, Color color2, Vector3 pos1, Vector3 pos2)
    {
        Vector3 mixPosition = CalculateEffectPosition(pos1, pos2);
    
        GameObject mixEffect = Instantiate(mixingEffectPrefab, mixPosition, Quaternion.identity);
    
        Color mixedColor = ColorMixingRules.MixColors(color1, color2);
        
        ApplyColorToParticleSystem(mixEffect, mixedColor);
        
        activeMixes[pair] = new MixSession {
            MixEffect = mixEffect,
            StartTime = Time.time,
            MixedColor = mixedColor,
            SpawnPosition = mixPosition,
            HasSpawned = false
        };
    
        Debug.Log($"Started mixing: {color1} + {color2} = {mixedColor}");
    }
    
    Vector3 CalculateEffectPosition(Vector3 pos1, Vector3 pos2)
    {
        Vector3 mixPosition = (pos1 + pos2) * 0.5f;
        float higherY = Mathf.Max(pos1.y, pos2.y);
        mixPosition.y = higherY + effectHeightOffset;
        return mixPosition;
    }
    
    void ApplyColorToParticleSystem(GameObject effectObj, Color color)
    {
        ParticleSystem ps = effectObj.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            
            if (!ps.isPlaying)
                ps.Play();
        }
    }
    
    void UpdateMixPosition((ARTrackedImage, ARTrackedImage) pair, Vector3 pos1, Vector3 pos2)
    {
        if (activeMixes.TryGetValue(pair, out MixSession session))
        {
            Vector3 mixPosition = CalculateEffectPosition(pos1, pos2);
            session.MixEffect.transform.position = mixPosition;
            session.SpawnPosition = mixPosition;
            
            if (!session.HasSpawned && Time.time - session.StartTime >= mixDuration)
            {
                SpawnMixedObject(session.MixedColor, session.SpawnPosition);
                session.HasSpawned = true;
            }
        }
    }
    
    void SpawnMixedObject(Color mixedColor, Vector3 position)
    {
        GameObject prefab = GetPrefabForColor(mixedColor);
        if (prefab != null)
        {
            GameObject spawnedObject = Instantiate(prefab, position, Quaternion.identity);
            spawnedObject.transform.localScale = Vector3.one * 0.1f;
        
            RegisterWithShakeDetection(spawnedObject);
            MakeObjectInteractive(spawnedObject);
            StartCoroutine(SpawnAnimation(spawnedObject));
            Debug.Log($"Spawned: {prefab.name} at scale {spawnedObject.transform.localScale}");
        }
    }
    
    private void RegisterWithShakeDetection(GameObject obj)
    {
        if (ShakeDetection.instance != null)
        {
            ShakeDetection.instance.RegisterSpawnedObject(obj);
        }
        else
        {
            Debug.LogError("ShakeDetection instance is null! Cannot register object.");
        }
    }
    
    void MakeObjectInteractive(GameObject obj)
    {
        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
        }
        obj.AddComponent<ObjectInteraction>();
    }
    
    IEnumerator SpawnAnimation(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        obj.transform.localScale = originalScale;
    }
    
    GameObject GetPrefabForColor(Color color)
    {
        if (IsColorSimilar(color, Color.green)) return greenObjectPrefab;
        if (IsColorSimilar(color, new Color(0.5f, 0.25f, 0f))) return brownObjectPrefab;
        if (IsColorSimilar(color, new Color(1f, 0.5f, 0f))) return orangeObjectPrefab;
        if (IsColorSimilar(color, new Color(0.5f, 0f, 0.5f))) return purpleObjectPrefab;
        return null;
    }
    
    bool IsColorSimilar(Color a, Color b, float tolerance = 0.15f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
    
    void HandleCardSeparation((ARTrackedImage, ARTrackedImage) pair)
    {
        if (activeMixes.TryGetValue(pair, out MixSession session))
        {
            if (session.MixEffect != null)
            {
                Destroy(session.MixEffect);
            }
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
            HandleCardSeparation(pair);
        }
    }
    
    (ARTrackedImage, ARTrackedImage) GetOrderedPair(ARTrackedImage card1, ARTrackedImage card2)
    {
        return card1.GetInstanceID() < card2.GetInstanceID() ? (card1, card2) : (card2, card1);
    }
    
    private class MixSession
    {
        public GameObject MixEffect;
        public float StartTime;
        public Color MixedColor;
        public Vector3 SpawnPosition;
        public bool HasSpawned;
    }
}