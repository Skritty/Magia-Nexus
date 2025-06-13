using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Phases/Generic Phase")]
public class Phase : ScriptableObject
{
    public Phase nextPhase;
    public float tickDuration;
    public string sceneToLoad;

    public virtual void OnEnter()
    {
        
    }

    public virtual void OnExit()
    {
        
    }
}