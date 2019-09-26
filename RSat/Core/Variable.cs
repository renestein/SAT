using System;

namespace RSat.Core
{
  public readonly struct Variable
  {

    public Variable(string name)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name
    {
      get;
    }

    public static implicit operator Literal(Variable variable) => new Literal(variable.Name, isTrue: true);

    public static Literal operator ~(Variable variable) => new Literal(variable.Name, isTrue: false);

  }
}