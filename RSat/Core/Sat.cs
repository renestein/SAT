using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RSat.Core
{
  public class Sat
  {
    private ClausuleSet? _clausulesSet;
    private readonly List<Clausule> _clausules;

    private readonly Func<ClausuleSet, ImmutableDictionary<string, Variable>, Model?>
      _solverStrategy;

    private ImmutableDictionary<string, Variable> _variablesMap;

    public Sat(Func<ClausuleSet, ImmutableDictionary<string, Variable>, Model?> solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = ImmutableDictionary<string, Variable>.Empty;
      _clausules = new List<Clausule>();
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
      _variablesMap = _variablesMap.Add(name, variable);
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

      _clausules.Add(new Clausule(literals.ToList()));
    }

    public bool Solve()
    {
      _clausulesSet = new ClausuleSet(_clausules);
      FoundModel = _solverStrategy(_clausulesSet, _variablesMap);
      return FoundModel != null;
    }
  }
}