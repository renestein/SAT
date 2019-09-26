using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace RSat.Core
{
  public class Sat
  {
    private readonly List<Literal[]> _clausules;
    private readonly IDictionary<string, Variable> _variablesMap;

    public Sat()
    {
      _variablesMap = new Dictionary<string, Variable>();
      _clausules = new List<Literal[]>();
    }

    public IEnumerable<Model> FoundModels
    {
      get;
      private set;
    } = Enumerable.Empty<Model>();

    public void CreateVariable(string name)
    {
      var variable = new Variable(name);
      _variablesMap.Add(name, variable);
    }

    public Variable GetVariable(string name)
    {
      return _variablesMap[name];
    }

    public void AddClausule(params Literal[] literals)
    {
      if (literals == null)
      {
        throw new ArgumentNullException(nameof(literals));
      }

      _clausules.Add(literals);
    }

    public bool Solve()
    {
      var models = generateModels();

      //FoundModels = models.Where(model => model.IsModelFor(_clausules)).ToArray();
      var foundModel = models.FirstOrDefault(model => model.IsModelFor(_clausules));
      FoundModels = foundModel == null
        ? Enumerable.Empty<Model>()
        : new[] { foundModel };

      return FoundModels.Any();
    }


    private IEnumerable<Model> generateModels()
    {
      const int VALUATIONS = 2;
      BigInteger ONE = 1;
      var variablesMapCount = _variablesMap.Count;
      var singleLiterals = _clausules.Where(literals => literals.Length == 1)
                                     .Select(literals => literals[0])
                                     .ToDictionary(literal => literal.Name);
      var numberOfModels = (BigInteger)Math.Pow(VALUATIONS, variablesMapCount);
      for (BigInteger modelIndex = 0; modelIndex < numberOfModels; modelIndex++)
      {
        Console.WriteLine(modelIndex);
        var modelValues = new ModelValue[variablesMapCount];
        var varIndex = 0;
        var haveModel = true;
        foreach (var varName in _variablesMap.Keys)
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