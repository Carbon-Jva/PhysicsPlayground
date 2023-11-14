/*
* PlatformDetector.cs
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

public class PlatformDetector : MonoBehaviour
{
  [Tooltip("Transform to move with the platform.")]
  public Transform trans;

  //The Transform of the platform we are currently standing on, if any:
  [HideInInspector] public Transform platform = null;

  //Position of the platform on the last Update.
  private Vector3 platformPreviousPosition;

  //True if we have set the position of the current platform.
  private bool firstPositionLogged = false;

  //Unity events:
  void FixedUpdate()
  {
    //If we are standing on a platform
    if(platform != null)
    {
      //If we have already logged the platform position at least once and it is not the same as its current platform.
      if(firstPositionLogged && platformPreviousPosition != platform.position)
      {
        //Add the change in platform position to our trans.position:
        trans.position += platform.position - platformPreviousPosition;
      }

      //Log the platform position this frame:
      platformPreviousPosition = platform.position;
      firstPositionLogged = true;
      //Mark that we have logged the position at least one
    }
    else //If we are not standing on a platform
    {
      //We'll mark that we have not set the platform's position yet.
      //When a new platform is assigned, we won't move the transform until this is set to 'true'.
      firstPositionLogged = false;
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
