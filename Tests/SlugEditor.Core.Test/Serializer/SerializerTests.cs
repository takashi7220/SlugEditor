using SlugEditor.Core.Assembly;
using SlugEditor.Core.Serializer;
using SlugEditor.Test;

namespace SlugEditor.Core.Test.Serializer;

public sealed class SerializerTests
{
    [Fact]
    public void JsonSerializerEngine_RoundTrip_SimpleObject()
    {
        var engine = new JsonSerializerEngine();
        var source = new SampleDocument
        {
            Name = "sample",
            Version = 2,
            Tags = new List<string> { "a", "b" }
        };

        var raw = engine.SerializeToString(source, "Root");
        var restored = engine.DeserializeFromString<SampleDocument>(raw);

        Assert.Equal("sample", restored.Name);
        Assert.Equal(2, restored.Version);
        Assert.Equal(["a", "b"], restored.Tags);
    }

    [Fact]
    public void JsonSerializerEngine_UsesCustomSerializer()
    {
        var engine = new JsonSerializerEngine(customSerializers: [new SamplePointCustomSerializer()]);
        var source = new SamplePoint(3, 4);

        var raw = engine.SerializeToString(source);
        var restored = engine.DeserializeFromString<SamplePoint>(raw);

        Assert.Equal(3, restored.X);
        Assert.Equal(4, restored.Y);
    }

    [Fact]
    public void JsonSerializerEngine_UsesDataTemplateSerializer()
    {
        var engine = new JsonSerializerEngine(dataTemplates: [new ExternalValueTemplateSerializer()]);
        var source = new ExternalValue { Code = "A-10" };

        var raw = engine.SerializeToString(source);
        var restored = engine.DeserializeFromString<ExternalValue>(raw);

        Assert.Equal("A-10", restored.Code);
    }

    [Fact]
    public void ArchiverFactory_GenerateJsonArchiver()
    {
        var assemblyService = new AssemblyService(typeof(SerializerTests).Assembly);
        var factory = new ArchiverFactory(assemblyService);

        var archiver = factory.Generate("Json");

        Assert.Equal("Json", archiver.Backend.Name);
    }

    [Fact]
    public void ArchiverFactory_Generate_ThrowsForUnknownBackend()
    {
        var assemblyService = new AssemblyService(typeof(SerializerTests).Assembly);
        var factory = new ArchiverFactory(assemblyService);

        var thrown = false;
        try
        {
            _ = factory.Generate("Unknown");
        }
        catch (KeyNotFoundException)
        {
            thrown = true;
        }

        Assert.True(thrown);
    }

    [Fact]
    public void JsonSerializerEngine_RoundTrip_Struct()
    {
        var engine = new JsonSerializerEngine();
        var source = new SampleStruct
        {
            Id = 7,
            Ratio = 1.25f
        };

        var raw = engine.SerializeToString(source);
        var restored = engine.DeserializeFromString<SampleStruct>(raw);

        Assert.Equal(7, restored.Id);
        Assert.Equal(1.25f, restored.Ratio);
    }

    [Fact]
    public void JsonSerializerEngine_RoundTrip_ClassWithListAndArray()
    {
        var engine = new JsonSerializerEngine();
        var source = new ComplexContainer
        {
            Title = "container",
            Values = new List<int> { 1, 2, 3 },
            Names = ["A", "B", "C"],
            Nested = new NestedClass { Enabled = true, Label = "ok" }
        };

        var raw = engine.SerializeToString(source);
        var restored = engine.DeserializeFromString<ComplexContainer>(raw);

        Assert.Equal("container", restored.Title);
        Assert.Equal([1, 2, 3], restored.Values);
        Assert.Equal(["A", "B", "C"], restored.Names);
        Assert.True(restored.Nested.Enabled);
        Assert.Equal("ok", restored.Nested.Label);
    }

    public sealed class SampleDocument
    {
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public sealed class SamplePoint
    {
        public SamplePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    public sealed class SamplePointCustomSerializer : CustomSerializer
    {
        public override Type TargetType => typeof(SamplePoint);

        public override ArchiveNode Serialize(IArchiver archiver, object value, string? name = null)
        {
            var point = (SamplePoint)value;
            return new ArchiveNode
            {
                Name = name,
                ValueType = typeof(SamplePoint).AssemblyQualifiedName,
                Children =
                [
                    archiver.Archive(point.X, "X"),
                    archiver.Archive(point.Y, "Y")
                ]
            };
        }

        public override object Deserialize(IArchiver archiver, ArchiveNode node)
        {
            var xNode = node.Children.First(x => x.Name == "X");
            var yNode = node.Children.First(x => x.Name == "Y");
            return new SamplePoint(
                archiver.Unarchive<int>(xNode),
                archiver.Unarchive<int>(yNode));
        }
    }

    public sealed class ExternalValue
    {
        public string Code { get; set; } = string.Empty;
    }

    public sealed class ExternalValueTemplateSerializer : DataTemplateSerializer<ExternalValue>
    {
        public override ArchiveNode Serialize(IArchiver archiver, ExternalValue value, string? name = null)
        {
            return new ArchiveNode
            {
                Name = name,
                ValueType = typeof(ExternalValue).AssemblyQualifiedName,
                Value = value.Code
            };
        }

        public override ExternalValue Deserialize(IArchiver archiver, ArchiveNode node)
        {
            return new ExternalValue { Code = node.Value?.ToString() ?? string.Empty };
        }
    }

    public struct SampleStruct
    {
        public int Id { get; set; }
        public float Ratio { get; set; }
    }

    public sealed class ComplexContainer
    {
        public string Title { get; set; } = string.Empty;
        public List<int> Values { get; set; } = [];
        public string[] Names { get; set; } = [];
        public NestedClass Nested { get; set; } = new();
    }

    public sealed class NestedClass
    {
        public bool Enabled { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
