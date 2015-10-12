﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    using EFEntityState = System.Data.Entity.EntityState;
    using EntityState = Repository.EntityState;

    /// <summary>
    /// Class EFSpecifics. Implements <see cref="IOrmSpecifics"/> for Entity Framework.
    /// </summary>
    public class EFSpecifics : IOrmSpecifics
    {
        #region IOrmSpecifics
        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property that should be eagerly loaded.</param>
        /// <returns>The queryable sequence.</returns>
        public IQueryable<T> Fetch<T>(
            IQueryable<T> sequence,
            string path) where T : BaseDomainEntity => QueryableExtensions.Include(sequence, path);

        /// <summary>
        /// Suggests eager fetching of related objects when querying the repository.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the queried sequence.</typeparam>
        /// <typeparam name="TProperty">The type of the property to be eagerly loaded.</typeparam>
        /// <param name="sequence">The queryable sequence.</param>
        /// <param name="path">Specifies the navigation method/property to the property(s) that should be eagerly loaded as a lambda expression.</param>
        /// <returns>The queryable sequence.</returns>
        public IQueryable<T> Fetch<T, TProperty>(
            IQueryable<T> sequence,
            Expression<Func<T, TProperty>> path) where T : BaseDomainEntity => QueryableExtensions.Include(sequence, path);

        /// <summary>
        /// Enlists the repository's back store connection in the ambient transaction.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The repository.</returns>
        public IRepository EnlistInAmbientTransaction(
            IRepository repository)
        {
            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be implemented by EFRepositoryBase descendant.", "repository");

            if (efRepository.ObjectContext.Connection.State == ConnectionState.Open &&
                Transaction.Current != null)
                efRepository.ObjectContext.Connection.EnlistTransaction(Transaction.Current);

            return repository;
        }

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object instead of the actual object itself.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reference is proxy; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsProxy(
            object reference)
        {
            var type = reference.GetType();

            return type != ObjectContext.GetObjectType(type);
        }

        static IDictionary<TypeCode, Func<object, object>> _changeValue = new Dictionary<TypeCode, Func<object, object>>
        {
            [TypeCode.Boolean]  = v => !((bool)v),              
            [TypeCode.Byte]     = v => ((byte)v) + 1,
            [TypeCode.Char]     = v => ((char)v) + 1,
            [TypeCode.DateTime] = v => ((DateTime)v).AddMilliseconds(1),
            [TypeCode.Decimal]  = v => ((decimal)v) + 1,
            [TypeCode.Double]   = v => ((double)v) + 1,
            [TypeCode.Int16]    = v => ((short)v) + 1,
            [TypeCode.Int32]    = v => ((int)v) + 1,
            [TypeCode.Int64]    = v => ((long)v) + 1,
            [TypeCode.UInt16]   = v => ((ushort)v) + 1,
            [TypeCode.UInt32]   = v => ((uint)v) + 1,
            [TypeCode.UInt64]   = v => ((ulong)v) + 1,
            [TypeCode.SByte]    = v => ((sbyte)v) + 1,
            [TypeCode.Single]   = v => ((float)v) + 1,
            [TypeCode.String]   = v => ((string)v) + 1,
        };

        /// <summary>
        /// Determines whether the specified reference is a reference to an ORM generated wrapper/proxy of the actual object and that it
        /// is tracking automatically the changes to the properties.
        /// </summary>
        /// <param name="reference">The reference to be tested.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" /> if the specified reference is proxy; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentException">The repository must be implemented by EFRepositoryBase descendant.;repository</exception>
        /// <remarks>This method is not supposed to be called from operational code but rather from verification code which makes sure that
        /// the reference is tracking the changes to its properties. In order for that to happen all properties must be declared <c>virtual</c>.</remarks>
        public bool IsChangeTracking(
            object reference,
            IRepository repository)
        {
            bool result = false;

            if (!IsProxy(reference))
                return result;

            var efRepository = repository as EFRepositoryBase;

            if (efRepository == null)
                throw new ArgumentException("The repository must be implemented by EFRepositoryBase descendant.", "repository");

            var entry = efRepository.ChangeTracker
                                    .Entries()
                                    .FirstOrDefault(e => ReferenceEquals(e.Entity, reference));

            if (entry.State != EFEntityState.Unchanged)
                return result;

            // Find a simple non-key property which can be modified easily:
            var propertyInfo = reference.GetType()
                                        .GetProperties(BindingFlags.Public | 
                                                       BindingFlags.NonPublic | 
                                                       BindingFlags.Instance |
                                                       BindingFlags.FlattenHierarchy)
                                        .FirstOrDefault(pi => !pi.Name.Contains("Id")  &&  (pi.PropertyType.IsPrimitive ||
                                                                                            pi.PropertyType==typeof(string) ||
                                                                                            pi.PropertyType==typeof(DateTime)));
            Func<object, object> change;

            if (!_changeValue.TryGetValue(Type.GetTypeCode(propertyInfo.PropertyType), out change))
                return result;

            var propOriginalValue = propertyInfo.GetValue(reference, null);

            // change the value of the property
            propertyInfo.SetValue(reference, change(propOriginalValue), null);

            // if the entry's state changed - this is change tracking entity
            result = entry.State==System.Data.Entity.EntityState.Modified;

            // restore the value
            propertyInfo.SetValue(reference, propOriginalValue, null);

            return result;
        }

        /// <summary>
        /// Determines whether a reference to an object or collection which is associated to a principal object is already loaded in memory.
        /// </summary>
        /// <param name="associated">The reference object that needs testing.</param>
        /// <param name="principal">The owner object of the associated.</param>
        /// <param name="propertyName">The name of the <paramref name="principal" />'s property whose value is <paramref name="associated" />.</param>
        /// <param name="repository">The repository.</param>
        /// <returns><see langword="true" /> if the specified reference is loaded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentException">The repository must be implemented by EFRepositoryBase descendant.;repository</exception>
        public bool IsLoaded(
            object associated,
            object principal,
            string propertyName,
            IRepository repository)
        {
            // if it is not an ORM proxy - it must be loaded
            if (!IsProxy(associated))
                return true;

            // get the current repository which can tell if the object is loaded
            var efRepository = repository as EFRepositoryBase;

            // if there is no DbContext - assume that the object is loaded
            if (efRepository == null)
                throw new ArgumentException("The repository must be implemented by EFRepositoryBase descendant.", "repository");

            var ownerEntry = efRepository.ChangeTracker.Entries().FirstOrDefault(e => ReferenceEquals(e.Entity, principal));

            // if the owner is not in the change tracker, consider the reference loaded
            if (ownerEntry == null)
                return true;

            // is it a reference to a collection of objects or reference to an object?
            bool isCollection = associated.GetType().IsGenericType && (associated.GetType().GetGenericTypeDefinition() == typeof(ICollection<>))  ||
                                associated.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICollection<>))) != null;

            if (isCollection)
                return ownerEntry.Collection(propertyName).IsLoaded;
            else
                return ownerEntry.Reference(propertyName).IsLoaded;
        }

        /// <summary>
        /// Determines whether the specified exception is a result of detected optimistic concurrency.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is an optimistic concurrency problem; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsOptimisticConcurrency(
            Exception exception) => (exception is OptimisticConcurrencyException) ||
                                    (exception is DbUpdateConcurrencyException);

        /// <summary>
        /// Determines whether the specified exception is a result of problems connecting to the store.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see langword="true" /> if the specified exception is a connection problem; otherwise, <see langword="false" />.</returns>
        public bool IsConnectionRelated(
            Exception exception) => SqlExceptionExtensions.IsSqlConnectionProblem(exception);

        /// <summary>
        /// Determines whether the specified exception is a result of problems related to transactions isolation.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified exception is a transactions isolation problem; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsTransactionRelated(
            Exception exception) => SqlExceptionExtensions.IsSqlTransactionProblem(exception);

        /// <summary>
        /// Determines whether the specified exception allows for the operation to be repeated, e.g. optimistic concurrency, transaction killed, etc..
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see langword="true" /> if the specified exception is allows for the operation to be repeated; otherwise, <see langword="false" />.</returns>
        public bool IsTransient(
            Exception exception)
        {
            if (IsOptimisticConcurrency(exception))
                return true;

            // walk the exceptions chain and see if the root cause is a SqlException - transaction or connectivity problem
            Exception ex = exception;
            SqlException sqlException = null;

            do
            {
                sqlException = ex as SqlException;
                if (sqlException != null)
                    break;
                ex = ex.InnerException;
            }
            while (ex != null);

            if (sqlException == null)
                return false;

            return IsConnectionRelated(sqlException) ||
                   IsTransactionRelated(sqlException);
        }

        #endregion

        static readonly IDictionary<EntityState, EFEntityState>
            _stateTranslation = new SortedList<EntityState, EFEntityState>()
            {
                { EntityState.Added,    EFEntityState.Added },
                { EntityState.Deleted,  EFEntityState.Deleted },
                { EntityState.Modified, EFEntityState.Modified },
            };

        /// <summary>
        /// Converts <see cref="T:vm.Aspects.Model.Repository.EntityState"/> to <see cref="T:System.Data.Entity.EntityState"/>.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>System.Data.Entity.EntityState.</returns>
        public static EFEntityState ConvertState(EntityState state)
        {
            EFEntityState outState;

            if (!_stateTranslation.TryGetValue(state, out outState))
                outState = EFEntityState.Unchanged;

            return outState;
        }
    }
}