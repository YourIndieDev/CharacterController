# Versatile Character Controller for Unity

A comprehensive and flexible character controller that provides smooth, responsive movement with support for multiple movement types and interactions.

## Features

- Smooth movement with acceleration/deceleration
- First-person camera controls
- Jumping mechanics
- Crouching system with camera adjustment
- Sprint functionality
- Air control for mid-jump movement
- Slope handling and ground detection
- Fully customizable through Unity Inspector
- Built-in movement smoothing
- Ground normal projection for realistic slope movement
- Debug visualization for ground checking

## Installation

1. Create a new script in your Unity project called `CharacterController.cs`
2. Copy the entire script content into this file
3. Create a player object in your scene with the following hierarchy:

PlayerObject
└── Main Camera

## Setup

1. Add required components to your player object:
   - The CharacterController script will automatically add a Rigidbody
   - Add a Capsule Collider or custom collider
   - Ensure you have a camera as a child object

2. Configure the ground layer:
   - Create a layer for ground objects (e.g., "Ground")
   - Set the ground objects to use this layer
   - In the CharacterController component, set the Ground Mask to include this layer

3. Configure the controller in the Unity Inspector:

### Movement Settings
- `Move Speed`: Base movement speed (default: 6)
- `Sprint Multiplier`: Speed multiplier when sprinting (default: 1.5)
- `Crouch Multiplier`: Speed multiplier when crouching (default: 0.5)
- `Jump Force`: Force applied when jumping (default: 5)
- `Air Control`: Movement control multiplier while in air (default: 0.3)

### Ground Check Settings
- `Ground Mask`: Layer mask for ground detection
- `Ground Check Distance`: How far to check for ground (default: 0.2)
- `Slope Limit`: Maximum angle for walkable slopes (default: 45)

### Movement Smoothing
- `Rotation Speed`: How quickly the player turns (default: 10)
- `Movement Smoothing`: Smoothing factor for acceleration/deceleration (default: 0.1)

### Camera Settings
- `Player Camera`: Reference to the camera transform
- `Mouse Sensitivity`: Mouse look sensitivity (default: 2)
- `Max Look Angle`: Maximum up/down look angle (default: 80)

## Usage

### Basic Controls
- `WASD`: Movement
- `Mouse`: Look around
- `Space`: Jump
- `C`: Toggle crouch
- `Left Shift`: Sprint

### Code Example - Basic Setup
```csharp
// Example of setting up the controller in code
public class GameManager : MonoBehaviour
{
    public CharacterController playerController;

    void Start()
    {
        // Customize controller settings
        playerController.SetMouseSensitivity(2.5f);
        playerController.SetMoveSpeed(8f);
    }
}
```

### Public Methods
```csharp
// Enable/disable movement
controller.SetMovementEnabled(bool enabled);

// Adjust mouse sensitivity
controller.SetMouseSensitivity(float sensitivity);

// Change movement speed
controller.SetMoveSpeed(float speed);

// Check player states
bool isGrounded = controller.IsGrounded();
bool isCrouching = controller.IsCrouching();
bool isSprinting = controller.IsSprinting();
```

## Best Practices

### 1. Physics Setup
- Set the Rigidbody's Collision Detection to Continuous
- Adjust the player's mass and drag values for desired feel
- Use physics materials to control friction

### 2. Performance
- Adjust the ground check distance based on your game's scale
- Use appropriate layer masks for ground checking
- Consider the performance impact of movement smoothing

### 3. Camera Setup
- Position the camera at the character's head level
- Adjust Field of View (FOV) for desired perspective
- Consider adding camera bob or effects for more immersion

## Common Issues and Solutions

### Player Sliding on Slopes
- Check the ground normal angle calculation
- Adjust the slope limit
- Verify physics material friction settings

### Jittery Movement
- Increase the movement smoothing value
- Check for framerate issues
- Verify rigidbody interpolation settings

### Camera Clipping
- Adjust the camera's near clip plane
- Check for colliders between the camera and player
- Verify camera position during crouching

## Extending the Controller

The controller can be extended with additional features such as:
- Footstep system
- Animation integration
- Wall running
- Sliding mechanics
- Climbing system
- Swimming capabilities
- Advanced state machine