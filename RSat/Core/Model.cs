﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSat.Core
{
  public class Model
  {
    private readonly IEnumerable<ModelValue> _modelValues;

    public Model(long index,
                 IEnumerable<ModelValue> modelValues)
    {
      if (index < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(index));
      }

      Index = index;

      _modelValues = modelValues ?? throw new ArgumentNullException(nameof(modelValues));
    }

    public long Index
    {
      get;
    }

    public override string ToString()
    {
      return
        $"{nameof(Index)}: {Index} {_modelValues.Aggregate(new StringBuilder(), (sb, modelValue) => sb.Append(modelValue + "\n"))}";
    }

    public bool IsModelFor(IEnumerable<Literal[]> clausules)
    {
      foreach (var literals in clausules)
      {
        var isClausuleSatisfied = literals.Any(literal => _modelValues.Single(value => value.Name == literal.Name) == literal);

        if (!isClausuleSatisfied)
        {
          return false;
        }
      }

      return true;
    }
  }
}