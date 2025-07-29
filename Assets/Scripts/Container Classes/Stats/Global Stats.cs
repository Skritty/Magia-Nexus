public interface IStatTag : IModifiable, IDataContainer { }
public interface IStatTag<T> : IStatTag { }
public class Stat_Triggers : Stat<Alignment>, IStatTag<Alignment> { }
public class Stat_PreventExpire : EnumPrioritySolver<Alignment>, IStatTag<Alignment> { }
public class Stat_AoESize : NumericalSolver, IStatTag<float> { }
public class Stat_AdditionalTargets : NumericalSolver, IStatTag<float> { }
public class Stat_Removeable : NumericalSolver, IStatTag<float> { }
public class Stat_Knockback : NumericalSolver, IStatTag<float> { }
public class Stat_Enmity : NumericalSolver, IStatTag<float> { }
public class Stat_Untargetable : ListStat<(Entity, object)>, IStatTag<(Entity, object)> { }
public class Stat_TargetingMethod : PrioritySolver<Targeting>, IStatTag<Targeting> { }
