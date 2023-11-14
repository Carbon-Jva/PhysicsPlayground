/*
* PlatformMovement.cs
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
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
  private enum State
  {
    Stationary,
    MovimgToTarget,
    MovingToInitial
  }

  [Header("References")]
  [Tooltip("The Transform of the platform.")]
  public Transform trans;

  [Tooltip("The Rigidbody of the platform")]
  public Rigidbody rb;

  [Header("Stats")]
  [Tooltip("World-space position the platform should move to.")]
  public Vector3 targetPosition;

  [Tooltip("Amount of time taken to move from one position to the other.")]
  public float timeToChangePosition = 3f;

  [Tooltip("Time to wait after moving to a new position, before beginning to move to the next position.")]
  public float StationaryTime = 1f;

  //Returns the units to travel per second when moving.
  private float TravelSpeed
  {
    get
    {
      //Distance between the two positions, divided by number of seconds taken to change position:
      return Vector3.Distance(initialPosition, targetPosition) / timeToChangePosition;
    }
  }

  //Gets the current position we're moving towards based on state.
  private Vector3 CurrentDestination
  {
    get
    {
      if(state == State.MovingToInitial)
      {
        return initialPosition;
      }
      else
      {
        return targetPosition;
      }
    }
  }

  //Gets the current distance from our position to current destination.
  private float DistanceToDestination
  {
    get
    {
      return Vector3.Distance(trans.position, CurrentDestination);
    }
  }

  //World position of platform on Start:
  private Vector3 initialPosition;

  //Current state of the platforn
  private State state = State.Stationary;

  //State for the platform to use next - either MovingToTarget or MovingToInitial.
  private State nextState = State.MovimgToTarget;

  //Transitions 'state' to the 'nextState'.
  void GoToNextState()
  {
    state = nextState;
  } 

  //Unity events:

  // Start is called before the first frame update
  void Start()
  {
    //Mark the position of the platform at start:
    initialPosition = trans.position;

    //Invoke the first transition in state after 'stationaryTime' seconds:
    Invoke("GoToNextState", StationaryTime);
  }

  // Update is called once per frame
  void Update()
  {
      
  }

  void FixedUpdate()
  {
    if(state != State.Stationary)
    {
      //Set velocity to travel from our position towards the current destination by 'TraveSpeed' per second:
      rb.velocity = (CurrentDestination - trans.position).normalized * TravelSpeed;

      //Calculate how much distance our velocity is going to move us this frame:
      float distanceMovedThisFrame = (rb.velocity * Time.deltaTime).magnitude;

      //If the distance we'll move this Update is enough to reach the destination:
      if(distanceMovedThisFrame >= DistanceToDestination)
      {
        //Reset velocity to zero and snap us to the position so we don't overshoot it:
        rb.velocity = Vector3.zero;
        trans.position = CurrentDestination;

        //Based on our current state, determine what the next state will be:
        if(state == State.MovingToInitial)
        {
          nextState = State.MovimgToTarget;
        }
        else
        {
          nextState = State.MovingToInitial;
        }
        //Become statiionary and invoke the transition to the next state in 'stationaryTime' seconds:
        state = State.Stationary;
        Invoke("GoToNextState", StationaryTime);
      }
    }
    else //If we are stationary
    {
      //Maintain velocity at 0 to prevent unwanted movement:
      rb.velocity = Vector3.zero;
    }
  }
}
