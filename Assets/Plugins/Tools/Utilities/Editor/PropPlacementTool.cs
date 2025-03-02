// Created by: Jason Bunn
// Edited by:
// Credits: Sebastian Lague https://github.com/SebLague/Object-Placement-with-Physics
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System.Linq;

namespace Skritty.Tools.Utilities
{
    public class PropPlacementTool : OdinEditorWindow
    {

        [DisplayAsString]
        public string instructions = "Select an object in the scene to simulate physics";

        public int maxIterations = 1000;
        public Vector2 forceMinMax;
        public float forceAngleInDegrees;
        public bool randomizeForceAngle;

        private SimulatedBody[] simulatedBodies;
        private List<Rigidbody> generatedRigidBodies;
        private List<Collider> generatedColliders;

        private static void OpenWindow()
        {
            GetWindow<PropPlacementTool>().Show();
        }

        [Button(ButtonSizes.Large)]
        [DisableIf("@Selection.activeGameObject == null")]
        public void SimulatePhysics() 
        {
            AutoGenerateComponents();
            simulatedBodies = FindObjectsOfType<Rigidbody>().Select(rb => 
                    new SimulatedBody(rb.GetComponent<Rigidbody>(), Selection.transforms.Any(x => rb.transform == x))
                    ).ToArray();

            // Add force to bodies
            float randomForceAmount, forceAngle = 0;
            Vector3 forceDir = Vector3.zero;
            foreach (SimulatedBody body in simulatedBodies)
            {  
                randomForceAmount = Random.Range(forceMinMax.x, forceMinMax.y);
                forceAngle = ((randomizeForceAngle) ? Random.Range(0, 360f) : forceAngleInDegrees) * Mathf.Deg2Rad;
                forceDir = new Vector3(Mathf.Sin(forceAngle), 0, Mathf.Cos(forceAngle));
                body.rigidbody.AddForce(forceDir * randomForceAmount, ForceMode.Impulse);   
            }

            // Run simulation for maxIteration frames, or until all child rigidbodies are sleeping
            Physics.simulationMode = SimulationMode.Script;
            
            for (int i = 0; i < maxIterations; i++)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                if (simulatedBodies.All(body => body.rigidbody.IsSleeping()))
                {
                    break;
                }
            }
            Physics.simulationMode = SimulationMode.FixedUpdate;

            foreach (SimulatedBody body in simulatedBodies)
            {
                if(!body.isSelected)
                {
                    body.Reset();
                }
            }

            RemoveAutoGeneratedComponents();
            
        }

        [Button(ButtonSizes.Medium)]
        [DisableIf("@Selection.activeGameObject == null")]
        public void ResetObjects()
        {
            if (simulatedBodies != null)
            {
                foreach (SimulatedBody body in simulatedBodies)
                {
                    body.Reset();
                }
            }
        }

        private void AutoGenerateComponents()
        {
            generatedRigidBodies = new List<Rigidbody>();
            generatedColliders = new List<Collider>();

            foreach (Transform child in Selection.transforms)
            {
                if (!child.GetComponent<Rigidbody>())
                {
                    generatedRigidBodies.Add(child.gameObject.AddComponent<Rigidbody>());
                }
                if (!child.transform.GetComponent<Collider>())
                {
                    generatedColliders.Add(child.gameObject.AddComponent<BoxCollider>());
                }
            }
        }

        private void RemoveAutoGeneratedComponents()
        {
            for (int i = 0; i < generatedRigidBodies.Count; i++)
            {
                DestroyImmediate(generatedRigidBodies[i]);
            }
            for (int i = 0; i < generatedColliders.Count; i++)
            {
                DestroyImmediate(generatedColliders[i]);
            }
        }

        struct SimulatedBody
        {
            public readonly Rigidbody rigidbody;
            public readonly bool isSelected;
            private readonly Vector3 originalPosition;
            private readonly Quaternion originalRotation;
            private readonly Transform transform;

            public SimulatedBody(Rigidbody rigidbody, bool isSelected)
            {
                this.rigidbody = rigidbody;
                this.isSelected = isSelected;
                transform = rigidbody.transform;
                originalPosition = rigidbody.position;
                originalRotation = rigidbody.rotation;
            }

            public void Reset()
            {
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                if(rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
