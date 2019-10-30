using System;
using System.Collections.Generic;
using System.Linq;

namespace RSat.Core
{
  public class Clause
  {

    public static Clause EmptyClause = new Clause(new List<Literal>());
    
    public Clause(List<Literal> literals)
    {
      Literals = literals ?? throw new ArgumentNullException(nameof(literals));
      Literals.Sort();
    }

    public Literal FirstLiteral
    {
      get
      {
        if (IsEmptyClause())
        {
          throw new InvalidOperationException("Empty clause does not have literals!");
        }

        return Literals[0];
      }
    }

    public List<Literal> Literals
    {
      get;
    }

    public static IComparer<Clause> NumberOfLiteralsComparerComparer
    {
      get;
    } = new NumberOfLiteralsComparer();


    public bool IsUnitClause()
    {
      return Literals.Count == 1;
    }

    public bool IsEmptyClause()
    {
      return Literals.Count == 0;
    }

    public void DeleteLiteral(Literal literal)
    {
      int index;
      while ((index = getLiteralIndex(literal)) >= 0)
      {
        Literals.RemoveAt(index);
      }
    }

    public Literal? SelectUnusedLiteral(Variables variablesMap)
    {
      for (var i = 0; i < Literals.Count; i++)
      {
        var literal = Literals[i];
        if (!variablesMap[literal.Name].HasValue)
        {
          return literal;
        }
      }

      return null;
    }

    public bool IsSameAs(Clause clause)
    {
      return clause.Literals.Count == Literals.Count && hasSameLiterals(clause, this);

      static bool hasSameLiterals(Clause first,
                                  Clause second)
      {
        for (var i = 0; i < first.Literals.Count; i++)
        {
          if (!first.Literals[i].Equals(second.Literals[i]))
          {
            return false;
          }
        }

        return true;
      }
    }


    public Clause Clone()
    {
      return new Clause(Literals.ToList());
    }

    public bool HasLiteral(Literal literal)
    {
      return getLiteralIndex(literal) >= 0;
    }

    //Assume sorted literals
    public bool IsTautology()
    {
      for (var i = 1; i < Literals.Count; i++)
      {
        if (Literals[i - 1].IsNegationOf(Literals[i]))
        {
          return true;
        }
      }

      return false;
    }

    private int getLiteralIndex(Literal literal)
    {
      return Literals.BinarySearch(literal);
    }

    private sealed class NumberOfLiteralsComparer : IComparer<Clause>
    {
      public int Compare(Clause x,
                         Clause y)
      {
        if (x.Literals.Count == y.Literals.Count)
        {
          return 1;
        }

        return x.Literals.Count > y.Literals.Count
          ? 1
          : -1;
      }
    }
  }
}