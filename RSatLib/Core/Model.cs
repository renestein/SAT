﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RSatLib.Core
{
  public class Model
  {
    public Model(BigInteger index,
                 IEnumerable<ModelValue> modelValues)
    {
      if (index < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(index));
      }

      Index = index;

      ModelValues = modelValues ?? throw new ArgumentNullException(nameof(modelValues));
    }

    public IEnumerable<ModelValue> ModelValues
    {
      get;
    }

    public BigInteger Index
    {
      get;
    }

    public override string ToString()
    {
      return
        $"{nameof(Index)}: {Index} {ModelValues.Aggregate(new StringBuilder(), (sb, modelValue) => sb.Append(modelValue + "\n"))}";
    }

    public bool IsModelFor(ClauseSet clauseSet)
    {
      foreach (var literals in clauseSet.Clauses.Select(clause => clause.Literals))
      {
        var isClauseSatisfied =
          literals.Any(literal => ModelValues.Single(val => val.Name == literal.Name) == literal);

        if (!isClauseSatisfied)
        {
          return false;
        }
      }

      return true;
    }
  }
}