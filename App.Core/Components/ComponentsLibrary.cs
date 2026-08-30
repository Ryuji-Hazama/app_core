using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace App.Core.Components
{
    internal class ComponentsJson
    {
        public List<Component> Components { get; set; } = new List<Component>();
    }

    internal class Component
    {
        public string Name { get; set; } = string.Empty;
        public string Assembly { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class ComponentsLibrary : IComponentsLibrary
    {
        private readonly App.Config _config = App.ConfigManager.GetConfig();
        private readonly Dictionary<string, object> _components = new();

        public ComponentsLibrary()
        {
            string componentsListPath = _config.App.ComponentsList;

            if (File.Exists(componentsListPath))
            {
                string json = File.ReadAllText(componentsListPath);
                ComponentsJson componentsJson = JsonSerializer.Deserialize<ComponentsJson>(json) ?? new ComponentsJson();

                foreach (Component component in componentsJson.Components)
                {
                    try
                    {
                        _components[component.Name] = Invoke(component.Assembly, component.Context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading component '{component.Name}': {ex.Message}");
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"Components list file not found: {componentsListPath}");
            }
        }

        public object Invoke(string assemblyName, string context)
        {
            string assemblyPath = Path.Combine(_config.App.AssemblyPath, assemblyName);

            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Assembly file not found: {assemblyPath}");
            }

            var loadContext = new AssemblyLoadContext("plugin-load-context", isCollectible: true);
            loadContext.Resolving += (context, asmName) =>
            {
                var existing = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == asmName.Name);
                if (existing != null)
                {
                    return existing;
                }

                var dependencyPath = Path.Combine(_config.App.AssemblyPath, asmName.Name + ".dll");
                if (File.Exists(dependencyPath))
                {
                    return context.LoadFromAssemblyPath(dependencyPath);
                }

                return null;
            };

            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var type = assembly.GetType(context);
            if (type == null)
            {
                throw new TypeLoadException($"Type '{context}' not found in assembly '{assemblyName}'.");
            }

            var instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not create an instance of type '{context}'.");
            }

            return instance;
        }

        public T GetComponent<T>(string componentName)
        {
            if (_components.TryGetValue(componentName, out var component))
            {
                return (T)component;
            }
            throw new KeyNotFoundException($"Component '{componentName}' not found.");
        }
    }

    public interface IComponentsLibrary
    {
        T GetComponent<T>(string componentName);
    }
}