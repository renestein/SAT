using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RSat.Core
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

    public bool IsModelFor(ImmutableList<ImmutableList<Literal>> clausules)
    {
      foreach (var literals in clausules)
      {
        var isClausuleSatisfied =
          literals.Any(literal => ModelValues.Single(val => val.Name == literal.Name) == literal);

        if (!isClausuleSatisfied)
        {
          return false;
        }
      }

      return true;
    }
  }
}