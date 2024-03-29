﻿using System;

namespace RSatLib.Core
{
  public class ModelValue
  {
    public ModelValue(string name, bool isTrue)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      IsTrue = isTrue;
      IsFalse = !isTrue;
    }
    public string Name 
    {
      get;
    }

    public bool IsTrue
    {
      get;
    }

    public bool IsFalse
    {
      get;
    }

    public static implicit operator bool(ModelValue modelValue) => modelValue.IsTrue;

    public override string ToString()
    {
      return $"{nameof(Name)}: {Name}, {nameof(IsTrue)}: {IsTrue}";
    }
  }
}