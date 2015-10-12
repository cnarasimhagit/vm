﻿using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class ObjectContextExtensions.
    /// </summary>
    public static class ObjectContextExtensions
    {
        /// <summary>
        /// Gets the name of the entity set of the type <paramref name="type"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type for which to find the entity set name.</param>
        /// <returns>The name of the entity set.</returns>
        public static string GetEntitySetName(
            this ObjectContext context,
            Type type)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            for (var t = type; t != typeof(object); t = t.BaseType)
            {
                var entitySetName = context.MetadataWorkspace
                                           .GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace)
                                           .BaseEntitySets
                                           .Where(meta => meta.ElementType.Name == t.Name)
                                           .Select(m => m.Name)
                                           .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(entitySetName))
                    return entitySetName;
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the entity set of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to find the entity set name.</typeparam>
        /// <param name="context">The context.</param>
        /// <returns>The name of the entity set.</returns>
        public static string GetEntitySetName<T>(
            this ObjectContext context)
        {
            Contract.Requires<ArgumentNullException>(context != null, nameof(context));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            for (var t = typeof(T); t != typeof(object); t = t.BaseType)
            {
                var entitySetName = context.MetadataWorkspace
                                           .GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace)
                                           .BaseEntitySets
                                           .Where(meta => meta.ElementType.Name == t.Name)
                                           .Select(m => m.Name)
                                           .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(entitySetName))
                    return entitySetName;
            }

            return null;
        }
    }
}