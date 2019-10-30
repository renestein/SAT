using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RSatLib.Dimacs;

namespace RSatLib.Core
{
  public class Sat
  {
    private ClauseSet? _clauseSet;
    private readonly HashSet<Clause> _clauses;

    private readonly Func<ClauseSet, Variables, Model?> _solverStrategy;
    private readonly Variables _variablesMap;
    private readonly Dictionary<string, LiteralsToClausesMap> _varClausesMap;

    public Sat(Func<ClauseSet, Variables, Model?> solverStrategy)
    {
      _solverStrategy = solverStrategy ?? throw new ArgumentNullException(nameof(solverStrategy));
      _variablesMap = new Variables();
      _clauses = new HashSet<Clause>();
      _varClausesMap = new Dictionary<string, LiteralsToClausesMap>();
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
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
      }

      _varClausesMap.Add(name, new LiteralsToClausesMap(name));
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

      var clause = new Clause(literals.ToList());
      _clauses.Add(clause);
      foreach (var literal in literals)
      {
        if (literal.IsTrue)
        {
          _varClausesMap[literal.Name].ClausesWithPositiveLiterals.Add(clause);
        }
        else
        {
          _varClausesMap[literal.Name].ClausesWithNegativeLiterals.Add(clause);
        }
      }
    }

    public bool Solve()
    {
      var stopWatch = new Stopwatch();
      stopWatch.Start();
      _clauseSet = new ClauseSet(_clauses, _varClausesMap,  _varClausesMap.Keys.ToArray());

      FoundModel = _solverStrategy(_clauseSet, _variablesMap);
      stopWatch.Stop();
      Console.WriteLine($"Sat - elapsedTime (ms): {stopWatch.ElapsedMilliseconds}");
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