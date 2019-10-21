using System;

namespace RSat.Core
{
  public readonly struct Variable
  {
    public Variable(string name) : this(name, null)
    {
      
    }

    private Variable(string name,
                     bool? value)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Value = value;
    }

    public bool? Value
    {
      get;
    }

    public string Name
    {
      get;
    }

    public Variable TryTrueValue() => new Variable(Name, true);

    public Variable TryFalseValue() => new Variable(Name, false);


    public bool HasValue => Value.HasValue;
    

    public static implicit operator Literal(Variable variable) => new Literal(variable.Name, isTrue: true);

    public static Literal operator ~(Variable variable) => new Literal(variable.Name, isTrue: false);

  }
}