using System;

namespace RSat.SAT
{
  public struct Literal
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
  }
}