using System;

namespace RSat.Core
{
  public readonly struct Variable : IEquatable<Variable>
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

    public bool Equals(Variable other)
    {
      return Value == other.Value && Name == other.Name;
    }

    public override bool Equals(object obj)
    {
      return obj is Variable other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Value.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
      }
    }

    public static bool operator ==(Variable left,
                                   Variable right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Variable left,
                                   Variable right)
    {
      return !left.Equals(right);
    }
  }
}