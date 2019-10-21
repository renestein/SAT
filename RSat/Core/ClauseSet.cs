using System;
using System.Collections.Generic;
using System.Linq;

namespace RSat.Core
{
  public partial class ClauseSet
  {
    public ClauseSet(List<Clause> clauses)
    {
      Clauses = clauses ?? throw new ArgumentNullException(nameof(clauses));
      
    }

    public bool HasClauses => Clauses.Count != 0;
    public int ClausesCount => Clauses.Count;

    public bool IsEmpty => !HasClauses;

    public List<Clause> Clauses
    {
      get;
    }

    public Clause SelectUnitClause(Variables variablesMap)
    {
      return Clauses.FirstOrDefault(clause => clause.IsUnitClause()
                                              && !variablesMap[clause.FirstLiteral.Name].HasValue);
    }

    public Literal? SelectUnusedLiteral(Variables variablesMap)
    {
      for (var i = 0; i < Clauses.Count; i++)
      {
        var selectedLiteral = Clauses[i].SelectUnusedLiteral(variablesMap);
        if (selectedLiteral != null)
        {
          return selectedLiteral;
        }
      }

      return null;
    }

    public void AddClause(Clause clause)
    {
      if (clause == null)
      {
        throw new ArgumentNullException(nameof(clause));
      }

      Clauses.Add(clause);
    }

    public ClauseOperationResult DeleteLiteralFromClauses(Literal literal)
    {
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clause = Clauses[i];
        clause.DeleteLiteral(literal);
        if (clause.IsEmptyClause())
        {
          return ClauseOperationResult.MinOneEmptyClausuleFound;
        }
      }

      return ClauseOperationResult.OperationSuccess;
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
      var pureCandidates = new Dictionary<string, PureLiteralResult>();
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clause = Clauses[i];
        if (clause.IsEmptyClause())
        {
          continue;
        }

        for (var j = 0; j < clause.Literals.Count; j++)
        {
          var currentLiteral = clause.Literals[j];
          if (pureCandidates.TryGetValue(currentLiteral.Name, out var candidateValue))
          {
            if (candidateValue == PureLiteralResult.NoPure)
            {
              continue;
            }

            if (currentLiteral.IsTrue && candidateValue == PureLiteralResult.PureFalse ||
                currentLiteral.IsFalse && candidateValue == PureLiteralResult.PureTrue)
            {
              pureCandidates[currentLiteral.Name] = PureLiteralResult.NoPure;
            }
          }
          else
          {
            if (clause.IsUnitClause())
            {
              continue;
            }

            pureCandidates[currentLiteral.Name] = currentLiteral.IsTrue
              ? PureLiteralResult.PureTrue
              : PureLiteralResult.PureFalse;
          }
        }
      }

      var pureLiterals = new List<Literal>();
      foreach (var pureLiteralResult in pureCandidates)
      {
        if (pureLiteralResult.Value == PureLiteralResult.PureFalse)
        {
          pureLiterals.Add(new Literal(pureLiteralResult.Key, false));
        }
        else if (pureLiteralResult.Value == PureLiteralResult.PureTrue)
        {
          pureLiterals.Add(new Literal(pureLiteralResult.Key, true));
        }
      }

      return pureLiterals;
    }

    public void DeleteClausesWithLiteral(Literal pureLiteral)
    {
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clause = Clauses[i];

        if (clause.HasLiteral(pureLiteral))
        {
          Clauses.Remove(clause);
        }
      }
    }

    public ClauseSet CloneWithClause(Clause clause)
    {
      var clonedClauses = cloneInternal();

      if (clause != null)
      {
        clonedClauses.Add(clause);
      }

      return new ClauseSet(clonedClauses);
    }

    public ClauseSet Clone()
    {
      var clonedClausesBuilder = cloneInternal();
      return new ClauseSet(clonedClausesBuilder);
    }

    public bool IsContradiction()
    {
      var seenLiterals = new Dictionary<string, bool>();
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clause = Clauses[i];
        if (!clause.IsUnitClause())
        {
          continue;
        }

        var singleLiteral = clause.FirstLiteral;
        if (seenLiterals.TryGetValue(singleLiteral.Name, out var currentLiteralValue))
        {
          if (currentLiteralValue != singleLiteral.IsTrue)
          {
            return true;
          }
        }
        else
        {
          seenLiterals[singleLiteral.Name] = singleLiteral.IsTrue;
        }
      }

      return false;
    }

    public void DeleteTautologies()
    {
      Clauses.RemoveAll(clausule => clausule.IsTautology());
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

    private List<Clause> cloneInternal()
    {
      var newClauses = new List<Clause>();
      for (var i = 0; i < Clauses.Count; i++)
      {
        var clauseClone = Clauses[i].Clone();
        newClauses.Add(clauseClone);
      }

      return newClauses;
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