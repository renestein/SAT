#define DUMP_MODELS
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace RSat.Core
{
  public static class NaiveSolverStrategy
  {
    public static Model? Solve(ImmutableList<ImmutableList<Literal>> clausules, ImmutableDictionary<string, Variable> variablesMap)
    {
      var models = generateModels(variablesMap, clausules);
#if DUMP_MODELS
      foreach (var model in models)
      {
        if (model.IsModelFor(clausules))
        {
          Console.WriteLine("Found model (dump)...");
          Console.WriteLine(model);
        }
      }
#endif

      return models.FirstOrDefault(model => model.IsModelFor(clausules));
    }

    private static IEnumerable<Model> generateModels(IDictionary<string, Variable> variablesMap,
                                                     ImmutableList<ImmutableList<Literal>> clausules)
    {
      const int VALUATIONS = 2;
      BigInteger ONE = 1;
      var variablesMapCount = variablesMap.Count;
      var singleLiterals = clausules.Where(literals => literals.Count == 1)
                                     .Select(literals => literals[0])
                                     .ToDictionary(literal => literal.Name);
      var numberOfModels = (BigInteger)Math.Pow(VALUATIONS, variablesMapCount);
      for (BigInteger modelIndex = 0; modelIndex < numberOfModels; modelIndex++)
      {
        Console.WriteLine(modelIndex);
        var modelValues = new ModelValue[variablesMapCount];
        var varIndex = 0;
        var haveModel = true;
        foreach (var varName in variablesMap.Keys)
        {

          var isVarTrue = (modelIndex & (ONE << varIndex)) != 0;
          var modelValue = new ModelValue(varName, isVarTrue);
          modelValues[varIndex] = modelValue;
          if (singleLiterals.TryGetValue(modelValue.Name, out var literal))
          {
            if (literal.IsTrue != modelValue.IsTrue)
            {
              Console.WriteLine($"Declined: {literal.Name}");
              haveModel = false;
              break;
            }
          }
          varIndex++;
        }

        if (!haveModel)
        {
          continue;

        }

        var model = new Model(modelIndex, modelValues);
        Console.WriteLine("Evaluating...");

        yield return model;
      }
    }
  }
}