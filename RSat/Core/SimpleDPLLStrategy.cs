using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Clausules = RSat.Core.ClausuleSet;
  //System.Collections.Generic.List<System.Collections.Immutable.ImmutableList<RSat.Core.Literal>>;
  //using ImmutableClausules =
  //System.Collections.Immutable.ImmutableList<System.Collections.Immutable.ImmutableList<RSat.Core.Literal>>;
using VariablesMap = System.Collections.Immutable.ImmutableDictionary<string, RSat.Core.Variable>;
//Naive, inefficient (LINQ, Immutable collections), dirty.
namespace RSat.Core
{
  public static class SimpleDPLLStrategy
  {
    public static Model? Solve(Clausules clausules,
                               VariablesMap variablesMap)
    {
      const int INITIAL_LEVEL = 0;
      var solverStack = ImmutableStack<SolverState>.Empty;
      var initialSolverState = new SolverState(clausules,
                                               variablesMap,
                                               INITIAL_LEVEL,
                                               true);

      solverStack = solverStack.Push(initialSolverState);
      preprocesClausules(clausules);
      while (!solverStack.IsEmpty)
      {

        solverStack = solverStack.Pop(out var currentState);
        Trace.WriteLine($"Iteration depth: {currentState.Depth}");

        if (hasContradictions(currentState.Clausules))
        {
          Trace.WriteLine("Contradiction found. Backtracking...");
          break;
        }

        if (isConsistentSetOfLiterals(currentState.Clausules))
        {
          Trace.WriteLine("Found model...");
          return new Model(0, generateModelValues(currentState.Clausules, currentState.VariablesMap));
        }

        if (hasEmptyClausule(currentState.Clausules))
        {
          Trace.WriteLine("Empty clausule found. Backtracking...");
          continue;
        }


        var (afterPropagateClausule, afterPropagateVariableMap) = propagateUnitClausules(currentState.Clausules, currentState.VariablesMap);
        var (afterPureLiteralClausule, afterPureLiteralVariableMap) = handlePureLiterals(afterPropagateClausule, afterPropagateVariableMap);


        var chosenLiteral = chooseNewLiteral(afterPureLiteralClausule,
                                             afterPureLiteralVariableMap);



        var somethingChanged = chosenLiteral != null||
                               !ReferenceEquals(afterPureLiteralClausule, currentState.Clausules) ||
                               !ReferenceEquals(afterPureLiteralVariableMap, currentState.VariablesMap);

        if (!somethingChanged)
        {
          Trace.WriteLine("Nothing changed. Backtracking...");
          continue;
        }

        if (chosenLiteral == null)
        {
          Trace.WriteLine($"Out of literals");
          solverStack =
            solverStack.Push(new
                               SolverState(afterPureLiteralClausule.Clone(),
                                           afterPureLiteralVariableMap,
                                           currentState.Depth + 1,
                                           false));
        }
        else
        {
          Trace.WriteLine($"Chosen literal {chosenLiteral.Name}");

          var newClausulesNeg = afterPureLiteralClausule.CloneWithClausule(new Clausule(new List<Literal>{~chosenLiteral}));
          var newClausulesPos = afterPureLiteralClausule.CloneWithClausule(new Clausule(new List<Literal>{chosenLiteral}));


          solverStack =
            solverStack.Push(new
                               SolverState(newClausulesNeg,
                                           afterPureLiteralVariableMap,
                                           currentState.Depth + 1,
                                          true));
          solverStack = solverStack.Push(new
                                           SolverState(newClausulesPos,
                                                       afterPureLiteralVariableMap,
                                                       currentState.Depth + 1,
                                                      true));
        }
      }

      return null;
    }

    private static void preprocesClausules(ClausuleSet clausules)
    {
      clausules.DeleteTautologies();
    }

    private static bool hasContradictions(ClausuleSet clausules)
    {
      return clausules.IsContradiction();
    }

    private static IEnumerable<ModelValue> generateModelValues(Clausules clausulesSet,
                                                               VariablesMap variablesMap)
    {
      var modelValues = clausulesSet.Clausules.Select(clausule =>
                      {
                        var literal = clausule.FirstLiteral;
                        return new ModelValue(literal.Name, literal.IsTrue);
                      }).Distinct().ToArray();

      var retModelValues = modelValues.Concat(variablesMap
                                              .Keys.Where(varName => !modelValues.Any(modelValue =>
                                                                                        modelValue
                                                                                          .Name.Equals(varName)))
                                              .Select(varName => new ModelValue(varName, true))).ToArray();
      return retModelValues;
    }

    private static Literal? chooseNewLiteral(Clausules clausules,
                                           ImmutableDictionary<string, Variable> variablesMap)
    {
      var selectedLiteral = clausules.SelectUnusedLiteral(variablesMap);

      return selectedLiteral;
    }

    private static (Clausules, VariablesMap) handlePureLiterals(Clausules clausules,
                                                ImmutableDictionary<string, Variable> variablesMap)
    {
      var pureLiteralsInClausules = clausules.GetPureLiterals();

      var newVariablesMap = variablesMap;
      var newClausules = clausules;
      foreach (var pureLiteral in pureLiteralsInClausules)
      {
        Trace.WriteLine($"Trying pure literal strategy: {pureLiteral}");
        newVariablesMap = newVariablesMap.SetItem(pureLiteral.Name, pureLiteral.IsTrue
          ? variablesMap[pureLiteral.Name].TryTrueValue()
          : variablesMap[pureLiteral.Name].TryFalseValue());

        newClausules.DeleteComplexSatisfiedClausulesContainingLiteral(pureLiteral);
        newClausules = newClausules.CloneWithClausule(new Clausule(new List<Literal> {pureLiteral}));
        Console.WriteLine($"Remaining clausulesSet: {newClausules.ClausulesCount}");
      }

      return (newClausules, newVariablesMap);

    }

    private static bool hasEmptyClausule(Clausules clausules)
    {
      return clausules.HasEmptyClausule();
    }

    private static (Clausules, VariablesMap) propagateUnitClausules(Clausules clausules,
                                                    ImmutableDictionary<string, Variable> variablesMap)
    {
      var newClausules = clausules;
      var newVariableMap = variablesMap;
      while (true)
      {
        var toPropagateUnitClausule = newClausules.SelectUnitClausule(newVariableMap);
        if (toPropagateUnitClausule == null)
        {
          break;
        }

        var singleLiteral = toPropagateUnitClausule.FirstLiteral;
        Trace.WriteLine($"Trying unit propagation of the clausule with literal: {singleLiteral}");

        newVariableMap = newVariableMap.SetItem(singleLiteral.Name, singleLiteral.IsTrue
                                                  ? variablesMap[singleLiteral.Name].TryTrueValue()
                                                  : variablesMap[singleLiteral.Name].TryFalseValue());

        newClausules.DeleteLiteralFromClausules(~singleLiteral);

      }

      return (newClausules, newVariableMap);
    }


    private static bool isConsistentSetOfLiterals(Clausules clausules)
    {
      return clausules.IsConsistentSetOfLiterals();
    }


    private struct SolverState
    {
      public SolverState(Clausules clausules,
                         ImmutableDictionary<string, Variable> variablesMap,
                         int depth,
                         bool literalAdded)
      {
        Clausules = clausules;
        VariablesMap = variablesMap;
        Depth = depth;
        LiteralAdded = literalAdded;
      }

      public Clausules Clausules
      {
        get;
      }

      public VariablesMap VariablesMap
      {
        get;
      }

      public int Depth
      {
        get;
      }

      public bool LiteralAdded
      {
        get;
      }
    }
  }
}