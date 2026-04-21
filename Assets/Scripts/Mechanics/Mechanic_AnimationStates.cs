using Sirenix.OdinInspector;
using UnityEngine;

public class Mechanic_AnimationStates : Mechanic
{
    [FoldoutGroup("Animation States")]
    public Animator animator;
    [FoldoutGroup("Animation States")]
    public AnimatorOverrideController animationSkin;
    private AnimationState _animationState;

    public override void Initialize()
    {
        base.Initialize();
        animator.runtimeAnimatorController = animationSkin;
    }

    public AnimationState AnimationState
    {
        get => _animationState;
        set
        {
            _animationState = value;
            if(value != AnimationState.None)
                animator.SetTrigger(value.ToString());
        }
    }
}

public enum AnimationState
{
    None,
    Move,
    Stunned,
    AttackWindup,
    AttackFinish,
    CastingWindup,
    CastingFinish
}
