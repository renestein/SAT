using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RSat.Core
{
  public class Sat
  {
    private readonly ImmutableList<ImmutableArray<Literal>> _clausules;

    private readonly Func<ImmutableList<ImmutableArray<Literal>>, IDictionary<string, Variable>, Model?>
      _solverStrategy;

    private readonly IDictionary<string, Variable> _variablesMap;

    public Sat(Func<ImmutableList<ImmutableArray<Literal>>, IDictionary<string, Variable>, Model?> solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = new Dictionary<string, Variable>();
      _clausules = ImmutableList<ImmutableArray<Literal>>.Empty;
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

      _clausules.Add(literals.ToImmutableArray());
    }

    public bool Solve()
    {
      FoundModel = _solverStrategy(_clausules, _variablesMap);
      return FoundModel != null;
    }
  }
}