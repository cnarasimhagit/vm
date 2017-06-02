﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        static readonly PropertyInfo _piNamespace             = typeof(Type).GetProperty(nameof(Type.Namespace), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piAssemblyQualifiedName = typeof(Type).GetProperty(nameof(Type.AssemblyQualifiedName), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsArray               = typeof(Type).GetProperty(nameof(Type.IsArray), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsGenericType         = typeof(Type).GetProperty(nameof(Type.IsGenericType), BindingFlags.Public|BindingFlags.Instance);

        static readonly PropertyInfo _piArrayLength           = typeof(Array).GetProperty(nameof(Array.Length), BindingFlags.Public|BindingFlags.Instance);

        static readonly PropertyInfo _piRecurseDump           = typeof(DumpAttribute).GetProperty(nameof(DumpAttribute.RecurseDump), BindingFlags.Public|BindingFlags.Instance);

        static readonly MethodInfo _miGetElementType          = typeof(Type).GetMethod(nameof(Type.GetElementType), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miGetGenericArguments     = typeof(Type).GetMethod(nameof(Type.GetGenericArguments), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);

        static readonly PropertyInfo _piDumperWriter          = typeof(ObjectTextDumper).GetProperty(nameof(ObjectTextDumper.Writer), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly FieldInfo _fiDumperIndentLevel        = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentLevel), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly FieldInfo _fiDumperIndentLength       = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentSize), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly FieldInfo _fiDumperMaxDepth           = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._maxDepth), BindingFlags.NonPublic|BindingFlags.Instance);

        static readonly MethodInfo _miReferenceEquals         = typeof(object).GetMethod(nameof(object.ReferenceEquals), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(object), typeof(object) }, null);
        static readonly MethodInfo _miGetType                 = typeof(object).GetMethod(nameof(object.GetType), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miToString                = typeof(object).GetMethod(nameof(object.ToString), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miIntToString1            = typeof(int).GetMethod(nameof(int.ToString), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(IFormatProvider) }, null);

        static readonly MethodInfo _miGetTypeName             = typeof(Extensions).GetMethod(nameof(Extensions.GetTypeName), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(Type), typeof(bool) }, null);
        static readonly MethodInfo _miGetMaxToDump            = typeof(Extensions).GetMethod(nameof(Extensions.GetMaxToDump), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(DumpAttribute), typeof(int) }, null);

        //static readonly MethodInfo _miDispose                 = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public|BindingFlags.Instance);

        //static readonly MethodInfo _miIndent3                 = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Indent), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null);
        static readonly MethodInfo _miUnindent3               = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Unindent), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null);

        static readonly MethodInfo _miIndent                  = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Indent), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miUnindent                = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Unindent), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[0], null);

        static readonly MethodInfo _miDumperDumpObject        = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.DumpObject), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[] { typeof(object), typeof(Type), typeof(DumpAttribute), typeof(bool) }, null);

        static readonly MethodInfo _miIsMatch                 = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.IsFromSystem), BindingFlags.Public|BindingFlags.Static, null, new[] { typeof(Type) }, null);
        static readonly MethodInfo _miDumpedBasicValue        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicValue), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null);
        static readonly MethodInfo _miDumpedDelegate          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(Delegate) }, null);
        static readonly MethodInfo _miDumpedMemberInfo        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(MemberInfo) }, null);
        //static readonly MethodInfo _miDumpedDictionary        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedDictionary), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(Action<object>), typeof(Action), typeof(Action) }, null);
        //static readonly MethodInfo _miDumpedSequence          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedCollection), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(bool), typeof(Action<object>), typeof(Action), typeof(Action) }, null);

        static readonly MethodInfo _miDumpNullValues          = typeof(ClassDumpData).GetMethod(nameof(ClassDumpData.DumpNullValues), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(DumpAttribute) }, null);

        static readonly MethodInfo _miBitConverterToString    = typeof(BitConverter).GetMethod(nameof(BitConverter.ToString), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null);

        static readonly ConstantExpression _zero              = Expression.Constant(0, typeof(int));
        static readonly ConstantExpression _null              = Expression.Constant(null);
        static readonly ConstantExpression _empty             = Expression.Constant(string.Empty);
        static readonly ConstantExpression _false             = Expression.Constant(false);
        //static readonly ConstantExpression _true              = Expression.Constant(true);

        // parameters to the dump script:
        readonly ParameterExpression _instance;
        readonly ParameterExpression _instanceType          = Expression.Parameter(typeof(Type),             nameof(_instanceType));
        readonly ParameterExpression _instanceAsObject      = Expression.Parameter(typeof(object),           nameof(_instanceAsObject));
        readonly ParameterExpression _instanceDumpAttribute = Expression.Parameter(typeof(DumpAttribute),    nameof(_instanceDumpAttribute));
        readonly ParameterExpression _classDumpData         = Expression.Parameter(typeof(ClassDumpData),    nameof(_classDumpData));
        readonly ParameterExpression _dumper                = Expression.Parameter(typeof(ObjectTextDumper), nameof(_dumper));
        readonly ParameterExpression _tempBool              = Expression.Parameter(typeof(bool),             nameof(_tempBool));

        // helpful expressions inside the dump script:
        readonly Expression _writer;
        readonly Expression _indentLevel;
        readonly Expression _indentLength;
        readonly Expression _maxDepth;
        readonly LabelTarget _return = Expression.Label();

        /// <summary>
        /// The script's body expressions.
        /// </summary>
        ICollection<Expression> _script = new List<Expression>();

        /// <summary>
        /// Stack of temporarily saved script fragments
        /// </summary>
        readonly Stack<ICollection<Expression>> _scripts = new Stack<ICollection<Expression>>();

        /// <summary>
        /// The script is closed for adding more expressions to it.
        /// </summary>
        bool _isClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpScript" /> class.
        /// </summary>
        /// <param name="dumper">The dumper.</param>
        /// <param name="instanceType">Type of the instance.</param>
        /// <param name="callerFile">The caller file.</param>
        /// <param name="callerLine">The caller line.</param>
        public DumpScript(
            ObjectTextDumper dumper,
            Type instanceType,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(dumper       != null, nameof(dumper));
            Contract.Requires<ArgumentNullException>(instanceType != null, nameof(instanceType));

            _instance     = Expression.Parameter(instanceType, nameof(_instance));

            _writer       = Expression.Property(_dumper, _piDumperWriter);
            _indentLevel  = Expression.Field(_dumper, _fiDumperIndentLevel);
            _indentLength = Expression.Field(_dumper, _fiDumperIndentLength);
            _maxDepth     = Expression.Field(_dumper, _fiDumperMaxDepth);

            Add
            (
                callerFile,
                callerLine,
                //// if (ReferenceEqual(_instance,null)) { Writer.Write("<null>"); return; }
                Expression.IfThen
                (
                    Expression.Call(_miReferenceEquals, _instanceAsObject, _null),
                    Expression.Block
                    (
                        Write(Expression.Constant(DumpUtilities.Null, typeof(string))),
                        Expression.Return(_return)
                    )
                ),
                ////_instance              = (<actual instance type>)_instanceAsObject;
                ////_instanceType          = _instance.GetType();
                ////_instanceDumpAttribute = _classDumpData.DumpAttribute;
                Expression.Assign(_instance, Expression.Convert(_instanceAsObject, instanceType)),
                Expression.Assign(_instanceType, Expression.Call(_instance, _miGetType)),
                Expression.Assign(_instanceDumpAttribute, Expression.PropertyOrField(_classDumpData, nameof(ClassDumpData.DumpAttribute)))
            );
        }

        void BeginScriptSegment()
        {
            _scripts.Push(_script);
            _script = new List<Expression>();
        }

        ICollection<Expression> EndScriptSegment()
        {
            var segment = _script;

            _script = _scripts.Pop();

            return segment;
        }

        Expression MemberValue(
            MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Expression.MakeMemberAccess(_instance, mi);
        }

        //// Writer.Indent(++_dumper._indentLevel, _dumper._indentLength);
        Expression Indent()
            => Expression.Call(_dumper, _miIndent);

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        Expression Unindent()
            => Expression.Call(_dumper, _miUnindent);

        //// _dumper._maxDepth--;
        Expression DecrementMaxDepth()
            => Expression.PostDecrementAssign(_maxDepth);

        //// _dumper._maxDepth++;
        Expression IncrementMaxDepth()
            => Expression.PostIncrementAssign(_maxDepth);

        ////_dumper.Writer.Write(
        ////    DumpFormat.CyclicalReference,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        Expression DumpSeenAlready()
            => Write(
                    DumpFormat.CyclicalReference,
                    Expression.Call(_miGetTypeName, _instanceType, _false),
                    Expression.Property(_instanceType, _piNamespace),
                    Expression.Property(_instanceType, _piAssemblyQualifiedName));

        // ============= Dumping types:

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        Expression DumpType(
            Expression type)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return Write(
                        DumpFormat.Type,
                        Expression.Call(_miGetTypeName, type, _false),
                        Expression.Property(type, _piNamespace),
                        Expression.Property(type, _piAssemblyQualifiedName));
        }

        ////_dumper.Writer.Write(
        ////    DumpFormat.SequenceType,
        ////    sequenceType.GetTypeName(),
        ////    sequenceType.Namespace,
        ////    sequenceType.AssemblyQualifiedName);
        Expression DumpSequenceType(
            Expression sequenceType)
        {
            Contract.Requires<ArgumentNullException>(sequenceType != null, nameof(sequenceType));

            return Write(
                        DumpFormat.SequenceType,
                        Expression.Call(_miGetTypeName, sequenceType, _false),
                        Expression.Property(sequenceType, _piNamespace),
                        Expression.Property(sequenceType, _piAssemblyQualifiedName));
        }

        Expression DumpSequenceTypeName(
            Expression sequence,
            Expression sequenceType)
        {
            Contract.Requires<ArgumentNullException>(sequence     != null, nameof(sequence));
            Contract.Requires<ArgumentNullException>(sequenceType != null, nameof(sequenceType));

            return Write(
                DumpFormat.SequenceTypeName,
                Expression.Call(_miGetTypeName, sequenceType, _false),
                Expression.Call(Expression.Property(sequence, _piCollectionCount), _miIntToString1, Expression.Constant(CultureInfo.InvariantCulture)));
        }

        // ==================== Dumping delegates:

        //// _dumper.Writer.Dumped((Delegate)Instance);
        Expression DumpedDelegate(
            Expression @delegate)
        {
            Contract.Requires<ArgumentNullException>(@delegate != null, nameof(@delegate));

            return Expression.Call(
                        _miDumpedDelegate,
                        _writer,
                        Expression.TypeAs(@delegate, typeof(Delegate)));
        }

        //// _dumper.Writer.Dumped((Delegate)_instance.Property);
        Expression DumpedDelegate(
            MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return DumpedDelegate(MemberValue(mi));
        }

        //// _dumper.Writer.Dumped((Delegate)Instance);
        Expression DumpedDelegate()
            => DumpedDelegate(_instance);

        // ==================== Dumping MemberInfo-s

        Expression DumpedMemberInfo(
            Expression mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Expression.Call(
                        _miDumpedMemberInfo,
                        _writer,
                        Expression.TypeAs(mi, typeof(MemberInfo)));
        }

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo(
            MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return DumpedMemberInfo(MemberValue(mi));
        }

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo()
            => DumpedMemberInfo(_instance);

        // ====================== Dumping basic values:

        Expression DumpedBasicValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(MemberValue(mi), typeof(object)),
                        Expression.Constant(dumpAttribute));
        }

        Expression DumpedBasicValue(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(_instance, typeof(object)),
                        Expression.Constant(dumpAttribute));
        }

        // ===================

        Expression CustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return dumpMethod == null
                        ? Expression.Call(_writer, _miWrite1, Expression.Call(MemberValue(mi), _miToString))
                        : dumpMethod.IsStatic
                            ? Expression.Call(_writer, _miWrite1, Expression.Call(dumpMethod, MemberValue(mi)))
                            : Expression.Call(_writer, _miWrite1, Expression.Call(MemberValue(mi), dumpMethod));
        }

        // ===============

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Diagnostics.DumpImplementation.DumpScript.Write(System.String)")]
        Expression DumpedDictionary(
            Expression dictionary,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dictionary != null, nameof(dictionary));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            //// dictionary.GetType()
            var dictionaryType = Expression.Call(dictionary, _miGetType);

            BeginScriptSegment();

            //// writer.Write(DumpFormat.SequenceTypeName, dictionaryType.GetTypeName(), dictionary.Count.ToString(CultureInfo.InvariantCulture));
            //// writer.Write(DumpFormat.SequenceType, dictionaryType.GetTypeName(), dictionaryType.Namespace, dictionaryType.AssemblyQualifiedName);
            AddDumpSequenceTypeName(dictionary, dictionaryType);
            AddDumpSequenceType(dictionaryType);

            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
            {
                //// return true;
                _script.Add(Expression.Constant(true));
                return Expression.Block(EndScriptSegment());
            }

            ParameterExpression kv;     // the current key-value item            
            ParameterExpression n;      // how many items left to be dumped?            
            ParameterExpression max;    // max items to dump
            ParameterExpression count;  // count of items

            kv    = Expression.Parameter(typeof(DictionaryEntry), nameof(kv));
            n     = Expression.Parameter(typeof(int), nameof(n));
            max   = Expression.Parameter(typeof(int), nameof(max));
            count = Expression.Parameter(typeof(int), nameof(count));

            var @break = Expression.Label();

            _script.Add
            (
                //// if (ReferenceEqual(dictionary,null)) return false; else {
                Expression.IfThenElse
                (
                    Expression.Call(_miReferenceEquals, dictionary, _null),
                    Expression.Assign(_tempBool, Expression.Constant(false)),
                    Expression.Block
                    (
                        //// var kv; var n=0; var max = dumpAttribute.GetMaxToDump(sequence.Count); n = 0; WriteLine(); Write("{"); Indent();
                        new[] { kv, n, max, count },
                        Expression.Assign(n, _zero),
                        Expression.Assign(count, Expression.Property(dictionary, _piCollectionCount)),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), count)),

                        WriteLine(),
                        Write("{"),
                        Indent(),

                        //// foreach (kv in dictionary)
                        ForEachInDictionary
                        (
                            kv,
                            dictionary,
                            Expression.Block
                            (
                                //// { Writer.WriteLine();
                                WriteLine(),

                                //// if (n++ >= max) {
                                Expression.IfThen
                                (
                                    Expression.GreaterThanOrEqual(Expression.PostDecrementAssign(n), max),
                                    Expression.Block
                                    (
                                        //// Writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                        Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), Expression.Convert(count, typeof(object))),
                                        //// break; }
                                        Expression.Break(@break)
                                    )
                                ),

                                //// Writer.Write("[");
                                Write("["),
                                //// _dumper.DumpObject(kv.Key);
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryKey), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _false),
                                // Writer.Write("] = ");
                                Write("] = "),

                                //// _dumper.DumpObject(kv.Value);
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryValue), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _false)
                            // }
                            ),
                            @break
                        ),

                        Unindent(),
                        WriteLine(),
                        Write("}"),
                        //// return true; }
                        Expression.Assign(_tempBool, Expression.Constant(true))
                    )
                )
            );
            _script.Add
            (
                Expression.IsTrue(_tempBool)
            );

            return Expression.Block(EndScriptSegment());
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        Expression DumpedDictionary(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return DumpedDictionary(
                        //// (IDictionary)_instance
                        Expression.TypeAs(_instance, typeof(IDictionary)),
                        dumpAttribute);
        }

        Expression DumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return DumpedDictionary(
                    //// (IDictionary)_instance.Property
                    Expression.TypeAs(MemberValue(mi), typeof(IDictionary)),
                    dumpAttribute);
        }

        // ==============================

        Expression DumpedCollection(
            Expression collection,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(collection    != null, nameof(collection));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            //// _instance.GetType()
            var collectionType = Expression.Call(collection, _miGetType);

            ////var elementsType = sequenceType.IsArray
            ////                        ? new Type[] { sequenceType.GetElementType() }
            ////                        : sequenceType.IsGenericType
            ////                            ? sequenceType.GetGenericArguments()
            ////                            : new Type[] { typeof(object) };
            var elementsType = Expression.Condition(
                                            Expression.Property(collectionType, _piIsArray),
                                            Expression.NewArrayInit(typeof(Type), Expression.Call(collectionType, _miGetElementType)),
                                            Expression.Condition(
                                                Expression.Property(collectionType, _piIsGenericType),
                                                Expression.Call(collectionType, _miGetGenericArguments),
                                                Expression.NewArrayInit(typeof(Type), Expression.Constant(typeof(object)))));

            ParameterExpression n;      // how many items left to be dumped?            
            ParameterExpression max;    // max items to dump
            ParameterExpression bytes;  // collection as byte[];
            ParameterExpression item;   // the iteration variable
            ParameterExpression count;  // count of items

            n     = Expression.Parameter(typeof(int), nameof(n));
            max   = Expression.Parameter(typeof(int), nameof(max));
            bytes = Expression.Parameter(typeof(byte[]), nameof(bytes));
            item  = Expression.Parameter(typeof(object), nameof(item));
            count = Expression.Parameter(typeof(int), nameof(count));

            var @break = Expression.Label();

            //// if (ReferenceEqual(collection,null)) Writer.Write("<null>"); else {
            return Expression.Block
            (
                Expression.IfThenElse
                (
                    Expression.Call(_miReferenceEquals, collection, _null),
                    Expression.Assign(_tempBool, Expression.Constant(false)),
                    Expression.Block
                    (
                        //// var max = dumpAttribute.GetMaxToDump(sequence.Count); var n = 0; var bytes = collection as byte[]; var count = collection.Count;
                        new[] { n, max, bytes, count },
                        Expression.Assign(n, _zero),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), Expression.Property(collection, _piCollectionCount))),
                        Expression.Assign(bytes, Expression.TypeAs(collection, typeof(byte[]))),
                        Expression.Assign(count, Expression.Property(collection, _piCollectionCount)),

                        //// if (!(sequenceType.IsArray || sequenceType.IsFromSystem())) WriteLine();
                        Expression.IfThen
                        (
                            Expression.Not
                            (
                                Expression.OrElse
                                (
                                    Expression.Property(collectionType, _piIsArray),
                                    Expression.Call(_miIsMatch, collectionType)
                                )
                            ),
                            WriteLine()
                        ),

                        ////writer.Write(
                        ////    DumpFormat.SequenceTypeName,
                        ////    sequenceType.IsArray
                        ////            ? elementsType[0].GetTypeName()
                        ////            : sequenceType.GetTypeName(),
                        ////    collection != null
                        ////            ? collection.Count.ToString(CultureInfo.InvariantCulture)
                        ////            : string.Empty);
                        Write
                        (
                            DumpFormat.SequenceTypeName,
                            Expression.Condition
                            (
                                Expression.Property(collectionType, _piIsArray),
                                Expression.Call(_miGetTypeName, Expression.ArrayIndex(elementsType, _zero), _false),
                                Expression.Call(_miGetTypeName, collectionType, _false)
                            ),
                            Expression.Condition
                            (
                                Expression.NotEqual(collection, _null),
                                Expression.Call(Expression.Property(collection, _piCollectionCount), _miIntToString1, Expression.Constant(CultureInfo.InvariantCulture)),
                                _empty
                            )
                        ),

                        ////if (bytes != null)
                        ////{
                        ////    // dump no more than max elements from the sequence:
                        ////    writer.Write(BitConverter.ToString(bytes, 0, max));
                        ////    if (max < bytes.Length)
                        ////        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                        ////}
                        Expression.IfThenElse
                        (
                            Expression.NotEqual(bytes, _null),
                            Expression.Block
                            (
                                Write(Expression.Call(_miBitConverterToString, bytes, _zero, max)),
                                Expression.IfThen
                                (
                                    Expression.LessThan(max, Expression.Property(bytes, _piArrayLength)),
                                    Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), Expression.Convert(count, typeof(object)))
                                )
                            ),
                            ////else {
                            Expression.Block
                            (
                                ////writer.Write(
                                ////    DumpFormat.SequenceType,
                                ////    sequenceType.GetTypeName(),
                                ////    sequenceType.Namespace,
                                ////    sequenceType.AssemblyQualifiedName);
                                Write
                                (
                                    DumpFormat.SequenceType,
                                    Expression.Call(_miGetTypeName, collectionType, _false),
                                    Expression.Property(collectionType, _piNamespace),
                                    Expression.Property(collectionType, _piAssemblyQualifiedName)
                                ),

                                ////if (dumpAttribute.RecurseDump!=ShouldDump.Skip) {
                                Expression.IfThen
                                (
                                    Expression.NotEqual
                                    (
                                        Expression.Property(Expression.Constant(dumpAttribute), _piRecurseDump),
                                        Expression.Constant(ShouldDump.Skip)
                                    ),

                                    ////indent();
                                    ////foreach (var item in sequence)
                                    ////{
                                    ////    writer.WriteLine();
                                    ////    if (n++ >= max)
                                    ////    {
                                    ////        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                    ////        break;
                                    ////    }
                                    ////    dumpObject(item);
                                    ////}
                                    ////unindent();
                                    Expression.Block
                                    (
                                        Indent(),
                                        ForEachInEnumerable
                                        (
                                            item,
                                            collection,
                                            Expression.Block
                                            (
                                                WriteLine(),
                                                Expression.IfThen
                                                (
                                                    Expression.GreaterThanOrEqual(Expression.PostIncrementAssign(n), max),
                                                    Expression.Block
                                                    (
                                                        Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), Expression.Convert(count, typeof(object))),
                                                        Expression.Break(@break)
                                                    )
                                                ),
                                                Expression.Call(_dumper, _miDumperDumpObject, item, Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _false)
                                            ),
                                            @break
                                        ),
                                        Unindent()
                                    )
                                )
                            )
                        //// }
                        ),
                        //// return true; }
                        Expression.Assign(_tempBool, Expression.Constant(true))
                    )
                ),
                Expression.IsTrue(_tempBool)
            );
        }

        Expression DumpedCollection(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return DumpedCollection(
                        //// (ICollection)_instance
                        Expression.TypeAs(_instance, typeof(ICollection)),
                        dumpAttribute);
        }

        Expression DumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return DumpedCollection(
                        //// (IDictionary)_instance.Property
                        Expression.TypeAs(MemberValue(mi), typeof(ICollection)),
                        dumpAttribute);
        }

        // ================================ dump the value of a property

        Expression DumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Expression.IfThen
                    (
                        Expression.Not
                        (
                            Expression.OrElse(
                                DumpedBasicValue(mi, dumpAttribute),
                                Expression.OrElse(
                                    DumpedDelegate(mi),
                                    Expression.OrElse(
                                        DumpedMemberInfo(mi),
                                        Expression.OrElse
                                        (
                                            DumpedDictionary(mi, dumpAttribute),
                                            DumpedCollection(mi, dumpAttribute)
                                        )
                                    )
                                )
                            )
                        ),
                        DumpObject(mi, null, !dumpAttribute.IsDefaultAttribute() ? dumpAttribute : null)
                    );
        }

        internal Expression DumpObject(
            MemberInfo mi,
            Type dumpMetadata,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Expression.Call(
                    _dumper,
                    _miDumperDumpObject,
                    Expression.Convert(MemberValue(mi), typeof(object)),
                    Expression.Constant(dumpMetadata, typeof(Type)),
                    Expression.Constant(dumpAttribute, typeof(DumpAttribute)),
                    _false);
        }
    }
}