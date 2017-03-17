//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Data.Entity.Infrastructure.MappingViews;

[assembly: DbMappingViewCacheTypeAttribute(
    typeof(vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.TestRepository),
    typeof(Edm_EntityMappingGeneratedViews.ViewsForBaseEntitySets606c03ebdfa04f4f677cc4dabaa53db5f5fefc9dc42db02fb431bcf68bebc957))]

namespace Edm_EntityMappingGeneratedViews
{
    using System;
    using System.CodeDom.Compiler;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Implements a mapping view cache.
    /// </summary>
    [GeneratedCode("Entity Framework Power Tools", "0.9.0.0")]
    internal sealed class ViewsForBaseEntitySets606c03ebdfa04f4f677cc4dabaa53db5f5fefc9dc42db02fb431bcf68bebc957 : DbMappingViewCache
    {
        /// <summary>
        /// Gets a hash value computed over the mapping closure.
        /// </summary>
        public override string MappingHashValue
        {
            get { return "606c03ebdfa04f4f677cc4dabaa53db5f5fefc9dc42db02fb431bcf68bebc957"; }
        }

        /// <summary>
        /// Gets a view corresponding to the specified extent.
        /// </summary>
        /// <param name="extent">The extent.</param>
        /// <returns>The mapping view, or null if the extent is not associated with a mapping view.</returns>
        public override DbMappingView GetView(EntitySetBase extent)
        {
            if (extent == null)
            {
                throw new ArgumentNullException("extent");
            }

            var extentName = extent.EntityContainer.Name + "." + extent.Name;

            if (extentName == "CodeFirstDatabase.HiLoIdentityGenerator")
            {
                return GetView0();
            }

            if (extentName == "TestRepository.HiLoIdentityGenerators")
            {
                return GetView1();
            }

            if (extentName == "CodeFirstDatabase.Entity")
            {
                return GetView2();
            }

            if (extentName == "CodeFirstDatabase.Value")
            {
                return GetView3();
            }

            if (extentName == "TestRepository.Entities")
            {
                return GetView4();
            }

            if (extentName == "TestRepository.Values")
            {
                return GetView5();
            }

            return null;
        }

        /// <summary>
        /// Gets the view for CodeFirstDatabase.HiLoIdentityGenerator.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView0()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing HiLoIdentityGenerator
        [CodeFirstDatabaseSchema.HiLoIdentityGenerator](T1.HiLoIdentityGenerator_EntitySetName, T1.HiLoIdentityGenerator_HighValue, T1.HiLoIdentityGenerator_MaxLowValue)
    FROM (
        SELECT 
            T.EntitySetName AS HiLoIdentityGenerator_EntitySetName, 
            T.HighValue AS HiLoIdentityGenerator_HighValue, 
            T.MaxLowValue AS HiLoIdentityGenerator_MaxLowValue, 
            True AS _from0
        FROM TestRepository.HiLoIdentityGenerators AS T
    ) AS T1");
        }

        /// <summary>
        /// Gets the view for TestRepository.HiLoIdentityGenerators.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView1()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing HiLoIdentityGenerators
        [vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.HiLoIdentityGenerator](T1.HiLoIdentityGenerator_EntitySetName, T1.HiLoIdentityGenerator_HighValue, T1.HiLoIdentityGenerator_MaxLowValue)
    FROM (
        SELECT 
            T.EntitySetName AS HiLoIdentityGenerator_EntitySetName, 
            T.HighValue AS HiLoIdentityGenerator_HighValue, 
            T.MaxLowValue AS HiLoIdentityGenerator_MaxLowValue, 
            True AS _from0
        FROM CodeFirstDatabase.HiLoIdentityGenerator AS T
    ) AS T1");
        }

        /// <summary>
        /// Gets the view for CodeFirstDatabase.Entity.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView2()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing Entity
        [CodeFirstDatabaseSchema.Entity](T1.Entity_Id, T1.Entity_RepositoryId, T1.Entity_UniqueId, T1.Entity_CreatedOn, T1.Entity_UpdatedOn, T1.Entity_ConcurrencyStamp)
    FROM (
        SELECT 
            T.Id AS Entity_Id, 
            T.RepositoryId AS Entity_RepositoryId, 
            T.UniqueId AS Entity_UniqueId, 
            T.CreatedOn AS Entity_CreatedOn, 
            T.UpdatedOn AS Entity_UpdatedOn, 
            T.ConcurrencyStamp AS Entity_ConcurrencyStamp, 
            True AS _from0
        FROM TestRepository.Entities AS T
    ) AS T1");
        }

        /// <summary>
        /// Gets the view for CodeFirstDatabase.Value.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView3()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing Value
        [CodeFirstDatabaseSchema.Value](T1.Value_Id, T1.Value_RepositoryId, T1.Value_EntityId, T1.Value_CreatedOn, T1.Value_UpdatedOn, T1.Value_ConcurrencyStamp)
    FROM (
        SELECT 
            T.Id AS Value_Id, 
            T.RepositoryId AS Value_RepositoryId, 
            T.EntityId AS Value_EntityId, 
            T.CreatedOn AS Value_CreatedOn, 
            T.UpdatedOn AS Value_UpdatedOn, 
            T.ConcurrencyStamp AS Value_ConcurrencyStamp, 
            True AS _from0
        FROM TestRepository.Values AS T
    ) AS T1");
        }

        /// <summary>
        /// Gets the view for TestRepository.Entities.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView4()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing Entities
        [vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Entity](T1.Entity_Id, T1.Entity_UniqueId, T1.Entity_RepositoryId, T1.Entity_CreatedOn, T1.Entity_UpdatedOn, T1.Entity_ConcurrencyStamp)
    FROM (
        SELECT 
            T.Id AS Entity_Id, 
            T.UniqueId AS Entity_UniqueId, 
            T.RepositoryId AS Entity_RepositoryId, 
            T.CreatedOn AS Entity_CreatedOn, 
            T.UpdatedOn AS Entity_UpdatedOn, 
            T.ConcurrencyStamp AS Entity_ConcurrencyStamp, 
            True AS _from0
        FROM CodeFirstDatabase.Entity AS T
    ) AS T1");
        }

        /// <summary>
        /// Gets the view for TestRepository.Values.
        /// </summary>
        /// <returns>The mapping view.</returns>
        private static DbMappingView GetView5()
        {
            return new DbMappingView(@"
    SELECT VALUE -- Constructing Values
        [vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Value](T1.Value_Id, T1.Value_RepositoryId, T1.Value_EntityId, T1.Value_CreatedOn, T1.Value_UpdatedOn, T1.Value_ConcurrencyStamp)
    FROM (
        SELECT 
            T.Id AS Value_Id, 
            T.RepositoryId AS Value_RepositoryId, 
            T.EntityId AS Value_EntityId, 
            T.CreatedOn AS Value_CreatedOn, 
            T.UpdatedOn AS Value_UpdatedOn, 
            T.ConcurrencyStamp AS Value_ConcurrencyStamp, 
            True AS _from0
        FROM CodeFirstDatabase.[Value] AS T
    ) AS T1");
        }
    }
}
