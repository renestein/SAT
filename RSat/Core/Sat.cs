using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RSat.Core
{
  public class Sat
  {
    private readonly ImmutableList<ImmutableList<Literal>> _clausules;

    private readonly Func<ImmutableList<ImmutableList<Literal>>, ImmutableDictionary<string, Variable>, Model?>
      _solverStrategy;

    private readonly ImmutableDictionary<string, Variable> _variablesMap;

    public Sat(Func<ImmutableList<ImmutableList<Literal>>, ImmutableDictionary<string, Variable>, Model?> solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = ImmutableDictionary<string, Variable>.Empty;
      _clausules = ImmutableList<ImmutableList<Literal>>.Empty;
    }

    public Sat() : this(NaiveSolverStrategy.Solve)
    {
    }

    public Model? FoundModel
    {
      get;
      private set;
    }

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

      _clausules.Add(literals.ToImmutableList());
    }

    public bool Solve()
    {
      FoundModel = _solverStrategy(_clausules, _variablesMap);
      return FoundModel != null;
    }
  }
}