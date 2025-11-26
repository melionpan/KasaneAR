using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ShakeDetection : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeThreshold = 20.0f;
    [SerializeField] private float shakeCooldown = 2.0f;
    [SerializeField] private float accelerationMultiplier = 10f;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioClip shakeSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    public static ShakeDetection instance { get; private set; }
    
    private List<GameObject> spawnedObjects = new();
    private float lastShakeTime;
    private AudioSource audioSource;
    
    private void Awake()
    {
        InitializeSingleton();
        SetupAudioSource();
        CheckAccelerometerAvailability();
    }
    
    private void CheckAccelerometerAvailability()
    {
        if (Accelerometer.current != null)
        {
            Debug.Log("New Input System Accelerometer: AVAILABLE");
            InputSystem.EnableDevice(Accelerometer.current);
        }
        else
        {
            Debug.Log("New Input System Accelerometer: NOT AVAILABLE");
        }
        
        if (SystemInfo.supportsAccelerometer)
        {
            Debug.Log("Old Input System Accelerometer: AVAILABLE");
        }
        else
        {
            Debug.Log("Old Input System Accelerometer: NOT AVAILABLE");
        }
    }

    
    private void Update()
    {
        DetectShake();
        
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            ShowDebugInfo();
        }
        
#if UNITY_EDITOR
        HandleEditorInput();
#endif
    }
    
    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void DetectShake()
    {
        if (Time.time - lastShakeTime < shakeCooldown) return;
        
        Vector3 acceleration = GetAcceleration();
        float shakeIntensity = acceleration.magnitude;
        
        if (showDebugInfo && shakeIntensity > 0.1f)
        {
            Debug.Log($"Acceleration: {acceleration}, Intensity: {shakeIntensity:F2}");
        }
        
        if (shakeIntensity > shakeThreshold)
        {
            Debug.Log($"SHAKE DETECTED! Intensity: {shakeIntensity:F2}");
            OnShakeDetected();
            lastShakeTime = Time.time;
        }
    }
    
    private Vector3 GetAcceleration()
    {
        if (Accelerometer.current != null)
        {
            return Accelerometer.current.acceleration.ReadValue() * accelerationMultiplier;
        }
        
        return Input.acceleration * accelerationMultiplier;
    }
    
    private void OnShakeDetected()
    {
        Debug.Log("Shake detected! Removing all spawned objects.");
        
        PlayShakeSound();
        RemoveAllSpawnedObjects();
    }
    
    private void PlayShakeSound()
    {
        if (shakeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shakeSound);
        }
    }
    
    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj == null || spawnedObjects.Contains(obj)) return;
        
        spawnedObjects.Add(obj);
        Debug.Log($"Registered object: {obj.name}, Total objects: {spawnedObjects.Count}");
    }
    
    public void UnregisterSpawnedObject(GameObject obj)
    {
        if (obj == null) return;
        
        if (spawnedObjects.Contains(obj))
        {
            spawnedObjects.Remove(obj);
            Debug.Log($"Unregistered object: {obj.name}, Remaining objects: {spawnedObjects.Count}");
        }
    }
    
    public void RemoveAllSpawnedObjects()
    {
        if (spawnedObjects.Count == 0)
        {
            Debug.Log("No objects to remove");
            return;
        }
        
        Debug.Log($"Removing {spawnedObjects.Count} spawned objects");
        
        foreach (GameObject obj in spawnedObjects.ToArray())
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        spawnedObjects.Clear();
        Debug.Log("All objects removed successfully!");
    }
    
    private void ShowDebugInfo()
    {
        Vector3 acceleration = GetAcceleration();
        float intensity = acceleration.magnitude;
        
        Debug.Log($"Shake Debug - Intensity: {intensity:F2}, Threshold: {shakeThreshold}, " +
                  $"Objects: {spawnedObjects.Count}, Cooldown: {Time.time - lastShakeTime:F1}s");
    }
    
#if UNITY_EDITOR
    private void HandleEditorInput()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Editor test: R key pressed - simulating shake");
            OnShakeDetected();
        }
    }
#endif
    
    [ContextMenu("Manual Reset")]
    public void ManualReset()
    {
        Debug.Log("Manual reset triggered");
        OnShakeDetected();
    }
    
    [ContextMenu("Test Object Registration")]
    public void TestObjectRegistration()
    {
        Debug.Log("=== OBJECT REGISTRATION TEST ===");
        Debug.Log($"Total registered objects: {spawnedObjects.Count}");
        
        if (spawnedObjects.Count == 0)
        {
            Debug.Log("No objects registered!");
            return;
        }
        
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Debug.Log($"Registered: {obj.name} (Active: {obj.activeInHierarchy})");
                ObjectInteraction interaction = obj.GetComponent<ObjectInteraction>();
                Debug.Log(interaction != null ? " - Has ObjectInteraction component" : " - MISSING ObjectInteraction component!");
            }
            else
            {
                Debug.Log("NULL object in list");
            }
        }
        Debug.Log("=== END TEST ===");
    }
}