using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace RSat.Core
{
  public class Sat
  {
    private readonly Func<List<Literal[]>, IDictionary<string, Variable>, Model?> _solverStrategy;
    private readonly List<Literal[]> _clausules;
    private readonly IDictionary<string, Variable> _variablesMap;

    public Sat(Func<List<Literal[]>, IDictionary<string, Variable>, Model?>  solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = new Dictionary<string, Variable>();
      _clausules = new List<Literal[]>();
    }

    public Sat() : this(NaiveSolverStrategy.Solve)
    {
    }

    public Model? FoundModel
    {
      get;
      private set;
    } = null;

    public void CreateVariable(string name)
    {
      var variable = new Variable(name);
      _variablesMap.Add(name, variable);
    }

    public Variable GetVariable(string name)
    {
      return _variablesMap[name];
    }

    public void AddClausule(params Literal[] literals)
    {
      if (literals == null)
      {
        throw new ArgumentNullException(nameof(literals));
      }

      _clausules.Add(literals);
    }

    public bool Solve()
    {
      FoundModel = _solverStrategy(_clausules, _variablesMap);
      return FoundModel != null;
    }

  }
}