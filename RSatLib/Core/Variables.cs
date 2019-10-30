using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RSatLib.Core
{
  public class Variables
  {
    private ImmutableDictionary<string, Variable> _variableMap;

    public Variables()
    {
      _variableMap = ImmutableDictionary.Create<string, Variable>();
    }

    private Variables(ImmutableDictionary<string, Variable> variableMap)
    {
      _variableMap = variableMap;
    }

    public Variable this[string name] => _variableMap[name];

    public int VariablesCount => _variableMap.Count;
   
    public Variable SetToFalse(string variableName)
    {
      var variable = _variableMap[variableName];
      var falseVariable = variable.TryFalseValue();
      _variableMap = _variableMap.SetItem(variableName, variable.TryFalseValue());
      return falseVariable;
    }

    public Variable SetToTrue(string variableName)
    {
      var variable = _variableMap[variableName];
      var trueVariable = variable.TryTrueValue();
      _variableMap = _variableMap.SetItem(variableName, variable.TryFalseValue());
      return trueVariable;
    }

    public Variable SetToValue(string variableName,
                                bool variableValue) => variableValue
                                      ? SetToTrue(variableName)
                                      : SetToFalse(variableName);

    public Variable Add(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
      }
      var variable = new Variable(name);
      _variableMap = _variableMap.Add(name, variable);
      return variable;
    }

    public IEnumerable<string> VariableNames()
    {
      return _variableMap.Keys;
    }

    public Variables Clone()
    {
      return new Variables(_variableMap);
    }

    public bool HasValueFor(string variableName)
    {
      return _variableMap[variableName].HasValue;
    }
  }
}