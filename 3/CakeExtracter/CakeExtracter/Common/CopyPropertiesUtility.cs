using System;
using System.Linq;
using CakeExtracter.Common.ReflectionExtensions;

namespace CakeExtracter.Common
{
    public static class CopyPropertiesUtility
    {
        public static void Copy(object source, object target)
        {
            // Join together the source and target properties by case insensitive property name.
            // Then filter on null source values.
            // Then partition into system types and non-system types.
            var items = source
                            .EnumerateProperties()
                            .Join(
                                target.EnumerateProperties(), 
                                c => c.Name, 
                                c => c.Name, 
                                (p, P) => new { V = p.GetValue(source, null), P }, 
                                StringComparer.OrdinalIgnoreCase)
                            .Where(c => c.V != null)
                            .ToLookup(c => c.V.GetType().FullName.StartsWith("System."));

            // Copy values of system types
            items[true].ToList().ForEach(c =>
            {
                c.P.SetValue(target, c.V);
            });

            // Recursively copy non-system types
            items[false].ToList().ForEach(c =>
            {
                var v = c.P.GetValue(target, null);

                // Create new instance if target property has null value (i.e. source has non null but target is null, so new'ing makes sense)
                if (v == null)
                {
                    v = Activator.CreateInstance(c.P.PropertyType);
                    c.P.SetValue(target, v);
                }

                Copy(c.V, v); // RECURSE
            });
        }

        public static TTarget Clone<TTarget>(TTarget source) where TTarget : new()
        {
            TTarget target = new TTarget();
            Copy(source, target);
            return target;
        }
    }
}
