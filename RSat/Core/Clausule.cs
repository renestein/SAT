using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RSat.Core
{
  public class Clausule
  {
    public Clausule(List<Literal> literals)
    {
      Literals = literals ?? throw new ArgumentNullException(nameof(literals));
      Literals.Sort();
    }

    public bool IsUnitClausule()
    {
      return Literals.Count == 1;
    }

    public bool IsEmptyClausule()
    {
      return Literals.Count == 0;
    }

    public Literal FirstLiteral
    {
      get
      {
        if (IsEmptyClausule())
        {
          throw new InvalidOperationException("Empty clausule does not have literals!");
        }

        return Literals[0];
      }
     
    }

    public List<Literal> Literals
    {
      get;
      private set;
    }

		public void DeleteLiteral(Literal literal)
    {
      int index;
      while ((index = getLiteralIndex(literal)) >= 0)
      {
        Literals.RemoveAt(index);
      }
      //Literals.RemoveAll(ourLiteral => ourLiteral.Equals(literal));
    }

    public Literal? SelectUnusedLiteral(IDictionary<string, Variable> variablesMap)
    {
      for (int i = 0; i < Literals.Count; i++)
      {
        var literal = Literals[i];
        if (!variablesMap[literal.Name].AnyValueUsed())
        {
          return literal;
        }
      }

      return null;
    }

    public Clausule Clone()
    {
      return new Clausule(Literals.ToList());
    }

    public bool HasLiteral(Literal pureLiteral)
    {
      return getLiteralIndex(pureLiteral) >= 0;
    }

    private int getLiteralIndex(Literal literal)
    {
      return Literals.BinarySearch(literal);
    }
  }
}