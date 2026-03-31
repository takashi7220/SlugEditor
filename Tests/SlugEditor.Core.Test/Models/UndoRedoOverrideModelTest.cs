using SlugEditor.Core.Models;
using SlugEditor.Core.Properties;
using SlugEditor.Test;
using CoreMatrix4x4 = SlugEditor.Core.Properties.Matrix4x4;
using CoreTrackedList = SlugEditor.Core.Properties.TrackedList<int>;
using CoreVector3 = SlugEditor.Core.Properties.Vector3;

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

    [Fact]
    public void StringOverride_TracksUndoRedo()
    {
        var model = new OverrideModel();

        model.Name.SetValue("Alice");
        Assert.Equal("Alice", model.Name.Value);
        Assert.True(model.Name.IsOverride());

        model.History.Undo();
        Assert.Equal("default-name", model.Name.Value);
        Assert.False(model.Name.IsOverride());

        model.History.Redo();
        Assert.Equal("Alice", model.Name.Value);
        Assert.True(model.Name.IsOverride());
    }

    [Fact]
    public void FloatOverride_TracksUndoRedo()
    {
        var model = new OverrideModel();

        model.Rate.SetValue(1.5f);
        Assert.Equal(1.5f, model.Rate.Value);
        Assert.True(model.Rate.IsOverride());

        model.History.Undo();
        Assert.Equal(0f, model.Rate.Value);
        Assert.False(model.Rate.IsOverride());
    }

    [Fact]
    public void VectorOverride_TracksUndoRedo()
    {
        var model = new OverrideModel();
        var vector = new CoreVector3(1, 2, 3);

        model.Position.SetValue(vector);
        Assert.Same(vector, model.Position.Value);
        Assert.True(model.Position.IsOverride());

        model.History.Undo();
        Assert.Equal(0f, model.Position.Value.X);
        Assert.Equal(0f, model.Position.Value.Y);
        Assert.Equal(0f, model.Position.Value.Z);
        Assert.False(model.Position.IsOverride());

        model.History.Redo();
        Assert.Same(vector, model.Position.Value);
        Assert.True(model.Position.IsOverride());
    }

    [Fact]
    public void MatrixOverride_TracksUndoRedo()
    {
        var model = new OverrideModel();
        var matrix = new CoreMatrix4x4 { M11 = 9, M22 = 8, M33 = 7, M44 = 1 };

        model.Transform.SetValue(matrix);
        Assert.Same(matrix, model.Transform.Value);
        Assert.True(model.Transform.IsOverride());

        model.History.Undo();
        Assert.False(model.Transform.IsOverride());

        model.History.Redo();
        Assert.Same(matrix, model.Transform.Value);
        Assert.True(model.Transform.IsOverride());
    }

    [Fact]
    public void TrackedListOverride_TracksUndoRedo()
    {
        var model = new OverrideModel();
        var list = new CoreTrackedList { 10, 20, 30 };

        model.Items.SetValue(list);
        Assert.Same(list, model.Items.Value);
        Assert.True(model.Items.IsOverride());

        model.History.Undo();
        Assert.False(model.Items.IsOverride());
        Assert.Empty(model.Items.Value);

        model.History.Redo();
        Assert.Same(list, model.Items.Value);
        Assert.True(model.Items.IsOverride());
        Assert.Equal([10, 20, 30], model.Items.Value);
    }

    [Fact]
    public void MixedOverrideOperations_CanUndoRedoInOrder()
    {
        var model = new OverrideModel();

        model.Speed.SetValue(50);
        model.Name.SetValue("Bob");
        model.Rate.SetValue(2.5f);

        Assert.Equal(3, model.History.UndoCount);
        Assert.True(model.Speed.IsOverride());
        Assert.True(model.Name.IsOverride());
        Assert.True(model.Rate.IsOverride());

        model.History.Undo(); // Rate
        Assert.Equal(0f, model.Rate.Value);
        Assert.False(model.Rate.IsOverride());

        model.History.Undo(); // Name
        Assert.Equal("default-name", model.Name.Value);
        Assert.False(model.Name.IsOverride());

        model.History.Undo(); // Speed
        Assert.Equal(10, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.History.Redo();
        model.History.Redo();
        model.History.Redo();
        Assert.Equal(50, model.Speed.Value);
        Assert.Equal("Bob", model.Name.Value);
        Assert.Equal(2.5f, model.Rate.Value);
    }

    [Fact]
    public void SetDefaultWithResetValue_TracksUndoRedo()
    {
        var model = new OverrideModel();

        model.Speed.SetDefault(15, resetValue: true);
        Assert.Equal(15, model.Speed.Default);
        Assert.Equal(15, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.Speed.SetValue(40);
        Assert.True(model.Speed.IsOverride());

        model.History.Undo();
        Assert.Equal(15, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.History.Undo();
        Assert.Equal(10, model.Speed.Value);
        Assert.Equal(15, model.Speed.Default);
        Assert.True(model.Speed.IsOverride());

        model.History.Undo();
        Assert.Equal(10, model.Speed.Default);
        Assert.Equal(10, model.Speed.Value);
        Assert.False(model.Speed.IsOverride());

        model.History.Redo();
        model.History.Redo();
        model.History.Redo();
        Assert.Equal(15, model.Speed.Default);
        Assert.Equal(40, model.Speed.Value);
        Assert.True(model.Speed.IsOverride());
    }

    private sealed class OverrideModel : UndoRedoModel
    {
        private readonly OverrideProperty<int> _speed = new(10);
        private readonly OverrideProperty<string> _name = new("default-name");
        private readonly OverrideProperty<float> _rate = new(0f);
        private readonly OverrideProperty<CoreVector3> _position = new(new CoreVector3(0, 0, 0));
        private readonly OverrideProperty<CoreMatrix4x4> _transform = new(new CoreMatrix4x4());
        private readonly OverrideProperty<CoreTrackedList> _items = new(new CoreTrackedList());

        public OverrideModel()
        {
            TrackNestedProperties(_speed, nameof(Speed));
            TrackNestedProperties(_name, nameof(Name));
            TrackNestedProperties(_rate, nameof(Rate));
            TrackNestedProperties(_position, nameof(Position));
            TrackNestedProperties(_transform, nameof(Transform));
            TrackNestedProperties(_items, nameof(Items));
        }

        public OverrideProperty<int> Speed => _speed;
        public OverrideProperty<string> Name => _name;
        public OverrideProperty<float> Rate => _rate;
        public OverrideProperty<CoreVector3> Position => _position;
        public OverrideProperty<CoreMatrix4x4> Transform => _transform;
        public OverrideProperty<CoreTrackedList> Items => _items;
    }
}
