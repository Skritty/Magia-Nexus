using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

public abstract class StackBoolean
{
    private int amount;// TODO make this
    [ShowInInspector]
    public bool Value => amount > 0;
}