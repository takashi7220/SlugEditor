using System.Reflection;
using SlugEditor.Test;

namespace SlugEditor.Core.Test;

public static class Program
{
    public static int Main()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var factType = typeof(FactAttribute);

        var testMethods = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 && m.GetCustomAttributes(factType, inherit: true).Any())
                .Select(m => (Type: t, Method: m)))
            .ToArray();

        var failed = new List<string>();
        foreach (var (type, method) in testMethods)
        {
            var instance = Activator.CreateInstance(type);
            if (instance is null)
            {
                failed.Add($"{type.FullName}.{method.Name}: could not create test class instance.");
                continue;
            }

            try
            {
                method.Invoke(instance, null);
                Console.WriteLine($"[PASS] {type.Name}.{method.Name}");
            }
            catch (TargetInvocationException ex)
            {
                var error = ex.InnerException ?? ex;
                failed.Add($"{type.Name}.{method.Name}: {error.Message}");
                Console.WriteLine($"[FAIL] {type.Name}.{method.Name}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Executed: {testMethods.Length}, Failed: {failed.Count}");
        if (failed.Count > 0)
        {
            foreach (var item in failed)
            {
                Console.WriteLine(item);
            }

            return 1;
        }

        return 0;
    }
}
