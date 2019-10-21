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
    public static Model? Solve(ClauseSet clauseSet, Variables variablesMap)
    {
      var models = generateModels(variablesMap, clauseSet);
#if DUMP_MODELS
      foreach (var model in models)
      {
        if (model.IsModelFor(clauseSet))
        {
          Console.WriteLine("Found model (dump)...");
          Console.WriteLine(model);
        }
      }
#endif

      return models.FirstOrDefault(model => model.IsModelFor(clauseSet));
    }

    private static IEnumerable<Model> generateModels(Variables variablesMap,
                                                     ClauseSet clauses)
    {
      const int VALUATIONS = 2;
      BigInteger ONE = 1;
      var variablesMapCount = variablesMap.VariablesCount;
      var singleLiterals = clauses.Clauses.Where(clause => clause.Literals.Count == 1)
                                     .Select(clause=> clause.FirstLiteral)
                                     .ToDictionary(literal => literal.Name);
      var numberOfModels = (BigInteger)Math.Pow(VALUATIONS, variablesMapCount);
      for (BigInteger modelIndex = 0; modelIndex < numberOfModels; modelIndex++)
      {
        Console.WriteLine(modelIndex);
        var modelValues = new ModelValue[variablesMapCount];
        var varIndex = 0;
        var haveModel = true;
        foreach (var varName in variablesMap.VariableNames())
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