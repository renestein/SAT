using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RSat.Core
{
  public class ClausuleSet
  {
    public ClausuleSet(List<Clausule> clausules)
    {
      Clausules = clausules ?? throw new ArgumentNullException(nameof(clausules));
    }

    public bool HasClausules => Clausules.Count != 0;
    public int ClausulesCount => Clausules.Count;

    public bool IsEmpty => !HasClausules;

    public List<Clausule> Clausules
    {
      get;
      private set;
    }

    public Clausule SelectUnitClausule(IDictionary<string, Variable> variablesMap)
    {
      return Clausules.FirstOrDefault(clausule => clausule.IsUnitClausule()
                                                   && !variablesMap[clausule.FirstLiteral.Name].AnyValueUsed());
    }

    public Literal? SelectUnusedLiteral(IDictionary<string, Variable> variablesMap)
    {
      for (var i = 0; i < Clausules.Count; i++)
      {
        var selectedLiteral = Clausules[i].SelectUnusedLiteral(variablesMap);
        if (selectedLiteral != null)
        {
          return selectedLiteral;
        }
      }

      return null;
    }

    public void DeleteLiteralFromClausules(Literal literal)
    {
      for (var i = 0; i < Clausules.Count; i++)
      {
        Clausules[i].DeleteLiteral(literal);
      }
    }

    public bool IsConsistentSetOfLiterals()
    {
      return IsEmpty || hasOnlyConsistentLiterals();
    }

    public bool HasEmptyClausule()
    {
      for (var i = 0; i < Clausules.Count; i++)
      {
        if (Clausules[i].IsEmptyClausule())
        {
          return true;
        }
      }

      return false;
    }

    public IEnumerable<Literal> GetPureLiterals()
    {
      var pureCandidates = new Dictionary<string, PureLiteralResult>();
      for (int i = 0; i < Clausules.Count; i++)
      {
        var clausule = Clausules[i];
        if (clausule.IsEmptyClausule())
        {
          continue;
        }

        for (int j = 0; j < clausule.Literals.Count; j++)
        {
          var currentLiteral = clausule.Literals[j];
          if (pureCandidates.TryGetValue(currentLiteral.Name, out var candidateValue))
          {
            if (candidateValue == PureLiteralResult.NoPure)
            {
              continue;
            }

            if ((currentLiteral.IsTrue && candidateValue == PureLiteralResult.PureFalse) ||
                (currentLiteral.IsFalse && candidateValue == PureLiteralResult.PureTrue))
            {
              pureCandidates[currentLiteral.Name] = PureLiteralResult.NoPure;
            }
          }
          else
          {
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

    public void DeleteComplexSatisfiedClausulesContainingLiteral(Literal pureLiteral)
    {
      for (int i = 0; i < Clausules.Count; i++)
      {
        var clausule = Clausules[i];
        
        if (clausule.HasLiteral(pureLiteral))
        {
          Clausules.Remove(clausule);
        }
      }

    }

    public ClausuleSet CloneWithClausule(Clausule clausule)
    {
      var clonedCalusules = cloneInternal();

      if (clausule != null)
      {
        clonedCalusules.Add(clausule);
      }

      return new ClausuleSet(clonedCalusules);
    }

    public ClausuleSet Clone()
    {
      var clonedClausulesBuilder = cloneInternal();
      return new ClausuleSet(clonedClausulesBuilder);
    }

    public bool IsContradiction()
    {
      var seenLiterals = new Dictionary<string, bool>();
      for (int i = 0; i < Clausules.Count; i++)
      {
        var clausule = Clausules[i];
        if (!clausule.IsUnitClausule())
        {
          continue;
        }

        var singleLiteral = clausule.FirstLiteral;
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
      Clausules.RemoveAll(clausule => clausule.IsTautology());
    }

    private bool hasOnlyConsistentLiterals()
    {
      var dictionary = new Dictionary<string, Literal>();
      for (var i = 0; i < Clausules.Count; i++)
      {
        var clausule = Clausules[i];
        if (!clausule.IsUnitClausule())
        {
          return false;
        }

        var singleLiteral = clausule.FirstLiteral;
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

    private List<Clausule> cloneInternal()
    {
      var newClausules = new List<Clausule>();
      for (int i = 0; i < Clausules.Count; i++)
      {
        var clausuleClone = Clausules[i].Clone();
        newClausules.Add(clausuleClone);

      }

      return newClausules;
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