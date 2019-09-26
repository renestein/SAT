using System;

namespace RSat.SAT
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

  }
}