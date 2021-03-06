﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
#if NETFRAMEWORK
using System.Security.Permissions;
#endif

using vm.Aspects.Diagnostics.Implementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Delegate Script is the type of the generated dump script that can be cached and reused.
    /// </summary>
    /// <param name="instance">The instance being dumped.</param>
    /// <param name="classDumpData">The data containing the metadata type and the dump attribute.</param>
    /// <param name="dumper">The dumper which has the current writer.</param>
    /// <param name="dumpState">The current state of the dump.</param>
    internal delegate void Script(
        object instance,
        ClassDumpData classDumpData,
        ObjectTextDumper dumper,
        DumpState dumpState);

    /// <summary>
    /// Class ObjectTextDumper. This class cannot be inherited. The main class which dumps the requested object.
    /// </summary>
    public sealed partial class ObjectTextDumper : IDisposable
    {
        static ReaderWriterLockSlim _syncDefaultDumpSettings = new ReaderWriterLockSlim();
        static DumpSettings _defaultDumpSettings             = DumpSettings.Default;

        /// <summary>
        /// Gets or sets the object dumper settings.
        /// </summary>
        public static DumpSettings DefaultDumpSettings
        {
            get
            {
                try
                {
                    _syncDefaultDumpSettings.EnterReadLock();
                    return _defaultDumpSettings;
                }
                finally
                {
                    _syncDefaultDumpSettings.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _syncDefaultDumpSettings.EnterWriteLock();
                    _defaultDumpSettings = value;
                }
                finally
                {
                    _syncDefaultDumpSettings.ExitWriteLock();
                }
            }
        }

        #region Fields
        /// <summary>
        /// The current indent.
        /// </summary>
        internal int _indentLevel;

        /// <summary>
        /// The number of spaces in a single indent.
        /// </summary>
        internal readonly int _indentSize;

        /// <summary>
        /// Flag that the current writer is actually our DumpTextWriter.
        /// </summary>
        readonly bool _isDumpWriter;

        /// <summary>
        /// The current maximum depth of recursing into the aggregated objects. When it goes down to 0 - the recursion should stop.
        /// </summary>
        internal int _maxDepth = int.MinValue;
        #endregion

        #region Internal properties for access by the DumpState
        ReaderWriterLockSlim _syncSettings = new ReaderWriterLockSlim();
        DumpSettings _settings             = DumpSettings.Default;

        /// <summary>
        /// The dump writer.
        /// </summary>
        internal TextWriter Writer { get; }

        /// <summary>
        /// Gets the settings for this instance.
        /// </summary>
        public DumpSettings InstanceSettings
        {
            get
            {
                try
                {
                    _syncSettings.EnterReadLock();
                    return _settings;
                }
                finally
                {
                    _syncSettings.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _syncSettings.EnterWriteLock();
                    _settings = value;
                }
                finally
                {
                    _syncSettings.ExitWriteLock();
                }
            }
        }

        internal DumpSettings Settings { get; set; }

        /// <summary>
        /// Gets the member information comparer, used to order the dumped members in the desired sort order.
        /// </summary>
        internal IMemberInfoComparer MemberInfoComparer { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the currently dumped instance is sub-expression of a previously dumped expression.
        /// </summary>
        internal bool IsSubExpression { get; set; }

        /// <summary>
        /// Contains references to all dumped objects to avoid infinite dumping due to cyclical references.
        /// </summary>
        internal HashSet<DumpedObject> DumpedObjects { get; } = new HashSet<DumpedObject>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTextDumper" /> class with a text writer where to dump the object and initial indent
        /// level.
        /// </summary>
        /// <param name="writer">The text writer where to dump the object to.</param>
        /// <param name="memberInfoComparer">
        /// The member comparer used to order the dumped members in the desired sort order.
        /// If <see langword="null"/> the created here dumper instance will use the default member sorting order.
        /// </param>
        /// <exception cref="ArgumentNullException">writer</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="writer" /> is <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ObjectTextDumper(
            TextWriter writer,
            IMemberInfoComparer memberInfoComparer = null)
        {
            Writer             = writer ?? throw new ArgumentNullException(nameof(writer));
            MemberInfoComparer = memberInfoComparer ?? new MemberInfoComparer();
            Settings           = DefaultDumpSettings;
            _indentSize        = Settings.IndentSize;

            if (Writer is StringWriter)
            {
                // wrap the writer in DumpTextWriter - handles better the indentations and
                // limits the dump output to DumpTextWriter.DefaultMaxLength
                Writer        = new DumpTextWriter((StringWriter)Writer, Settings.MaxDumpLength);
                _isDumpWriter = true;
            }
        }
        #endregion

        #region Public method/s
        /// <summary>
        /// Dumps the specified object in a text form to this object's <see cref="TextWriter" /> instance.
        /// </summary>
        /// <param name="value">The object to be dumped.</param>
        /// <param name="dumpMetadata">Optional metadata class to use to extract the dump parameters, options and settings. If not specified, the dump metadata will be
        /// extracted from the <see cref="MetadataTypeAttribute" /> attribute applied to <paramref name="value" />'s class if specified otherwise from
        /// the attributes applied within the class itself.</param>
        /// <param name="dumpAttribute">An explicit dump attribute to be applied at a class level. If not specified the <see cref="MetadataTypeAttribute" /> attribute applied to
        /// <paramref name="value" />'s class or <see cref="DumpAttribute.Default" /> will be assumed.</param>
        /// <param name="initialIndentLevel">The initial indent level.</param>
        /// <returns>The <paramref name="value" /> parameter.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ObjectTextDumper Dump(
            object value,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null,
            int initialIndentLevel = DumpSettings.DefaultInitialIndentLevel)
        {
#if NETFRAMEWORK
            var reflectionPermission = new ReflectionPermission(PermissionState.Unrestricted);
            var revertPermission     = false;
#endif

            try
            {
#if NETFRAMEWORK
                // assert the permission and dump
                reflectionPermission.Demand();
                revertPermission = true;
                reflectionPermission.Assert();
#endif

                // dump
                _indentLevel = initialIndentLevel >= DumpSettings.DefaultInitialIndentLevel ? initialIndentLevel : DumpSettings.DefaultInitialIndentLevel;
                _maxDepth    = int.MinValue;
                Settings     = InstanceSettings;

                DumpObject(value, dumpMetadata, dumpAttribute);
            }
            catch (SecurityException)
            {
                Writer.WriteLine();
                Writer.Write(Resources.CallerDoesNotHavePermissionFormat, value?.ToString() ?? DumpUtilities.Null);
            }
            catch (TypeAccessException)
            {
                Writer.WriteLine();
                Writer.Write(Resources.CallerDoesNotHavePermissionFormat, value?.ToString() ?? DumpUtilities.Null);
            }
            catch (Exception x)
            {
                var message = $"\n\nATTENTION:\nThe TextDumper threw an exception:\n{x.ToString()}";

                Writer.WriteLine(message);
                Debug.WriteLine(message);
            }
            finally
            {
#if NETFRAMEWORK
                // revert the permission assert
                if (revertPermission)
                    CodeAccessPermission.RevertAssert();
#endif

                // clear the dumped objects register
                DumpedObjects.Clear();
                // prepare our writer for a new dump
                if (_isDumpWriter)
                    ((DumpTextWriter)Writer).Reset();
            }

            return this;
        }
        #endregion

        /// <summary>
        /// Increases the indentation of the writer by <see cref="_indentSize"/>.
        /// </summary>
        internal void Indent() => Writer.Indent(++_indentLevel, _indentSize);

        /// <summary>
        /// Decreases the indentation of the writer by <see cref="_indentSize"/>.
        /// </summary>
        internal void Unindent() => Writer.Unindent(--_indentLevel, _indentSize);

        #region Internal methods
        internal void DumpObject(
            object obj,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null,
            DumpState parentState = null)
        {
            if (Writer.DumpedBasicValue(obj, dumpAttribute)  ||  Writer.DumpedBasicNullable(obj, dumpAttribute))    // incl. null
                return;

            // resolve the class metadata and the dump attribute
            ClassDumpData classDumpData;
            var objectType = obj.GetType();

            if (dumpMetadata==null)
            {
                classDumpData = ClassMetadataResolver.GetClassDumpData(objectType);
                if (dumpAttribute != null)
                    classDumpData.DumpAttribute = dumpAttribute;
            }
            else
                classDumpData = new ClassDumpData(dumpMetadata, dumpAttribute);

            // if we're too deep - stop here.
            if (_maxDepth == int.MinValue)
                _maxDepth = classDumpData.DumpAttribute.MaxDepth;

            if (_maxDepth < 0)
            {
                Writer.Write(Resources.DumpReachedMaxDepth);
                return;
            }

            // save here the IsSubExpression flag as it will change if obj is Expression and IsSubExpression==false.
            // but in the end of this method restore the value from this local variable. See below.
            var isSubExpressionStore = IsSubExpression;

            // slow dump vs. run script?
            bool isTopLevelObject = parentState == null;
            var buildScript       = Settings.UseDumpScriptCache  &&  !obj.IsDynamicObject();
            Script script         = null;

            using (var state = new DumpState(this, obj, classDumpData, buildScript))
            {
                if (buildScript)
                {
                    // does the script exist or is it in process of building
                    if (DumpScriptCache.TryFind(this, obj, classDumpData, out script))
                    {
                        if (script != null)
                        {
                            if (isTopLevelObject)
                                Writer.Indent(_indentLevel, _indentSize)
                                      .WriteLine();

                            script(obj, classDumpData, this, state);
                        }

                        return;
                    }

                    DumpScriptCache.BuildingScriptFor(this, objectType, classDumpData);
                }
                else
                {
                    if (isTopLevelObject)
                        Writer.Indent(_indentLevel, _indentSize)
                           .WriteLine();
                }

                if (!state.DumpedAlready())     // the object has been dumped already (block circular and repeating references)
                {
                    // this object will be dumped below.
                    // Add it to the dumped objects now so that if nested property refers back to it, it won't be dumped in an infinite recursive chain.
                    DumpedObjects.Add(new DumpedObject(obj, objectType));

                    if (!state.DumpedCollection(classDumpData.DumpAttribute, false))   // custom collections are dumped after dumping all other properties (see below *  )
                    {
                        Stack<DumpState> statesWithRemainingProperties = new Stack<DumpState>();
                        Queue<DumpState> statesWithTailProperties = new Queue<DumpState>();

                        // recursively dump all properties with non-negative order in class inheritance descending order (base classes' properties first)
                        // and if there are more properties  to be dumped put them in the stack 
                        if (!DumpedTopProperties(state, statesWithRemainingProperties))
                        {
                            // dump all properties with negative order in class ascending order (derived classes' properties first)
                            DumpRemainingProperties(statesWithRemainingProperties, statesWithTailProperties);

                            // dump all properties with Order=int.MinValue in ascending order (derived classes' properties first)
                            DumpTailProperties(statesWithTailProperties);

                            // * if the object implements IEnumerable and the state allows it - dump the elements.
                            state.DumpedCollection(classDumpData.DumpAttribute, true);
                        }

                        // we are done dumping
                        state.Unindent();
                    }
                }

                if (buildScript  &&  state.DumpScript != null)
                    script = DumpScriptCache.Add(this, objectType, classDumpData, state.DumpScript);

                if (!buildScript)
                {
                    if (isTopLevelObject)
                        Writer.Unindent(_indentLevel, _indentSize);
                }
                else
                {
                    if (script != null)
                    {
                        if (isTopLevelObject)
                            Writer.Indent(_indentLevel, _indentSize)
                                  .WriteLine();

                        script(obj, classDumpData, this, state);
                    }
                }

                // restore here the IsSubExpression flag as it have changed if obj is Expression and IsSubExpression==false.
                IsSubExpression = isSubExpressionStore;
            }
        }

        /// <summary>
        /// Dumps the properties with non-negative dump order.
        /// </summary>
        /// <param name="state">The current dump state.</param>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <returns><c>true</c> if this is a dump tree leaf object, the current dump item is one of:
        /// <list type="bullet">
        /// <item><c>typeof(System.Object)</c></item><item><see cref="MemberInfo" /></item>
        /// <item>a delegate</item>
        /// <item>should not be dumped marked with <c>DumpAttribute(false)</c></item>
        /// </list>
        /// ; otherwise <c>false</c>.</returns>
        bool DumpedTopProperties(
            DumpState state,
            Stack<DumpState> statesWithRemainingProperties)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (statesWithRemainingProperties == null)
                throw new ArgumentNullException(nameof(statesWithRemainingProperties));

            if (state.DumpedDelegate()    ||
                state.DumpedMemberInfo()  ||
                state.DumpedRootClass())
            {
                // we reached the root of the hierarchy, indent and while unwinding the stack dump the properties from each descending class
                state.Indent();
                state.Dispose();
                return true;
            }

            var baseDumpState = state.GetBaseTypeDumpState();

            // 1. dump the properties of the base class with non-negative order 
            DumpedTopProperties(baseDumpState, statesWithRemainingProperties);

            // 2. dump the non-negative order properties of the current class
            while (state.MoveNext())
            {
                // if we reached a negative order property,
                if (state.CurrentMemberDumpAttribute.Order < 0)
                {
                    // put the state in the stack of states with remaining properties to be dumped
                    // and suspend the dumping at this inheritance level
                    statesWithRemainingProperties.Push(state);
                    return false;
                }
                // otherwise dump the property
                state.DumpProperty();
            }

            // if we are here all properties have been dumped (no negative order properties) 
            // and we do not need the state anymore.
            state.Dispose();

            return false;
        }

        /// <summary>
        /// Dumps the properties with negative dump order.
        /// </summary>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <param name="statesWithTailProperties">The queue containing the states which have tail properties.</param>
        static void DumpRemainingProperties(
            Stack<DumpState> statesWithRemainingProperties,
            Queue<DumpState> statesWithTailProperties)
        {
            if (statesWithRemainingProperties == null)
                throw new ArgumentNullException(nameof(statesWithRemainingProperties));
            if (statesWithTailProperties == null)
                throw new ArgumentNullException(nameof(statesWithTailProperties));

            bool isTailProperty;

            while (statesWithRemainingProperties.Any())
            {
                // pop the highest inheritance tree dump state from the stack
                var state = statesWithRemainingProperties.Pop();

                do
                {
                    isTailProperty = state.CurrentMemberDumpAttribute.Order == int.MinValue;

                    if (isTailProperty)
                    {
                        // the properties with order int.MinValue are to be dumped last (at the dump tail)
                        // put the state in the tail queue and suspend the dumping again
                        statesWithTailProperties.Enqueue(state);
                        break;
                    }

                    // otherwise dump the current property
                    state.DumpProperty();
                }
                while (state.MoveNext());

                // if all properties have been dumped (no int.MinValue order properties) we do not need the state anymore.
                if (!isTailProperty)
                    state.Dispose();
            }
        }

        /// <summary>
        /// Dumps the properties with order int.MinValue
        /// </summary>
        /// <param name="statesWithTailProperties">The bottom properties collection (usually a queue).</param>
        static void DumpTailProperties(
            IEnumerable<DumpState> statesWithTailProperties)
        {
            if (statesWithTailProperties == null)
                throw new ArgumentNullException(nameof(statesWithTailProperties));

            foreach (var state in statesWithTailProperties)
                using (state)
                    do
                        state.DumpProperty();
                    while (state.MoveNext());
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer of <see cref="ObjectTextDumper"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<Writer>k__BackingField", Justification = "We don't necessarily")]
        void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing && _isDumpWriter)
                // if the writer wraps foreign StringWriter, it is smart enough not to dispose it
                Writer.Dispose();

            _syncSettings.Dispose();
        }
        #endregion
    }
}
