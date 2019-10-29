using System;
using System.Collections.Generic;
using System.Linq;

namespace RSat.Core
{
  public partial class ClauseSet
  {
    private readonly Dictionary<string, LiteralsToClausesMap> _clausesByLiterals;
    private readonly IEnumerable<string> _variableNames;

    public ClauseSet(HashSet<Clause> clonedClauses,
                     Dictionary<string, LiteralsToClausesMap> varClausesMap,
                     IEnumerable<string> variableNames) : this(clonedClauses,
                                                               varClausesMap,
                                                               variableNames,
                                                               true)
    {
    }

    public ClauseSet(HashSet<Clause> clonedClauses,
                     Dictionary<string, LiteralsToClausesMap> varClausesMap,
                     IEnumerable<string> variableNames,
                     bool preprocessFormula)
    {
      Clauses = clonedClauses ?? throw new ArgumentNullException(nameof(clonedClauses));
      _clausesByLiterals = varClausesMap ?? throw new ArgumentNullException(nameof(varClausesMap));
      _variableNames = variableNames ?? throw new ArgumentNullException(nameof(variableNames));
    }


    public bool HasClauses => Clauses.Count != 0;
    public int ClausesCount => Clauses.Count;

    public bool IsEmpty => !HasClauses;

    public HashSet<Clause> Clauses
    {
      get;
    }

    public Clause? SelectUnitClause(Variables variablesMap)
    {
      return Clauses.FirstOrDefault(clause => clause.IsUnitClause() &&
                                              !variablesMap[clause.FirstLiteral.Name].HasValue);
    }

    public Literal? SelectUnusedLiteral(Variables variablesMap)
    {
      //var name = _clausesByLiterals.Keys.FirstOrDefault(name => !variablesMap.HasValueFor(name));
      //return name == null
      //  ? null
      //  : (Literal)variablesMap[name];

      return selectUnusedLiteralWithMinClausule(variablesMap);
      
    }

    private Literal? selectUnusedLiteralWithMinClausule(Variables variablesMap)
    {
      return _clausesByLiterals.Values.Where(literalsMap => !variablesMap.HasValueFor(literalsMap.VariableName))
                               .SelectMany(literalsMap => new[]
                               {
                                 new
                                 {
                                   literal = (Literal) variablesMap[literalsMap.VariableName],
                                   minClausuleLength =
                                     literalsMap.ClausesWithPositiveLiterals.DefaultIfEmpty(Clause.EmptyClause)
                                                .Min(clause => clause.Literals.Count)
                                 },
                                 new
                                 {
                                   literal = ~variablesMap[literalsMap.VariableName],
                                   minClausuleLength =
                                     literalsMap.ClausesWithNegativeLiterals.DefaultIfEmpty(Clause.EmptyClause)
                                                .Min(clause => clause.Literals.Count)
                                 }
                               }).OrderBy(minClausules => minClausules.minClausuleLength)
                               .Select(minClausule => minClausule.literal)
                               .FirstOrDefault();
    }

    public void AddClause(Clause clause)
    {
      if (clause == null)
      {
        throw new ArgumentNullException(nameof(clause));
      }

      foreach (var literal in clause.Literals)
      {
        if (literal.IsTrue)
        {
          _clausesByLiterals[literal.Name].ClausesWithPositiveLiterals.Add(clause);
        }
        else
        {
          _clausesByLiterals[literal.Name].ClausesWithNegativeLiterals.Add(clause);
        }

        Clauses.Add(clause);
      }
    }


    public ClauseOperationResult DeleteLiteralFromClauses(Literal literal)
    {
      var literals = _clausesByLiterals[literal.Name];
      bool haveEmptyClauses;
      if (literal.IsTrue)
      {
        haveEmptyClauses = literals.ClausesWithPositiveLiterals.Select(clause =>
        {
          clause.DeleteLiteral(literal);
          return clause.IsEmptyClause();
        }).Any(emptyClauseFound => emptyClauseFound);

        literals.ClausesWithPositiveLiterals.Clear();
      }
      else
      {
        haveEmptyClauses = literals.ClausesWithNegativeLiterals.Select(clause =>
        {
          clause.DeleteLiteral(literal);
          return clause.IsEmptyClause();
        }).Any(emptyClauseFound => emptyClauseFound);
        literals.ClausesWithNegativeLiterals.Clear();
      }

      return haveEmptyClauses
        ? ClauseOperationResult.MinOneEmptyClausuleFound
        : ClauseOperationResult.OperationSuccess;
    }

    public bool IsConsistentSetOfLiterals()
    {
      return IsEmpty || hasOnlyConsistentLiterals();
    }

    public bool HasEmptyClause()
    {
      foreach (var clause in Clauses)
      {
        if (clause.IsEmptyClause())
        {
          return true;
        }
      }

      return false;
    }

    public IEnumerable<Literal> GetPureLiterals(Variables variablesMap)
    {
      return _clausesByLiterals.Values
                               .Select(varClauses => varClauses.TryGetPureLiteral(variablesMap))
                               .Where(literal => literal != null)!;
    }

    public void DeleteClausesWithLiteral(Literal pureLiteral)
    {
      var varClausules = _clausesByLiterals[pureLiteral.Name];

      var toDeleteClauses = pureLiteral.IsTrue
        ? varClausules.ClausesWithPositiveLiterals
        : varClausules.ClausesWithNegativeLiterals;

      foreach (var deleteClause in toDeleteClauses)
      {
        Clauses.Remove(deleteClause);
      }

      toDeleteClauses.Clear();
    }

    public ClauseSet CloneWithClause(Clause clause)
    {
      var (clonedClauses, varClausesMap) = cloneInternal();

      if (clause != null)
      {
        clonedClauses.Add(clause);
      }

      return new ClauseSet(clonedClauses,
                           varClausesMap,
                           _variableNames,
                           false);
    }

    public ClauseSet Clone()
    {
      var (clauses, varClausesMap) = cloneInternal();
      return new ClauseSet(clauses,
                           varClausesMap,
                           _variableNames,
                           false);
    }

    public bool IsContradiction()
    {
      return Clauses.Any(clause => clause.IsEmptyClause());
    }

    public void DeleteTautologies()
    {
      Clauses.RemoveWhere(clausule => clausule.IsTautology());
    }


    private bool hasOnlyConsistentLiterals()
    {
      var dictionary = new Dictionary<string, Literal>();
      foreach (var clause in Clauses)
      {
        if (!clause.IsUnitClause())
        {
          return false;
        }

        var singleLiteral = clause.FirstLiteral;
        if (dictionary.TryGetValue(singleLiteral.Name, out var currentLiteral))
        {
          if (!currentLiteral.Equals(singleLiteral))
          {
            return false;
          }
        }
        else
        {
          dictionary.Add(singleLiteral.Name, singleLiteral);
        }
      }

      return true;
    }

    private (HashSet<Clause> newClauses, Dictionary<string, LiteralsToClausesMap> newVarClausesMap) cloneInternal()
    {
      var newClauses = new HashSet<Clause>();
      var newVarClausesMap = _clausesByLiterals.ToDictionary(pair => pair.Key,
                                                             pair =>
                                                               new LiteralsToClausesMap(pair.Key,
                                                                                        new List<Clause>(),
                                                                                        new List<Clause>()));
      foreach (var clause in Clauses)
      {
        var clauseClone = clause.Clone();
        foreach (var literal in clauseClone.Literals)
        {
          if (literal.IsTrue)
          {
            newVarClausesMap[literal.Name].ClausesWithPositiveLiterals.Add(clauseClone);
          }
          else
          {
            newVarClausesMap[literal.Name].ClausesWithNegativeLiterals.Add(clauseClone);
          }
        }

        newClauses.Add(clauseClone);
      }

      return (newClauses, newVarClausesMap);
    }

    [Flags]
    private enum PureLiteralResult
    {
      Unknown = 0,
      PureTrue = 1,
      PureFalse = 2,
      NoPure = 4
    }
  }
}