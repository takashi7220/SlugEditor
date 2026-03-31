using CoreMatrix3x3 = SlugEditor.Core.Properties.Matrix3x3;
using CoreMatrix4x4 = SlugEditor.Core.Properties.Matrix4x4;
using SlugEditor.Core.Models;
using CoreTrackedList = SlugEditor.Core.Properties.TrackedList<int>;
using CoreVector2 = SlugEditor.Core.Properties.Vector2;
using CoreVector3 = SlugEditor.Core.Properties.Vector3;
using CoreVector4 = SlugEditor.Core.Properties.Vector4;
using SlugEditor.Test;
using Xunit;

namespace SlugEditor.Core.Test.Models;

public sealed class UndoRedoModelTests
{
    [Fact]
    public void SetTrackedProperty_RecordsHistory_AndCanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        Assert.Equal(0, model.Value);
        Assert.False(model.History.CanUndo);
        Assert.False(model.History.CanRedo);

        model.Value = 10;

        Assert.Equal(10, model.Value);
        Assert.True(model.History.CanUndo);
        Assert.Equal(1, model.History.UndoCount);
        Assert.Equal(0, model.History.RedoCount);

        model.History.Undo();
        Assert.Equal(0, model.Value);
        Assert.False(model.History.CanUndo);
        Assert.True(model.History.CanRedo);

        model.History.Redo();
        Assert.Equal(10, model.Value);
        Assert.True(model.History.CanUndo);
        Assert.False(model.History.CanRedo);
    }

    [Fact]
    public void SetTrackedProperty_SameValue_DoesNotRecordHistory()
    {
        var model = new SampleUndoRedoModel();

        model.Value = 5;
        Assert.Equal(1, model.History.UndoCount);

        model.Value = 5;
        Assert.Equal(1, model.History.UndoCount);
        Assert.Equal(0, model.History.RedoCount);
    }

    [Fact]
    public void UndoRedo_EmitsPropertyChanged_ForValue()
    {
        var model = new SampleUndoRedoModel();
        using var tracker = new PropertyChangedTracker(model);

        model.Value = 1;
        model.History.Undo();
        model.History.Redo();

        Assert.Equal(3, tracker.Count(nameof(SampleUndoRedoModel.Value)));
    }

    [Fact]
    public void SetTrackedProperty_String_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.Text = "alpha";
        Assert.Equal("alpha", model.Text);

        model.History.Undo();
        Assert.Equal(string.Empty, model.Text);

        model.History.Redo();
        Assert.Equal("alpha", model.Text);
    }

    [Fact]
    public void SetTrackedProperty_Float_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.Rate = 1.5f;
        Assert.Equal(1.5f, model.Rate);

        model.History.Undo();
        Assert.Equal(0f, model.Rate);

        model.History.Redo();
        Assert.Equal(1.5f, model.Rate);
    }

    [Fact]
    public void SetTrackedProperty_List_CanUndoRedoByReference()
    {
        var model = new SampleUndoRedoModel();
        var oldList = model.Items;
        var newList = new CoreTrackedList { 1, 2, 3 };

        model.Items = newList;
        Assert.Same(newList, model.Items);

        model.History.Undo();
        Assert.Same(oldList, model.Items);

        model.History.Redo();
        Assert.Same(newList, model.Items);
    }

    [Fact]
    public void TrackedList_DirectAddRemoveReplace_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.Items.Add(10);
        model.Items.Add(20);
        model.Items[1] = 99;
        model.Items.RemoveAt(0);

        Assert.Equal([99], model.Items);
        Assert.Equal(4, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal([10, 99], model.Items);

        model.History.Undo();
        Assert.Equal([10, 20], model.Items);

        model.History.Undo();
        Assert.Equal([10], model.Items);

        model.History.Undo();
        Assert.Empty(model.Items);

        model.History.Redo();
        model.History.Redo();
        model.History.Redo();
        model.History.Redo();
        Assert.Equal([99], model.Items);
    }

    [Fact]
    public void SetTrackedProperty_Vector2_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();
        var original = model.V2;
        var v = new CoreVector2(2, 4);

        model.V2 = v;
        Assert.Same(v, model.V2);
        Assert.Equal(2f, model.V2.X);
        Assert.Equal(4f, model.V2.Y);

        model.History.Undo();
        Assert.Same(original, model.V2);

        model.History.Redo();
        Assert.Same(v, model.V2);
    }

    [Fact]
    public void Vector2_ElementEdit_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.V2.X = 9;
        Assert.Equal(9f, model.V2.X);
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(0f, model.V2.X);

        model.History.Redo();
        Assert.Equal(9f, model.V2.X);
    }

    [Fact]
    public void SetTrackedProperty_Vector3_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();
        var original = model.V3;
        var v = new CoreVector3(1, 2, 3);

        model.V3 = v;
        Assert.Same(v, model.V3);
        Assert.Equal(1f, model.V3.X);
        Assert.Equal(2f, model.V3.Y);
        Assert.Equal(3f, model.V3.Z);

        model.History.Undo();
        Assert.Same(original, model.V3);

        model.History.Redo();
        Assert.Same(v, model.V3);
    }

    [Fact]
    public void Vector3_ElementEdit_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.V3.Z = 12;
        Assert.Equal(12f, model.V3.Z);
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(0f, model.V3.Z);

        model.History.Redo();
        Assert.Equal(12f, model.V3.Z);
    }

    [Fact]
    public void SetTrackedProperty_Vector4_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();
        var original = model.V4;
        var v = new CoreVector4(1, 2, 3, 4);

        model.V4 = v;
        Assert.Same(v, model.V4);
        Assert.Equal(1f, model.V4.X);
        Assert.Equal(2f, model.V4.Y);
        Assert.Equal(3f, model.V4.Z);
        Assert.Equal(4f, model.V4.W);

        model.History.Undo();
        Assert.Same(original, model.V4);

        model.History.Redo();
        Assert.Same(v, model.V4);
    }

    [Fact]
    public void Vector4_ElementEdit_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.V4.W = 3.5f;
        Assert.Equal(3.5f, model.V4.W);
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(0f, model.V4.W);

        model.History.Redo();
        Assert.Equal(3.5f, model.V4.W);
    }

    [Fact]
    public void SetTrackedProperty_Matrix3x3_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();
        var original = model.M3;
        var m = new CoreMatrix3x3();
        m.Set(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);

        model.M3 = m;
        Assert.Same(m, model.M3);
        Assert.Equal(5f, model.M3.M22);

        model.History.Undo();
        Assert.Same(original, model.M3);

        model.History.Redo();
        Assert.Same(m, model.M3);
    }

    [Fact]
    public void Matrix3x3_ElementEdit_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.M3.M23 = 20;
        Assert.Equal(20f, model.M3.M23);
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(0f, model.M3.M23);

        model.History.Redo();
        Assert.Equal(20f, model.M3.M23);
    }

    [Fact]
    public void SetTrackedProperty_Matrix4x4_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();
        var original = model.M4;
        var m = new CoreMatrix4x4
        {
            M11 = 1,
            M12 = 2,
            M13 = 3,
            M14 = 4,
            M21 = 5,
            M22 = 6,
            M23 = 7,
            M24 = 8,
            M31 = 9,
            M32 = 10,
            M33 = 11,
            M34 = 12,
            M41 = 13,
            M42 = 14,
            M43 = 15,
            M44 = 16
        };

        model.M4 = m;
        Assert.Same(m, model.M4);
        Assert.Equal(11f, model.M4.M33);

        model.History.Undo();
        Assert.Same(original, model.M4);

        model.History.Redo();
        Assert.Same(m, model.M4);
    }

    [Fact]
    public void Matrix4x4_ElementEdit_CanUndoRedo()
    {
        var model = new SampleUndoRedoModel();

        model.M4.M14 = 7;
        Assert.Equal(7f, model.M4.M14);
        Assert.Equal(1, model.History.UndoCount);

        model.History.Undo();
        Assert.Equal(0f, model.M4.M14);

        model.History.Redo();
        Assert.Equal(7f, model.M4.M14);
    }

    private sealed class SampleUndoRedoModel : UndoRedoModel
    {
        private int _value;
        private string _text = string.Empty;
        private float _rate;
        private CoreTrackedList _items = [];
        private CoreVector2 _v2 = new(0, 0);
        private CoreVector3 _v3 = new(0, 0, 0);
        private CoreVector4 _v4 = new(0, 0, 0, 0);
        private CoreMatrix3x3 _m3 = new();
        private CoreMatrix4x4 _m4 = new();

        public SampleUndoRedoModel()
        {
            TrackNestedProperties(_v2, nameof(V2), "Value");
            TrackNestedProperties(_v3, nameof(V3), "Value");
            TrackNestedProperties(_v4, nameof(V4), "Value");
            TrackNestedProperties(_m3, nameof(M3));
            TrackNestedProperties(_m4, nameof(M4), "Value");
            TrackNestedCollection(_items, _items, nameof(Items));
        }

        public int Value
        {
            get => _value;
            set => SetTrackedProperty(ref _value, value);
        }

        public string Text
        {
            get => _text;
            set => SetTrackedProperty(ref _text, value);
        }

        public float Rate
        {
            get => _rate;
            set => SetTrackedProperty(ref _rate, value);
        }

        public CoreTrackedList Items
        {
            get => _items;
            set => SetTrackedProperty(ref _items, value);
        }

        public CoreVector2 V2
        {
            get => _v2;
            set => SetTrackedProperty(ref _v2, value);
        }

        public CoreVector3 V3
        {
            get => _v3;
            set => SetTrackedProperty(ref _v3, value);
        }

        public CoreVector4 V4
        {
            get => _v4;
            set => SetTrackedProperty(ref _v4, value);
        }

        public CoreMatrix3x3 M3
        {
            get => _m3;
            set => SetTrackedProperty(ref _m3, value);
        }

        public CoreMatrix4x4 M4
        {
            get => _m4;
            set => SetTrackedProperty(ref _m4, value);
        }
    }
}
