using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Manages the training environment for ML-Agents
/// Can be used to set up multiple training instances
/// </summary>
public class TrainingEnvironment : MonoBehaviour
{
    [Header("Environment Settings")]
    public bool autoReset = true;
    public float maxEpisodeTime = 60f;
    
    private Academy academy;
    private float episodeStartTime;
    
    void Start()
    {
        academy = Academy.Instance;
        episodeStartTime = Time.time;
    }
    
    void Update()
    {
        // Auto-reset if episode runs too long
        if (autoReset && Time.time - episodeStartTime > maxEpisodeTime)
        {
            ResetEnvironment();
            episodeStartTime = Time.time;
        }
    }
    
    public void ResetEnvironment()
    {
        // This can be called to reset the environment
        // The agent's OnEpisodeBegin will handle most of the reset
        episodeStartTime = Time.time;
    }
    
    /// <summary>
    /// Call this when training is complete to set up for inference
    /// </summary>
    public void SetInferenceMode()
    {
        autoReset = false;
    }
}

