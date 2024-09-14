// Created by: Unknown
// Edited by: Bill D.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    [RequireComponent(typeof(Camera))]
    public class MoveCameraWithArrowKeys : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotateSpeed = 75f;
        private Vector3 input;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Update() 
        {
            if(!Input.GetKey(KeyCode.LeftShift))
            {
                input = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"))
                    + (transform.up * (Input.GetKey(KeyCode.E) ? 1f : (Input.GetKey(KeyCode.Q) ? -1f : 0f)));
                input.Normalize();
                cam.transform.position += moveSpeed * Time.deltaTime * input;
            }
            else
            {
                input = new Vector3(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0.0f);
                cam.transform.Rotate(rotateSpeed * Time.deltaTime * input);
                cam.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, 0);
            }
        }
    }
}