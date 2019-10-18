using System;

namespace RSat.Core
{
  public class Literal : IEquatable<Literal>, IComparable<Literal>
  {
    public Literal(string name, bool isTrue)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      IsTrue = isTrue;
      IsFalse = !isTrue;
    }
    public string Name
    {
      get;
    }

    public bool IsTrue
    {
      get;
    }

    public bool IsFalse
    {
      get;
    }

    public bool IsNegationOf(Literal literal)
    {
      if (literal == null)
      {
        throw new ArgumentNullException(nameof(literal));
      }

      return literal.Name == Name && literal.IsTrue != IsTrue;
    }
    public bool Equals(Literal other)
    {
      return string.Equals(Name, other.Name) && IsTrue == other.IsTrue;
    }

    public override bool Equals(object? obj)
    {
      return obj is Literal other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Name.GetHashCode() * 397) ^ IsTrue.GetHashCode();
      }
    }



    public static implicit operator bool(Literal literal) => literal.IsTrue;

    public static Literal operator ~(Literal literal) => new Literal(literal.Name, !literal.IsTrue);
    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(IsTrue)}: {IsTrue}";
    }

    public int CompareTo(Literal other)
    {
      if (ReferenceEquals(this, other))
      {
        return 0;
      }

      if (ReferenceEquals(null, other))
      {
        return 1;
      }

      var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
      if (nameComparison != 0)
      {
        return nameComparison;
      }

      return IsTrue.CompareTo(other.IsTrue);
    }

  }
}