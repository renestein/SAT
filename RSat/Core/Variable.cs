using System;

namespace RSat.Core
{
  public readonly struct Variable
  {
    [Flags]
    public enum AssignedValues
    {
      None = 0,
      False = 1,
      True = 2,
      All = False | True
    }

    public Variable(string name) : this(name, AssignedValues.None)
    {
      
    }

    private Variable(string name,
                     AssignedValues assignedValues)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      UsedValues = assignedValues;
    }

    public AssignedValues UsedValues
    {
      get;
    }

    public string Name
    {
      get;
    }

    public Variable TryTrueValue()
    {
      return new Variable(Name, UsedValues | AssignedValues.True);
    }

    public Variable TryFalseValue()
    {
      return new Variable(Name, UsedValues | AssignedValues.False);
    }

    public  bool AllValuesUsed()
    {
      return UsedValues == AssignedValues.All;
    }

    public bool TrueValueUsed()
    {
      return UsedValues.HasFlag(AssignedValues.True);
    }

    public bool FalseValueUsed()
    {
      return UsedValues.HasFlag(AssignedValues.False);
    }

    public bool AnyValueUsed()
    {
      return UsedValues != AssignedValues.None;
    }
    public bool NoneValuesUsed()
    {
      return UsedValues == AssignedValues.None;
    }

    public static implicit operator Literal(Variable variable) => new Literal(variable.Name, isTrue: true);

    public static Literal operator ~(Variable variable) => new Literal(variable.Name, isTrue: false);

  }
}