using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RSat.Dimacs;

namespace RSat.Core
{
  public class Sat
  {
    private ClauseSet? _clauseSet;
    private readonly List<Clause> _clauses;

    private readonly Func<ClauseSet, Variables, Model?> _solverStrategy;

    private Variables _variablesMap;

    public Sat(Func<ClauseSet, Variables, Model?> solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = new Variables();
      _clauses = new List<Clause>();
    }

    public Sat() : this(NaiveSolverStrategy.Solve)
    {
    }

    public Model? FoundModel
    {
      get;
      private set;
    }

    public Variable CreateVariable(string name)
    {
      return _variablesMap.Add(name);
    }

    public Variable GetVariable(string name)
    {
      return _variablesMap[name];
    }

    public void AddClause(params Literal[] literals)
    {
      if (literals == null)
      {
        throw new ArgumentNullException(nameof(literals));
      }

      _clauses.Add(new Clause(literals.ToList()));
    }

    public bool Solve()
    {
      _clauseSet = new ClauseSet(_clauses);
      FoundModel = _solverStrategy(_clauseSet, _variablesMap);
      return FoundModel != null;
    }

    public static Task<Sat> FromStream(Stream stream)
    {
      return DimacsParser.Default.ParseCnf(stream);
    }

    public static Task<Sat> FromFile(string path)
    {
      return FromStream(new FileStream(path,
                                       FileMode.Open,
                                       FileAccess.Read,
                                       FileShare.Read));
    }

    
  }
}