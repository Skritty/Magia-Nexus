using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingVisualizer : MonoBehaviour
{
    public static MultiTargeting toVisualize;
    [SerializeReference]
    public MultiTargeting toVisualizeManual;
    
    private void OnDrawGizmos()
    {
        toVisualize?.OnDrawGizmos(transform);
        toVisualizeManual?.OnDrawGizmos(transform);
    }
}
