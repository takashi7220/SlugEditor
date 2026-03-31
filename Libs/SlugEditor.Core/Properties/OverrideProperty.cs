using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SlugEditor.Core.Properties;

public class OverrideProperty<T> : IOverrideProperty where T : notnull
{
    public OverrideProperty(T value)
    {
        Value = value;
        Default = value;
    }

    public void Reset()
    {
        Value = Default;
    }

    public bool IsOverride()
    {
        return Value.Equals(Default);
    }

    public T Value { get; set; }
    public T Default { get; set; }
}
