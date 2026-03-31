using SlugEditor.Core.Models;
using SlugEditor.Core.Properties;
using SlugEditor.Test;

namespace SlugEditor.Core.Test.Models;

public sealed class UndoRedoOverrideModelTest
{
    [Fact]
    public void OverrideProperty_BasicBehavior_IsOverrideAndReset()
    {
        var p = new OverrideProperty<int>(10);

        Assert.False(p.IsOverride());

        p.Value = 20;
        Assert.True(p.IsOverride());

        p.Reset();
        Assert.Equal(10, p.Value);
        Assert.False(p.IsOverride());
    }

    [Fact]
    public void OverrideProperty_HelperMethods_WorkAsExpected()
    {
        var p = new OverrideProperty<int>(value: 10, @default: 10);

        Assert.True(p.IsDefault());
        Assert.False(p.SetValue(10));
        Assert.False(p.SetDefault(10));

        Assert.True(p.SetValue(20));
        Assert.True(p.IsOverride());

        Assert.True(p.ResetIfOverridden());
        Assert.Equal(10, p.Value);
        Assert.True(p.IsDefault());

        Assert.True(p.SetBoth(30, 15));
        Assert.Equal(30, p.Value);
        Assert.Equal(15, p.Default);
        Assert.True(p.IsOverride());
    }

    [Fact]
    public void SetValue_TracksUndoRedo_AndOverrideFlag()
    {
        var model = new OverrideModel();

        model.Speed.SetValue(20);
        Assert.Equal(20, model.Speed.Value);
        Assert.True(model.Speed.IsOverride());
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(10, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.History.Redo();
        Assert.Equal(20, model.Speed.Value);
        Assert.True(model.Speed.IsOverride());
    }

    [Fact]
    public void Reset_TracksUndoRedo_AndCanRestoreOverride()
    {
        var model = new OverrideModel();
        model.Speed.SetValue(30);
        Assert.True(model.Speed.IsOverride());

        model.Speed.Reset();
        Assert.Equal(10, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.History.Undo();
        Assert.Equal(30, model.Speed.Value);
        Assert.True(model.Speed.IsOverride());

        model.History.Redo();
        Assert.Equal(10, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());
    }

    private sealed class OverrideModel : UndoRedoModel
    {
        private readonly OverrideProperty<int> _speed = new(10);

        public OverrideModel()
        {
            TrackNestedProperties(_speed, nameof(Speed));
        }

        public OverrideProperty<int> Speed => _speed;
    }
}
