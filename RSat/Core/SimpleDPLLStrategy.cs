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
      if (clausules == null)
      {
        throw new ArgumentNullException(nameof(clausules));
      }

      if (variablesMap == null)
      {
        throw new ArgumentNullException(nameof(variablesMap));
      }

      const int INITIAL_LEVEL = 0;

      preprocessClausules(clausules);
      var solverStack = ImmutableStack<SolverState>.Empty;
      var initialSolverState = new SolverState(clausules,
                                               variablesMap,
                                               INITIAL_LEVEL,
                                               true);

      solverStack = solverStack.Push(initialSolverState);
      while (!solverStack.IsEmpty)
      {

        solverStack = solverStack.Pop(out var currentState);
        Trace.WriteLine($"Iteration depth: {currentState.Depth}");

        var clausuleSet = currentState.Clausules;

        if (hasContradictions(clausuleSet))
        {
          Trace.WriteLine("Contradiction found. Backtracking...");
          break;
        }

        if (isConsistentSetOfLiterals(clausuleSet))
        {
          Trace.WriteLine("Found model...");
          return new Model(0, generateModelValues(clausuleSet, currentState.VariablesMap));
        }

        if (hasEmptyClausule(clausuleSet))
        {
          Trace.WriteLine("Empty clausule found. Backtracking...");
          continue;
        }


        var (afterPropagateClausule, afterPropagateVariableMap) = propagateUnitClausules(clausuleSet, currentState.VariablesMap);
        var (afterPureLiteralClausule, afterPureLiteralVariableMap) = handlePureLiterals(afterPropagateClausule, afterPropagateVariableMap);


        afterPureLiteralClausule = afterPureLiteralClausule.Clone();
        var chosenLiteral = chooseNewLiteral(afterPureLiteralClausule,
                                             afterPureLiteralVariableMap);



        var somethingChanged = chosenLiteral != null ||
                               !ReferenceEquals(afterPureLiteralVariableMap, currentState.VariablesMap);

        if (!somethingChanged && !currentState.SomethingChangedInPreviousIteration)
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
                                           false)
                               {
                                 SomethingChangedInPreviousIteration =  somethingChanged
                               });
        }
        else
        {
          Trace.WriteLine($"Chosen literal {chosenLiteral.Name}");

          var newClausulesNeg = afterPureLiteralClausule.CloneWithClausule(new Clausule(new List<Literal> { ~chosenLiteral }));
          var newClausulesPos = afterPureLiteralClausule.CloneWithClausule(new Clausule(new List<Literal> { chosenLiteral }));


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

    private static void preprocessClausules(ClausuleSet clausules)
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
      var newClausules = clausules;
      var newVariablesMap = variablesMap;
      foreach (var pureLiteral in pureLiteralsInClausules)
      {
        Trace.WriteLine($"Trying pure literal strategy: {pureLiteral}");
        newVariablesMap = newVariablesMap.SetItem(pureLiteral.Name, pureLiteral.IsTrue
          ? variablesMap[pureLiteral.Name].TryTrueValue()
          : variablesMap[pureLiteral.Name].TryFalseValue());

        newClausules = simplifyClausulesSatisfiedByLiteral(newClausules, pureLiteral);
      }

      return (newClausules, newVariablesMap);

    }

    private static Clausules simplifyClausulesSatisfiedByLiteral(Clausules clausules,
                                                                 Literal literal)
    {
      clausules.DeleteClausulesContainingLiteral(literal);
      Console.WriteLine($"Remaining ClausulesSet: {clausules.ClausulesCount}");
      return clausules.AddClausule(new Clausule(new List<Literal> { literal }));
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
        Trace.WriteLine($"Trying unit propagation of the clausules with literal: {singleLiteral}");

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
        SomethingChangedInPreviousIteration = true;
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

      public bool SomethingChangedInPreviousIteration 
      {
        get;
        set;
      }
    }
  }
}