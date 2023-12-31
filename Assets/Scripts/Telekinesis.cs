/*
* Telekinesis.cs
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

public class Telekinesis : MonoBehaviour
{
  public enum State
  {
    Idle,
    Pushing,
    Pulling
  }

  private State state = State.Idle;

  [Header("References")]
  public Transform baseTrans;
  public Camera cam;

  [Header("Stats")]
  [Tooltip("Force applied when pulling a target.")]
  public float pullForce = 60f;

  [Tooltip("Force applied when pushing a target.")]
  public float pushForce = 60f;

  [Tooltip("Maximum distance from the player that a telekinesis target can be.")]
  public float range = 70f;

  [Tooltip("Layer mask for objects that can be pulled and pushed.")]
  public LayerMask detectionLayerMask;

  //Current target of telekinesis, if any.
  private Transform target;

  //The world position that the target detection ray hit on the current target.
  private Vector3 targetHitPoint;

  //Rigidbody component of target. For something to be marked as a target, it must have a Rigidbody.
  //So as long as 'target' is not null, this won't be null either.
  private Rigidbody targetRigidbody;

  //If there is no current target, this is always false. Otherwise, true if the target is in range, false if they are not.
  private bool targetIsOutsideRange = false;

  //Gets the Colour that the cursor should display based on the state and target distance.
  private Color CursorColor
  {
    get
    {
      if(state == State.Idle)
      {
        //If there is no target, return graey:
        if(target == null)
        {
          return Color.gray;
        }
        //If there is a target but it's not in range, return orange:
        else if(targetIsOutsideRange)
        {
          return new Color(1, .6f, 0);
        }
        //If there is a target and it is in range, return white:
        else
        {
          return Color.white;
        }
      }
      //If we're pushing or pulling, return green:
      else
      {
        return Color.green;
      }
    }
  }

  void ClearTarget()
  {
    //Clear and reset varibales that related to targeting:
    target = null;
    targetRigidbody = null;
    targetIsOutsideRange = false;
  }

  //Update logic:
  void TargetDetection()
  {
    //Get a ray going out of the center of the screen:
    var ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
    RaycastHit hit;

    //Cast the ray using detectionLayerMask:
    if(Physics.Raycast(ray, out hit, Mathf.Infinity, detectionLayerMask.value))
    {
      //If the ray using detectionLayerMask:
      if(hit.rigidbody != null && !hit.rigidbody.isKinematic) // and it has a  non-kinematic Rigidbody
      {
        //Set the telekinesis target:
        target = hit.transform;
        targetRigidbody = hit.rigidbody;
        targetHitPoint = hit.point;

        //Based on distance, set targetIsOutsideRange:
        if(Vector3.Distance(baseTrans.position, hit.point) > range)
        {
          targetIsOutsideRange = true;
        }
        else
        {
          targetIsOutsideRange = false;
        }
      }

      //If the thing the ray hit has no Rigidbody
      else
      {
        ClearTarget();
      }
    }
    else //If the ray didn't hit anything
    {
      ClearTarget();
    }
  }

  //FixedUpdate logic:
  void PullingAndPushing()
  {
    //If we have a target that is within range:
    if(target != null && !targetIsOutsideRange)
    {
      //If the left mouse button is down
      if(Input.GetMouseButton(0))
      {
        //Pull the target from the hit point towards our position:
        targetRigidbody.AddForce((baseTrans.position - targetHitPoint).normalized * pullForce,
                                  ForceMode.Acceleration);
        state = State.Pulling;
      }
      //Else if the right mouse button is down
      else if(Input.GetMouseButton(1))
      {
        //Push the target from our position towards the hit point:
        targetRigidbody.AddForce((targetHitPoint - baseTrans.position).normalized * pushForce,
                                  ForceMode.Acceleration);
        state = State.Pushing;
      }
      //If neither mouse buttons are held down
      else
      {
        state = State.Idle;
      }
    }
    //If we don't have a target or we have one but it is not in range:
    else
    {
      state = State.Idle;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
      
  }

  // Update is called once per frame
  void Update()
  {
    TargetDetection();
  }

  void FixedUpdate()
  {
    PullingAndPushing();
  }

  void OnGUI()
  {
    //Draw a 2x2 rectangle of the CursorColour at the center of the screen:
    UnityEditor.EditorGUI.DrawRect(new Rect(Screen.width * .5f, Screen.height * .5f, 2, 2), CursorColor);
  }
}
