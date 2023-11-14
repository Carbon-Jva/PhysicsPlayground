/*
* ForceField.cs
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

public class ForceField : MonoBehaviour
{
  [Tooltip("Should the force field affect the palyer?")]
  public bool affectsPlayer = true;

  [Tooltip("Should the force field affect Rigidbodies?")]
  public bool affectRigidbodies = true;

  [Tooltip("Method of applying force.")]
  public ForceMode forceMode;

  [Tooltip("Amount of force applied.")]
  public Vector3 force;

  [Tooltip("Should the force be applied in world space or local space realtive to this Transform's facing?")]
  public Space forceSpace = Space.World;

  //Gets the force in world space.
  public Vector3 ForceInWorldSpace
  {
    get
    {
      //If it's world-space we can just return 'force' as-is:
      if(forceSpace == Space.World)
      {
        return force;
      }
      //If it's local space, we use our transform to convert 'force' from local to world space:
      else
      {
        return transform.TransformDirection(force);
      }
    }
  }

  void OnColliderTouched(Collider other)
  {
    //If we affect the player,
    if(affectsPlayer)
    {
      // check for a Player component on the other collider's GameObject:
      var player = other.GetComponent<Player>();

      //If we found one, call AddVelocity:
      if(player != null)
      {
        //If the force mode is a constant push mode, use Time.deltaTime to make the force "per second".
        if(forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration)
        {
          player.AddVelocity(ForceInWorldSpace * Time.deltaTime);
        }
        else //Otherwise, use the force as-is.
        {
          player.AddVelocity(ForceInWorldSpace);
        }
      }
    }

    //If we affect Rigidbodies,
    if(affectRigidbodies)
    {
      // check for a Rigidbody component on the other collider's GameObject:
      var rb = other.GetComponent<Rigidbody>();

      //If we found one, call AddForce:
      if(rb != null)
      {
        rb.AddForce(ForceInWorldSpace, forceMode);
      }
    }
  }

  void OnTriggerEnter(Collider other)
  {
    //Impulse and VelocityChange will apply force only when the trigger is first entered.
    if(forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange)
    {
      OnColliderTouched(other);
    }
  }

  void OnTriggerStay(Collider other)
  {
    //Acceleration and Force modes will apply force constantly as long as the collision stay in contact.
    if(forceMode == ForceMode.Acceleration || forceMode == ForceMode.Force)
    {
      OnColliderTouched(other);
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    
  }
}
