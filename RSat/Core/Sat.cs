using System;
using System.Collections.Generic;

namespace RSat.Core
{
  public class Sat
  {
    private readonly IDictionary<string, Variable> _variablesMap;
    private readonly List<Literal[]> _clausules;

    public Sat()
    {
      _variablesMap = new Dictionary<string, Variable>();
      _clausules = new List<Literal[]>();
    }

    public void CreateVariable(string name)
    {
       var variable = new Variable(name);
      _variablesMap.Add(name, variable);
    }

    public Variable GetVariable(string name) => _variablesMap[name];

    public void AddLiterals(params Literal[] literals)
    {
      if (literals == null)
      {
        throw new ArgumentNullException(nameof(literals));
      }

      _clausules.Add(literals);
    }
  }
}