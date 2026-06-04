using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace Retrosharp.DI
{
    /// <summary>
    /// Provides dependency injection container registration by automatically discovering
    /// and invoking all IRegister implementations across loaded assemblies.
    /// </summary>
    public static class ContainerRegistration
    {
        /// <summary>
        /// Registers all discovered IRegister implementations with the service collection.
        /// </summary>
        /// <param name="services">The IServiceCollection to register services with.</param>
        /// <param name="callingAssembly">The root calling assembly (typically Retrosharp.UI.Api or Retrosharp.Engine.Console).</param>
        public static async Task RegisterContainer(IServiceCollection services, Assembly callingAssembly)
        {
            // Get all referenced assemblies from the calling assembly
            var assembliesToLoad = GetAssemblies(callingAssembly);

            // Find all types that implement IRegister
            var registrars = assembliesToLoad
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IRegister).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Instantiate each IRegister implementation and call Register
            foreach (var registrarType in registrars)
            {
                var registrar = (IRegister)Activator.CreateInstance(registrarType)!;
                await registrar.Register(services);
            }
        }

        /// <summary>
        /// Gets all assemblies that should be scanned for IRegister implementations,
        /// including the calling assembly and all its referenced assemblies.
        /// </summary>
        /// <param name="callingAssembly">The root calling assembly.</param>
        /// <returns>A collection of assemblies to scan for registrations.</returns>
        private static IEnumerable<Assembly> GetAssemblies(Assembly callingAssembly)
        {
            var loadedAssemblies = new HashSet<Assembly> { callingAssembly };
            var queue = new Queue<Assembly>(new[] { callingAssembly });

            while (queue.Count > 0)
            {
                var assembly = queue.Dequeue();

                // Get all referenced assembly names
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                foreach (var referencedAssemblyName in referencedAssemblies)
                {
                    try
                    {
                        // Only load Retrosharp-related assemblies to avoid loading unnecessary system assemblies
                        if (referencedAssemblyName.Name?.StartsWith("Retrosharp") == true)
                        {
                            var loadedAssembly = Assembly.Load(referencedAssemblyName);

                            if (loadedAssemblies.Add(loadedAssembly))
                            {
                                queue.Enqueue(loadedAssembly);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Silently skip assemblies that cannot be loaded
                    }
                }
            }

            return loadedAssemblies;
        }
    }
}
