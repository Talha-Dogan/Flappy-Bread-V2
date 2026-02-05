using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FlappyBreadAgent : Agent
{
    [Header("References")]
    public Rigidbody2D rb;
    public GameManager gameManager;
    public float jumpVelocity = 1f;
    
    [Header("Training Settings")]
    public float survivalReward = 0.1f;
    public float obstaclePassedReward = 1f;
    public float deathPenalty = -10f;
    
    private Vector2 startPosition;
    private int lastScore = 0;
    private float timeAlive = 0f;
    
    // For observations - finding nearest obstacle
    private GameObject[] obstacles;
    
    public override void Initialize()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        
        startPosition = transform.position;
    }
    
    public override void OnEpisodeBegin()
    {
        // Reset agent position
        transform.position = startPosition;
        rb.velocity = Vector2.zero;
        
        // Reset score tracking
        lastScore = Score.score;
        Score.score = 0;
        timeAlive = 0f;
        
        // Reset game state if possible
        if (gameManager != null && gameManager.gameOverCanvas != null)
        {
            gameManager.gameOverCanvas.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's vertical position (normalized)
        sensor.AddObservation(transform.position.y / 5f);
        
        // Agent's vertical velocity
        sensor.AddObservation(rb.velocity.y / 10f);
        
        // Find nearest obstacle
        FindNearestObstacle(out float distanceX, out float topY, out float bottomY);
        
        // Distance to nearest obstacle (normalized)
        sensor.AddObservation(distanceX / 10f);
        
        // Top of obstacle gap (normalized)
        sensor.AddObservation(topY / 5f);
        
        // Bottom of obstacle gap (normalized)
        sensor.AddObservation(bottomY / 5f);
    }
    
    private void FindNearestObstacle(out float distanceX, out float topY, out float bottomY)
    {
        // Find all pipe obstacles
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("Pipe");
        if (pipes.Length == 0)
        {
            // If no pipes found, return default values
            distanceX = 10f;
            topY = 2f;
            bottomY = -2f;
            return;
        }
        
        // Find the nearest pipe that's ahead of the agent
        GameObject nearestPipe = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject pipe in pipes)
        {
            if (pipe == null) continue;
            
            float distance = pipe.transform.position.x - transform.position.x;
            
            // Only consider pipes ahead of the agent
            if (distance > 0 && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPipe = pipe;
            }
        }
        
        if (nearestPipe != null)
        {
            distanceX = nearestDistance;
            
            // Try to find the gap (top and bottom pipes)
            // This assumes pipes are set up in pairs
            Collider2D pipeCollider = nearestPipe.GetComponent<Collider2D>();
            if (pipeCollider != null)
            {
                // Get pipe bounds
                Bounds bounds = pipeCollider.bounds;
                
                // For a typical Flappy Bird setup, we need to check if it's top or bottom pipe
                // This is a simplified version - you may need to adjust based on your pipe setup
                if (nearestPipe.transform.position.y > 0)
                {
                    // Top pipe
                    topY = bounds.min.y; // Gap is below this
                    bottomY = bounds.min.y - 2f; // Assuming gap height of 2
                }
                else
                {
                    // Bottom pipe
                    bottomY = bounds.max.y; // Gap is above this
                    topY = bounds.max.y + 2f; // Assuming gap height of 2
                }
            }
            else
            {
                topY = 2f;
                bottomY = -2f;
            }
        }
        else
        {
            distanceX = 10f;
            topY = 2f;
            bottomY = -2f;
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete action: 0 = don't jump, 1 = jump
        int jumpAction = actions.DiscreteActions[0];
        
        if (jumpAction == 1)
        {
            // Jump
            rb.velocity = Vector2.up * jumpVelocity;
        }
        
        // Reward for staying alive
        AddReward(survivalReward * Time.fixedDeltaTime);
        
        // Check for score increase (passed obstacle)
        if (Score.score > lastScore)
        {
            AddReward(obstaclePassedReward);
            lastScore = Score.score;
        }
        
        timeAlive += Time.fixedDeltaTime;
        
        // Penalize for going too high or too low
        if (transform.position.y > 5f || transform.position.y < -5f)
        {
            AddReward(-0.5f);
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for testing
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            discreteActionsOut[0] = 1;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Hit an obstacle or ground/ceiling
        // Check if it's a pipe or any obstacle (fallback to name check if tag not set)
        bool isObstacle = collision.gameObject.CompareTag("Pipe") || 
                         collision.gameObject.CompareTag("Ground") || 
                         collision.gameObject.CompareTag("Ceiling") ||
                         collision.gameObject.name.ToLower().Contains("pipe") ||
                         collision.gameObject.name.ToLower().Contains("dog");
        
        if (isObstacle)
        {
            AddReward(deathPenalty);
            EndEpisode();
            
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Passed through an obstacle (score trigger)
        if (collision.CompareTag("ScoreTrigger"))
        {
            AddReward(obstaclePassedReward);
        }
    }
}

