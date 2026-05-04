using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingVisualizer : MonoBehaviour
{
    public static Targeting toVisualize;
    [SerializeReference]
    public Targeting toVisualizeManual;
    
    private void OnDrawGizmos()
    {
        toVisualize?.OnDrawGizmos(transform);
        toVisualizeManual?.OnDrawGizmos(transform);
    }
}
