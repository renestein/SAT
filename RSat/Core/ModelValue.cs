using System;

namespace RSat.Core
{
  public struct ModelValue
  {
    public ModelValue(string name, bool isTrue)
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

    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(IsTrue)}: {IsTrue}";
    }
  }
}