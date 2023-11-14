/*
* Player.cs
* Description of the content and purpose of the file.
*
* Copyright (c) 2023 Jimmy Vall
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
  //Varibles
  [Header("References")]
  [Tooltip("Reference to the root Transform, on which the Player script is attached.")]
  public Transform trans;

  [Tooltip("Reference to the Model Holder Transform. Movement will be local to the facing of this Transform.")]
  public Transform modelHolder;

  [Tooltip("Reference to the CharacterController component.")]
  public CharacterController charController;

  [Header("Gravity")]
  [Tooltip("Maximum downward momentum the player can have due to gravity.")]
  public float maxGravity = 92.0f;

  [Tooltip("Time taken for downward velocity to go from 0 to the maxGravity.")]
  public float timeToMaxGravity = .6f;

  //Property that gets the downward momentum per second to apply as gravity:
  public float GravityPerSecond
  {
    get
    {
      return maxGravity / timeToMaxGravity;
    }
  }

  //Y velocity is stored in a seperate float, apart from the velocity vector:
  private float yVelocity = 0.0f;

  [Header("Movement")]
  [Tooltip("Maximum ground speed per second with normal movement.")]
  public float movespeed = 42.0f;

  [Tooltip("Time taken, in seconds, to reach maximum speed from a stand-still")]
  public float timeToMaxSpeed = .3f;

  [Tooltip("Time taken, in seconds, to go from moving at full speed to a stand-still")]
  public float timeToLoseMaxSpeed = .2f;

  [Tooltip("Multiplier for additional velocity gain when moving against ongoing momentum. For example, 0 means no additional velocity, 0.5 means 50% extra, etc.")]
  public float reverseMomentumMultiplier = .6f;

  [Tooltip("Multiplier for velocity influence when moving while midair. For example, 0.5 means 50% speed. A value greater than 1 will make you move faster while midair.")]
  public float midairMovementMultiplier = .4f;

  [Tooltip("Multiplier for how much velocity is retained atfer bouncing off of a wall. For example, 1 is full velocity, 0.2 is 20%.")]
  [Range(0, 1)]
  public float bounciness = .2f;

  //Movement direction, local to the model holder facing:
  private Vector3 localMovementDirection = Vector3.zero;

  //Current world-space velocity; only the X and Z axes are used:
  private Vector3 worldVelocity = Vector3.zero;

  //True if we are currently on the ground, false if we are midair:
  private bool grounded = false;

  //Velocity gained per second. Applies midairMovementMultiplier when we are not grounded:
  public float VelocityGainPerSecond
  {
    get
    {
      if(grounded)
      {
        return movespeed / timeToMaxSpeed;
      }
      //Only use the midairMovementMultiplier if we are not grounded:
      else
      {
        return (movespeed / timeToMaxSpeed) * midairMovementMultiplier;
      }
    }
  }

  //Velocity lost per second based on movespeed and timeToLoseMaxSpeed:
  public float VelocityLossPerSecond
  {
    get{
      return movespeed / timeToLoseMaxSpeed;

    }
  }

  [Header("Jumping")]
  [Tooltip("Upward velocity provided on jump.")]
  public float jumpPower = 76f;

  [Header("Wall Jumping")]
  [Tooltip("Outward velocity provided by wall jumping.")]
  public float wallJumpPower = 40f;

  [Tooltip("Upward velocity provided by wall jumping.")]
  public float wallJumpAir = 56f;

  [Tooltip("Maximum distance from the player's side that a wall can be detected for a wall jump.")]
  public float wallDetectionRange = 2.4f;

  [Tooltip("Cooldown time for wall jumps, in seconds.")]
  public float wallJumpCooldown = .3f;

  [Tooltip("Only layers included in this mask will count as walls that can be jumped off.")]
  public LayerMask wallDetectionLayerMask;

  //Time.time when we last performed a wall jump.
  private float lastWallJumpTime;

  //Returns true if wall jump is not on cooldown, false if it is on cooldown.
  private bool WallJumpIsCooldown
  {
    get
    {
      //Current time must be greater than the last wall jump time, plus wall jump cooldown:
      return Time.time > lastWallJumpTime + wallJumpCooldown;
    }
  }

  private bool WallIsNearby()
  {
    return Physics.OverlapBox(trans.position + Vector3.up * (charController.height * .5f),
                              Vector3.one * wallDetectionRange,
                              modelHolder.rotation,
                              wallDetectionLayerMask.value).Length > 0;
  }

  public void AddVelocity(Vector3 amount)
  {
    //Add the velocity X and Z to our 'worldVelocity':
    worldVelocity += new Vector3(amount.x, 0, amount.z);

    //Add the velocity Y to our 'yVelocity':
    yVelocity += amount.y;

    //Ensure that we become midair if our Y velocity was raised above 0.
    //If we don't do this, it will be set to -1 again in ApplyVelocity if we are grounded.
    if(yVelocity > 0)
    {
      grounded = false;
    }
  }

  //Update Logic:
  void Movement()
  {
    //Every frame, we'll reset local movement direction to zero and set its X and Z based on WASD keys:
    localMovementDirection = Vector3.zero;
    //Right and left (D and A):
    if(Input.GetKey(KeyCode.D))
    {
      localMovementDirection.x = 1;
    }
    else if(Input.GetKey(KeyCode.A))
    {
      localMovementDirection.x = -1;
    }

    //Forward and back (W and S):
    if(Input.GetKey(KeyCode.W))
    {
      localMovementDirection.z = 1;
   }
    else if(Input.GetKey(KeyCode.S))
    {
      localMovementDirection.z = -1;
    }

    //If any of the movement keys are held this frame:
    if(localMovementDirection != Vector3.zero)
    {
      //Convert local movement direction to world direction, relative to the model holder:
      Vector3 worldMovementDirection = modelHolder.TransformDirection(localMovementDirection.normalized);

      //We'll calculate a multiplier to add the reverse momentum multiplier based on the direction we're trying to move.
      float multiplier = 1.0f;

      //Dot product will be 1 if moving directly towards existing velocity, 0 if moving perpendicular to existing velocity.
      // and -1 if moving directly away from existing velocity.
      float dot = Vector3.Dot(worldMovementDirection.normalized, worldVelocity.normalized);
      //Debug.Log("DOT product " + dot);
      //If we're moving away the the velociyty bay any amount,
      if(dot < 0)
      {
        //Now, flipping the 'dot' with a '-' makes it between 0 and 1.
        //Exactly 1 means moving directly away from existing momentum.
        //Thus, we'll get the full 'reverseMomentumMultiplier' only when it's 1.
        multiplier += -dot * reverseMomentumMultiplier;
      }

      //Calculate the new velocity by adding movement velocity to the current velocity:
      Vector3 newVelocity = worldVelocity + worldMovementDirection * VelocityGainPerSecond * multiplier * Time.deltaTime;
      //If world velocity is already moving more than 'movespeed' per second:
      if(worldVelocity.magnitude > movespeed)
      {
        //Clamp the magnitude at that of our world velocity:
        worldVelocity = Vector3.ClampMagnitude(newVelocity, worldVelocity.magnitude);
      }
      //If we aren't moving over 'movespeed' units per second yet,
      else
      {
        //Clamp the magnitude at a maximum of 'movespeed':
        worldVelocity = Vector3.ClampMagnitude(newVelocity, movespeed);
      }
    }
  }

  void VelocityLoss()
  {
    //Lose velocity as long we are grounded, and we either are not holding movement keys, or are moving faster than 'movespeed':
    if(grounded && (localMovementDirection == Vector3.zero || worldVelocity.magnitude > movespeed))
    {
      //Calculate velocity we'll be losing this frame:
      float VelocityLoss = VelocityLossPerSecond * Time.deltaTime;

      //If we're losing more velocity than the world velocity magnitude:
      if(VelocityLoss > worldVelocity.magnitude)
      {
        //Zerro out velocity so we're totally still:
        worldVelocity = Vector3.zero;
      }
      //Otherwise if we're losing less velocity:
      else
      {
        //Apply velocity in the opposite direction of the world velocity:
        worldVelocity -=worldVelocity.normalized * VelocityLoss;
      }
    }

  }

  void Gravity()
  {
    //While not grounded,
    if(!grounded && yVelocity > -maxGravity)
    {
      //Decrease Y velocity by GravityPerSecond, but don't go under -maxGravity:
      yVelocity = Mathf.Max(yVelocity - GravityPerSecond * Time.deltaTime, -maxGravity);
    }
  }

  void WallJumping()
  {
    //If midair and wall jump is off cooldown:
    if(!grounded && WallJumpIsCooldown)
    {
      //If space is pressed:
      if(Input.GetKeyDown(KeyCode.Space))
      {
        //Make sure a wall is nearby to jump off:
        if(WallIsNearby())
        {
          //If any movement keys are held,
          if(localMovementDirection != Vector3.zero)
          {
            //Apply outward movement by converting local movement direction to world-space
            // relative to the model holder, and multiplying by wall jump power:
            //worldVelocity = modelHolder.TransformDirection(localMovementDirection) * wallJumpPower;
            worldVelocity = modelHolder.TransformDirection(localMovementDirection) * (wallJumpPower + worldVelocity.magnitude);
          }
          //We'll also apply Y velocity. If we're falling,
          if(yVelocity <= 0)
          {
            // all downward momentum is replaced with the wall jump air:
            yVelocity = wallJumpAir;
          }
          //If not falling, just add wall jump air to existing velocity:
          else
          {
            yVelocity += wallJumpAir;
          }
          //Apply wall jump cooldown:
          lastWallJumpTime = Time.time;
        }
      }
    }
  }

  void Jumping()
  {
    if(grounded && Input.GetKeyDown(KeyCode.Space))
    {
      //Start traveling 'jumpPower' upwards per second:
      yVelocity = jumpPower;

      //Stop counting ourselves as grounded since we know we just jumped:
      grounded = false;
    }
  }

  void ApplyVelocity()
  {
    //While grounded, apply slight downward velocity to keep our grounded state correct:
    if(grounded)
    {
      yVelocity = -1.0f;
    }
    //Calculate the movement we'll receive this frame:
    Vector3 movementThisFrame = (worldVelocity + (Vector3.up * yVelocity)) * Time.deltaTime;

    //Calculate where we expected to be after moving if we don't hit anything:
    Vector3 predictedPosition = trans.position + movementThisFrame;

    //Only call Move if we have a minimum of 0.03 velocity:
    if(movementThisFrame.magnitude > .03f)
    {
      charController.Move(movementThisFrame);
    }

    //Checking grounded state:
    if(!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Below))
    {
      grounded = true;
    }
    else if(grounded && !charController.collisionFlags.HasFlag(CollisionFlags.Below))
    {
      grounded = false;
    }

    //Bounce off of walls when we hit our sides while midair:
    if(!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Sides))
    {
      worldVelocity = (trans.position - predictedPosition).normalized * (worldVelocity.magnitude * bounciness);
    }

    //Lose Y velocity if we're going up and collide with something above us:
    if(yVelocity > 0 && charController.collisionFlags.HasFlag(CollisionFlags.Above))
    {
      yVelocity = 0;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    Movement();

    VelocityLoss();

    Gravity();

    WallJumping();

    Jumping();

    ApplyVelocity();
  }
}
