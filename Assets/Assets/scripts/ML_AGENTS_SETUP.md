# ML-Agents Setup Guide for Flappy Bread

This guide explains how to set up and train the ML-Agents agent for your Flappy Bread game.

## Prerequisites

1. **ML-Agents Package**: The ML-Agents package has been added to your project manifest.
2. **Python Environment**: You'll need Python 3.8+ with mlagents package installed.

## Setup Steps

### 1. Install ML-Agents Python Package

Open a terminal and install the ML-Agents Python package:

```bash
pip install mlagents
```

### 2. Configure Your Game Objects

#### Bread (Player)
1. Select your Bread prefab in the scene
2. Add the `FlappyBreadAgent` component
3. Assign references:
   - `Rb`: The Rigidbody2D component
   - `Game Manager`: Your GameManager component
   - Set `Jump Velocity` to match your current jump velocity (e.g., 1)

#### Pipes/Dogs
Make sure your pipe/dog prefabs have:
1. A Collider2D component
2. Tag set to "Pipe" (or update the agent script to match your tag)

#### Score Trigger
If you have a score trigger (the collider that detects passing obstacles):
1. Set its tag to "ScoreTrigger"
2. Make sure it's a Trigger collider

### 3. Create Tags (if not already created)

In Unity Editor:
1. Go to Edit > Project Settings > Tags and Layers
2. Add these tags if they don't exist:
   - "Pipe"
   - "Ground" (optional, for floor/ceiling)
   - "Ceiling" (optional, for ceiling)
   - "ScoreTrigger"

### 4. Set Up Behavior Parameters

1. Create a folder: `Assets/Assets/ML-Agents/` (optional but recommended)
2. In Unity, right-click in Project window > Create > ML-Agents > Behavior Parameters
3. Name it "FlappyBreadBehavior"
4. Configure:
   - **Behavior Name**: `FlappyBreadAgent`
   - **Vector Observation**: 
     - Space Size: 5 (agent position, velocity, obstacle distance, top gap, bottom gap)
   - **Actions**:
     - Discrete Actions: 1 branch, 2 actions (0 = no jump, 1 = jump)

5. Assign this Behavior Parameters asset to the FlappyBreadAgent component

### 5. Training Configuration

Create a training configuration file `trainer_config.yaml` in your project root or a separate training folder:

```yaml
behaviors:
  FlappyBreadAgent:
    trainer_type: ppo
    hyperparameters:
      batch_size: 64
      buffer_size: 2048
      learning_rate: 3.0e-4
      learning_rate_schedule: constant
      beta: 5.0e-3
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
    network_settings:
      normalize: false
      hidden_units: 128
      num_layers: 2
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 500000
    time_horizon: 64
    summary_freq: 10000
```

### 6. Train the Agent

1. **Build for Training** (optional, for faster training):
   - File > Build Settings
   - Add your scene
   - Build (save as `FlappyBreadTraining.exe` or similar)

2. **Start Training**:
   
   If using the Unity Editor:
   ```bash
   mlagents-learn trainer_config.yaml --run-id=FlappyBread_001
   ```
   Then press Play in Unity Editor.
   
   If using a build:
   ```bash
   mlagents-learn trainer_config.yaml --run-id=FlappyBread_001 --env=FlappyBreadTraining.exe
   ```

3. **Monitor Training**:
   Use TensorBoard to monitor training:
   ```bash
   tensorboard --logdir=results
   ```

### 7. Use Trained Model

After training:
1. The trained model will be in `results/FlappyBread_001/FlappyBreadAgent.onnx`
2. Copy this to your Unity project: `Assets/Assets/ML-Agents/models/`
3. In Unity:
   - Select your Bread GameObject
   - In FlappyBreadAgent component, set:
     - **Behavior Type**: Inference
     - **Model**: Drag your `.onnx` model here

## Customization

### Adjust Rewards

In `FlappyBreadAgent.cs`, you can modify:
- `survivalReward`: Reward per second alive (default: 0.1)
- `obstaclePassedReward`: Reward for passing an obstacle (default: 1.0)
- `deathPenalty`: Penalty for dying (default: -10.0)

### Adjust Observations

Modify `CollectObservations()` to add more information:
- Distance to multiple obstacles
- Agent rotation
- Game speed
- Time since last jump

### Testing Without Training

The agent includes heuristic (manual) control:
1. Set Behavior Type to Heuristic Only
2. You can control it with Space or Mouse Click (same as original)

## Troubleshooting

- **Agent not moving**: Check that Rigidbody2D is assigned and not kinematic
- **No observations**: Make sure Behavior Parameters Space Size matches observations (5)
- **Agent not learning**: Adjust hyperparameters, check reward values
- **Tags not found**: Create the tags in Project Settings > Tags and Layers

## Next Steps

- Train with multiple parallel environments for faster learning
- Add curriculum learning to progressively increase difficulty
- Experiment with different reward structures
- Add visual observations using camera sensors

