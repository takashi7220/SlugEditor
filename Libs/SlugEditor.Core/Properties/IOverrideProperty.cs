using System;
using System.Collections.Generic;
using System.Text;

namespace SlugEditor.Core.Properties;

public interface IOverrideProperty
{
    public void Reset();

    public bool IsOverride { get => false; }
}
