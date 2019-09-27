using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace RSat.Core
{
  public readonly struct Literal : IEquatable<Literal>
  {
    public Literal(string name, bool isTrue)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      IsTrue = isTrue;
      IsFalse = !isTrue;
      IsValid = true;
    }

    public bool IsValid
    {
      get;
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

    public bool Equals(Literal other)
    {
      return string.Equals(Name, other.Name) && IsTrue == other.IsTrue;
    }

    public override bool Equals(object obj)
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

    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(IsTrue)}: {IsTrue}";
    }
  }
}