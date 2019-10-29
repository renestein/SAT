using System;
using System.Collections.Generic;
using System.Linq;


namespace RSat.Core
{
  public partial class ClauseSet
  {
    private readonly IEnumerable<string> _variableNames;
    private readonly Dictionary<string, ClausesWithVariable> _clausesByLiterals;

    public ClauseSet(List<Clause> clauses,
                     IEnumerable<string> variableNames)
    {
      _variableNames = variableNames;
      Clauses = clauses ?? throw new ArgumentNullException(nameof(clauses));
      _clausesByLiterals = prepareClauses(Clauses, _variableNames);
    }

    private ClauseSet(List<Clause> clonedClauses,
                      Dictionary<string, ClausesWithVariable> varClausesMap,
                      IEnumerable<string> variableNames)
    {
      Clauses = clonedClauses ?? throw new ArgumentNullException(nameof(clonedClauses));
      _clausesByLiterals = varClausesMap ?? throw new ArgumentNullException(nameof(varClausesMap));
      _variableNames = variableNames ?? throw new ArgumentNullException(nameof(variableNames));
    }


    public bool HasClauses => Clauses.Count != 0;
    public int ClausesCount => Clauses.Count;

    public bool IsEmpty => !HasClauses;

    public List<Clause> Clauses
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
      var varName = _clausesByLiterals.Keys.FirstOrDefault(variableName => !variablesMap.HasValueFor(variableName));
      return varName == null
             ? null
             : (Literal)variablesMap[varName];
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
      for (var i = 0; i < Clauses.Count; i++)
      {
        if (Clauses[i].IsEmptyClause())
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
                           _variableNames);
    }

    public ClauseSet Clone()
    {
      var (clauses, varClausesMap) = cloneInternal();
      return new ClauseSet(clauses,
                           varClausesMap,
                           _variableNames);
    }

    public bool IsContradiction()
    {
      return Clauses.Any(clause => clause.IsEmptyClause());
    }

    public void DeleteTautologies()
    {
      Clauses.RemoveAll(clausule => clausule.IsTautology());
    }

    private Dictionary<string, ClausesWithVariable> prepareClauses(List<Clause> clauses,
                                IEnumerable<string> variables)
    {
      return variables.Select(varName =>
      {
        var positiveLiteral = new Literal(varName, true);
        var negativeLiteral = new Literal(varName, false);

        var positiveClauses = clauses.Where(clause => clause.HasLiteral(positiveLiteral)).ToList();
        var negativeClauses = clauses.Where(clause => clause.HasLiteral(negativeLiteral)).ToList();
        return new ClausesWithVariable(varName, positiveClauses, negativeClauses);
      }).ToDictionary(arg => arg.VariableName);

    }


    private bool hasOnlyConsistentLiterals()
    {
      var dictionary = new Dictionary<string, Literal>();
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clause = Clauses[i];
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

    private (List<Clause> Clauses, Dictionary<string, ClausesWithVariable> VarClausesMap) cloneInternal()
    {
      var newClauses = new List<Clause>();
      var newVarClausesMap = _clausesByLiterals.ToDictionary(pair => pair.Key,
                                                             pair =>
                                                               new ClausesWithVariable(pair.Key, new List<Clause>(),
                                                                                       new List<Clause>()));
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clauseClone = Clauses[i].Clone();
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

    private class ClausesWithVariable
    {
      public ClausesWithVariable(string variableName,
                                 List<Clause> clausesWithPositiveLiterals,
                                 List<Clause> clausesWithNegativeLiterals)
      {
        VariableName = variableName;
        ClausesWithPositiveLiterals = clausesWithPositiveLiterals;
        ClausesWithNegativeLiterals = clausesWithNegativeLiterals;
      }

      public string VariableName
      {
        get;
      }

      public List<Clause> ClausesWithPositiveLiterals
      {
        get;
      }

      public List<Clause> ClausesWithNegativeLiterals
      {
        get;
      }

      public Literal? TryGetPureLiteral(Variables variables)
      {

        if (ClausesWithPositiveLiterals.Count == 0 && ClausesWithNegativeLiterals.Count > 1)
        {
          return ClausesWithNegativeLiterals.Count == 1 && ClausesWithNegativeLiterals[0].IsUnitClause()
            ? null
            : ~variables[VariableName];
        }

        if (ClausesWithNegativeLiterals.Count == 0 && ClausesWithPositiveLiterals.Count > 1)
        {
          return ClausesWithPositiveLiterals.Count == 1 && ClausesWithPositiveLiterals[0].IsUnitClause()
            ? null
            : (Literal)variables[VariableName];
        }

        return null;
      }
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