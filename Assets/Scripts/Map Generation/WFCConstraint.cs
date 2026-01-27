using UnityEngine;

public abstract class WFCConstraint
{
    public abstract void ApplyConstraint((int, int, int) index);
}
