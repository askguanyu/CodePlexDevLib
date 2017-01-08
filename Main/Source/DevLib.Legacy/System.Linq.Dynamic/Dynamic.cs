namespace System.Linq.Dynamic
{
    public static class DynamicQueryable
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
        {
            return (IQueryable<T>)Where((IQueryable)source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            System.Linq.Expressions.LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);
            return source.Provider.CreateQuery(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Where",
                    new Type[] { source.ElementType },
                    source.Expression, System.Linq.Expressions.Expression.Quote(lambda)));
        }

        public static IQueryable Select(this IQueryable source, string selector, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            System.Linq.Expressions.LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);
            return source.Provider.CreateQuery(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { source.ElementType, lambda.Body.Type },
                    source.Expression, System.Linq.Expressions.Expression.Quote(lambda)));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            return (IQueryable<T>)OrderBy((IQueryable)source, ordering, values);
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (ordering == null) throw new ArgumentNullException("ordering");
            System.Linq.Expressions.ParameterExpression[] parameters = new System.Linq.Expressions.ParameterExpression[] {
                System.Linq.Expressions.Expression.Parameter(source.ElementType, "") };
            ExpressionParser parser = new ExpressionParser(parameters, ordering, values);
            System.Collections.Generic.IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            System.Linq.Expressions.Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings)
            {
                queryExpr = System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), o.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, o.Selector.Type },
                    queryExpr, System.Linq.Expressions.Expression.Quote(System.Linq.Expressions.Expression.Lambda(o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            return source.Provider.CreateQuery(queryExpr);
        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, System.Linq.Expressions.Expression.Constant(count)));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, System.Linq.Expressions.Expression.Constant(count)));
        }

        public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            System.Linq.Expressions.LambdaExpression keyLambda = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
            System.Linq.Expressions.LambdaExpression elementLambda = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);
            return source.Provider.CreateQuery(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "GroupBy",
                    new Type[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression, System.Linq.Expressions.Expression.Quote(keyLambda), System.Linq.Expressions.Expression.Quote(elementLambda)));
        }

        public static bool Any(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (bool)source.Provider.Execute(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Any",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static int Count(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }
    }

    public abstract class DynamicClass
    {
        public override string ToString()
        {
            System.Reflection.PropertyInfo[] props = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{");
            for (int i = 0; i < props.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    public class DynamicProperty
    {
        private string name;
        private Type type;

        public DynamicProperty(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            this.name = name;
            this.type = type;
        }

        public string Name
        {
            get { return name; }
        }

        public Type Type
        {
            get { return type; }
        }
    }

    public static class DynamicExpression
    {
        public static System.Linq.Expressions.Expression Parse(Type resultType, string expression, params object[] values)
        {
            ExpressionParser parser = new ExpressionParser(null, expression, values);
            return parser.Parse(resultType);
        }

        public static System.Linq.Expressions.LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values)
        {
            return ParseLambda(new System.Linq.Expressions.ParameterExpression[] { System.Linq.Expressions.Expression.Parameter(itType, "") }, resultType, expression, values);
        }

        public static System.Linq.Expressions.LambdaExpression ParseLambda(System.Linq.Expressions.ParameterExpression[] parameters, Type resultType, string expression, params object[] values)
        {
            ExpressionParser parser = new ExpressionParser(parameters, expression, values);
            return System.Linq.Expressions.Expression.Lambda(parser.Parse(resultType), parameters);
        }

        public static System.Linq.Expressions.Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
        {
            return (System.Linq.Expressions.Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
        }

        public static Type CreateClass(params DynamicProperty[] properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(System.Collections.Generic.IEnumerable<DynamicProperty> properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }
    }

    internal class DynamicOrdering
    {
        public System.Linq.Expressions.Expression Selector;
        public bool Ascending;
    }

    internal class Signature : IEquatable<Signature>
    {
        public DynamicProperty[] properties;
        public int hashCode;

        public Signature(System.Collections.Generic.IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (DynamicProperty p in properties)
            {
                hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature ? Equals((Signature)obj) : false;
        }

        public bool Equals(Signature other)
        {
            if (properties.Length != other.properties.Length) return false;
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type) return false;
            }
            return true;
        }
    }

    internal class ClassFactory
    {
        public static readonly ClassFactory Instance = new ClassFactory();

        static ClassFactory()
        {
        }  // Trigger lazy initialization of static fields

        private System.Reflection.Emit.ModuleBuilder module;
        private System.Collections.Generic.Dictionary<Signature, Type> classes;
        private int classCount;
        private System.Threading.ReaderWriterLock rwLock;

        private ClassFactory()
        {
            System.Reflection.AssemblyName name = new System.Reflection.AssemblyName("DynamicClasses");
            System.Reflection.Emit.AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, System.Reflection.Emit.AssemblyBuilderAccess.Run);
#if ENABLE_LINQ_PARTIAL_TRUST
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            try
            {
                module = assembly.DefineDynamicModule("Module");
            }
            finally
            {
#if ENABLE_LINQ_PARTIAL_TRUST
                PermissionSet.RevertAssert();
#endif
            }
            classes = new System.Collections.Generic.Dictionary<Signature, Type>();
            rwLock = new System.Threading.ReaderWriterLock();
        }

        public Type GetDynamicClass(System.Collections.Generic.IEnumerable<DynamicProperty> properties)
        {
            rwLock.AcquireReaderLock(System.Threading.Timeout.Infinite);
            try
            {
                Signature signature = new Signature(properties);
                Type type;
                if (!classes.TryGetValue(signature, out type))
                {
                    type = CreateDynamicClass(signature.properties);
                    classes.Add(signature, type);
                }
                return type;
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        private Type CreateDynamicClass(DynamicProperty[] properties)
        {
            System.Threading.LockCookie cookie = rwLock.UpgradeToWriterLock(System.Threading.Timeout.Infinite);
            try
            {
                string typeName = "DynamicClass" + (classCount + 1);
#if ENABLE_LINQ_PARTIAL_TRUST
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
                try
                {
                    System.Reflection.Emit.TypeBuilder tb = this.module.DefineType(typeName, System.Reflection.TypeAttributes.Class |
                        System.Reflection.TypeAttributes.Public, typeof(DynamicClass));
                    System.Reflection.FieldInfo[] fields = GenerateProperties(tb, properties);
                    GenerateEquals(tb, fields);
                    GenerateGetHashCode(tb, fields);
                    Type result = tb.CreateType();
                    classCount++;
                    return result;
                }
                finally
                {
#if ENABLE_LINQ_PARTIAL_TRUST
                    PermissionSet.RevertAssert();
#endif
                }
            }
            finally
            {
                rwLock.DowngradeFromWriterLock(ref cookie);
            }
        }

        private System.Reflection.FieldInfo[] GenerateProperties(System.Reflection.Emit.TypeBuilder tb, DynamicProperty[] properties)
        {
            System.Reflection.FieldInfo[] fields = new System.Reflection.Emit.FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty dp = properties[i];
                System.Reflection.Emit.FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, System.Reflection.FieldAttributes.Private);
                System.Reflection.Emit.PropertyBuilder pb = tb.DefineProperty(dp.Name, System.Reflection.PropertyAttributes.HasDefault, dp.Type, null);
                System.Reflection.Emit.MethodBuilder mbGet = tb.DefineMethod("get_" + dp.Name,
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.HideBySig,
                    dp.Type, Type.EmptyTypes);
                System.Reflection.Emit.ILGenerator genGet = mbGet.GetILGenerator();
                genGet.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                genGet.Emit(System.Reflection.Emit.OpCodes.Ldfld, fb);
                genGet.Emit(System.Reflection.Emit.OpCodes.Ret);
                System.Reflection.Emit.MethodBuilder mbSet = tb.DefineMethod("set_" + dp.Name,
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.HideBySig,
                    null, new Type[] { dp.Type });
                System.Reflection.Emit.ILGenerator genSet = mbSet.GetILGenerator();
                genSet.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                genSet.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                genSet.Emit(System.Reflection.Emit.OpCodes.Stfld, fb);
                genSet.Emit(System.Reflection.Emit.OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }
            return fields;
        }

        private void GenerateEquals(System.Reflection.Emit.TypeBuilder tb, System.Reflection.FieldInfo[] fields)
        {
            System.Reflection.Emit.MethodBuilder mb = tb.DefineMethod("Equals",
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.ReuseSlot |
                System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.HideBySig,
                typeof(bool), new Type[] { typeof(object) });
            System.Reflection.Emit.ILGenerator gen = mb.GetILGenerator();
            System.Reflection.Emit.LocalBuilder other = gen.DeclareLocal(tb);
            System.Reflection.Emit.Label next = gen.DefineLabel();
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            gen.Emit(System.Reflection.Emit.OpCodes.Isinst, tb);
            gen.Emit(System.Reflection.Emit.OpCodes.Stloc, other);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldloc, other);
            gen.Emit(System.Reflection.Emit.OpCodes.Brtrue_S, next);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(System.Collections.Generic.EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(System.Reflection.Emit.OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldloc, other);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                gen.EmitCall(System.Reflection.Emit.OpCodes.Callvirt, ct.GetMethod("Equals", new Type[] { ft, ft }), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Brtrue_S, next);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
                gen.Emit(System.Reflection.Emit.OpCodes.Ret);
                gen.MarkLabel(next);
            }
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
        }

        private void GenerateGetHashCode(System.Reflection.Emit.TypeBuilder tb, System.Reflection.FieldInfo[] fields)
        {
            System.Reflection.Emit.MethodBuilder mb = tb.DefineMethod("GetHashCode",
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.ReuseSlot |
                System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.HideBySig,
                typeof(int), Type.EmptyTypes);
            System.Reflection.Emit.ILGenerator gen = mb.GetILGenerator();
            gen.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                Type ft = field.FieldType;
                Type ct = typeof(System.Collections.Generic.EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(System.Reflection.Emit.OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                gen.EmitCall(System.Reflection.Emit.OpCodes.Callvirt, ct.GetMethod("GetHashCode", new Type[] { ft }), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Xor);
            }
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
        }
    }

    public sealed class ParseException : Exception
    {
        private int position;

        public ParseException(string message, int position)
            : base(message)
        {
            this.position = position;
        }

        public int Position
        {
            get { return position; }
        }

        public override string ToString()
        {
            return string.Format(Res.ParseExceptionFormat, Message, position);
        }
    }

    internal class ExpressionParser
    {
        private struct Token
        {
            public TokenId id;
            public string text;
            public int pos;
        }

        private enum TokenId
        {
            Unknown,
            End,
            Identifier,
            StringLiteral,
            IntegerLiteral,
            RealLiteral,
            Exclamation,
            Percent,
            Amphersand,
            OpenParen,
            CloseParen,
            Asterisk,
            Plus,
            Comma,
            Minus,
            Dot,
            Slash,
            Colon,
            LessThan,
            Equal,
            GreaterThan,
            Question,
            OpenBracket,
            CloseBracket,
            Bar,
            ExclamationEqual,
            DoubleAmphersand,
            LessThanEqual,
            LessGreater,
            DoubleEqual,
            GreaterThanEqual,
            DoubleBar
        }

        private interface ILogicalSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);
        }

        private interface IArithmeticSignatures
        {
            void F(int x, int y);

            void F(uint x, uint y);

            void F(long x, long y);

            void F(ulong x, ulong y);

            void F(float x, float y);

            void F(double x, double y);

            void F(decimal x, decimal y);

            void F(int? x, int? y);

            void F(uint? x, uint? y);

            void F(long? x, long? y);

            void F(ulong? x, ulong? y);

            void F(float? x, float? y);

            void F(double? x, double? y);

            void F(decimal? x, decimal? y);
        }

        private interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);

            void F(char x, char y);

            void F(DateTime x, DateTime y);

            void F(TimeSpan x, TimeSpan y);

            void F(char? x, char? y);

            void F(DateTime? x, DateTime? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        private interface IEqualitySignatures : IRelationalSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);
        }

        private interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);

            void F(TimeSpan x, TimeSpan y);

            void F(DateTime? x, TimeSpan? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        private interface ISubtractSignatures : IAddSignatures
        {
            void F(DateTime x, DateTime y);

            void F(DateTime? x, DateTime? y);
        }

        private interface INegationSignatures
        {
            void F(int x);

            void F(long x);

            void F(float x);

            void F(double x);

            void F(decimal x);

            void F(int? x);

            void F(long? x);

            void F(float? x);

            void F(double? x);

            void F(decimal? x);
        }

        private interface INotSignatures
        {
            void F(bool x);

            void F(bool? x);
        }

        private interface IEnumerableSignatures
        {
            void Where(bool predicate);

            void Any();

            void Any(bool predicate);

            void All(bool predicate);

            void Count();

            void Count(bool predicate);

            void Min(object selector);

            void Max(object selector);

            void Sum(int selector);

            void Sum(int? selector);

            void Sum(long selector);

            void Sum(long? selector);

            void Sum(float selector);

            void Sum(float? selector);

            void Sum(double selector);

            void Sum(double? selector);

            void Sum(decimal selector);

            void Sum(decimal? selector);

            void Average(int selector);

            void Average(int? selector);

            void Average(long selector);

            void Average(long? selector);

            void Average(float selector);

            void Average(float? selector);

            void Average(double selector);

            void Average(double? selector);

            void Average(decimal selector);

            void Average(decimal? selector);
        }

        private static readonly Type[] predefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        private static readonly System.Linq.Expressions.Expression trueLiteral = System.Linq.Expressions.Expression.Constant(true);
        private static readonly System.Linq.Expressions.Expression falseLiteral = System.Linq.Expressions.Expression.Constant(false);
        private static readonly System.Linq.Expressions.Expression nullLiteral = System.Linq.Expressions.Expression.Constant(null);

        private static readonly string keywordIt = "it";
        private static readonly string keywordIif = "iif";
        private static readonly string keywordNew = "new";

        private static System.Collections.Generic.Dictionary<string, object> keywords;

        private System.Collections.Generic.Dictionary<string, object> symbols;
        private System.Collections.Generic.IDictionary<string, object> externals;
        private System.Collections.Generic.Dictionary<System.Linq.Expressions.Expression, string> literals;
        private System.Linq.Expressions.ParameterExpression it;
        private string text;
        private int textPos;
        private int textLen;
        private char ch;
        private Token token;

        public ExpressionParser(System.Linq.Expressions.ParameterExpression[] parameters, string expression, object[] values)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            if (keywords == null) keywords = CreateKeywords();
            symbols = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            literals = new System.Collections.Generic.Dictionary<System.Linq.Expressions.Expression, string>();
            if (parameters != null) ProcessParameters(parameters);
            if (values != null) ProcessValues(values);
            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
        }

        private void ProcessParameters(System.Linq.Expressions.ParameterExpression[] parameters)
        {
            foreach (System.Linq.Expressions.ParameterExpression pe in parameters)
                if (!String.IsNullOrEmpty(pe.Name))
                    AddSymbol(pe.Name, pe);
            if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
                it = parameters[0];
        }

        private void ProcessValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                object value = values[i];
                if (i == values.Length - 1 && value is System.Collections.Generic.IDictionary<string, object>)
                {
                    externals = (System.Collections.Generic.IDictionary<string, object>)value;
                }
                else
                {
                    AddSymbol("@" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), value);
                }
            }
        }

        private void AddSymbol(string name, object value)
        {
            if (symbols.ContainsKey(name))
                throw ParseError(Res.DuplicateIdentifier, name);
            symbols.Add(name, value);
        }

        public System.Linq.Expressions.Expression Parse(Type resultType)
        {
            int exprPos = token.pos;
            System.Linq.Expressions.Expression expr = ParseExpression();
            if (resultType != null)
                if ((expr = PromoteExpression(expr, resultType, true)) == null)
                    throw ParseError(exprPos, Res.ExpressionTypeMismatch, GetTypeName(resultType));
            ValidateToken(TokenId.End, Res.SyntaxError);
            return expr;
        }

#pragma warning disable 0219

        public System.Collections.Generic.IEnumerable<DynamicOrdering> ParseOrdering()
        {
            System.Collections.Generic.List<DynamicOrdering> orderings = new System.Collections.Generic.List<DynamicOrdering>();
            while (true)
            {
                System.Linq.Expressions.Expression expr = ParseExpression();
                bool ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
                {
                    NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
                {
                    NextToken();
                    ascending = false;
                }
                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.End, Res.SyntaxError);
            return orderings;
        }

#pragma warning restore 0219

        // ?: operator
        private System.Linq.Expressions.Expression ParseExpression()
        {
            int errorPos = token.pos;
            System.Linq.Expressions.Expression expr = ParseLogicalOr();
            if (token.id == TokenId.Question)
            {
                NextToken();
                System.Linq.Expressions.Expression expr1 = ParseExpression();
                ValidateToken(TokenId.Colon, Res.ColonExpected);
                NextToken();
                System.Linq.Expressions.Expression expr2 = ParseExpression();
                expr = GenerateConditional(expr, expr1, expr2, errorPos);
            }
            return expr;
        }

        // ||, or operator
        private System.Linq.Expressions.Expression ParseLogicalOr()
        {
            System.Linq.Expressions.Expression left = ParseLogicalAnd();
            while (token.id == TokenId.DoubleBar || TokenIdentifierIs("or"))
            {
                Token op = token;
                NextToken();
                System.Linq.Expressions.Expression right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = System.Linq.Expressions.Expression.OrElse(left, right);
            }
            return left;
        }

        // &&, and operator
        private System.Linq.Expressions.Expression ParseLogicalAnd()
        {
            System.Linq.Expressions.Expression left = ParseComparison();
            while (token.id == TokenId.DoubleAmphersand || TokenIdentifierIs("and"))
            {
                Token op = token;
                NextToken();
                System.Linq.Expressions.Expression right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = System.Linq.Expressions.Expression.AndAlso(left, right);
            }
            return left;
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        private System.Linq.Expressions.Expression ParseComparison()
        {
            System.Linq.Expressions.Expression left = ParseAdditive();
            while (token.id == TokenId.Equal || token.id == TokenId.DoubleEqual ||
                token.id == TokenId.ExclamationEqual || token.id == TokenId.LessGreater ||
                token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
                token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual)
            {
                Token op = token;
                NextToken();
                System.Linq.Expressions.Expression right = ParseAdditive();
                bool isEquality = op.id == TokenId.Equal || op.id == TokenId.DoubleEqual ||
                    op.id == TokenId.ExclamationEqual || op.id == TokenId.LessGreater;
                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type))
                        {
                            right = System.Linq.Expressions.Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type))
                        {
                            left = System.Linq.Expressions.Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        System.Linq.Expressions.Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null)
                        {
                            left = e;
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else
                {
                    CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
                        op.text, ref left, ref right, op.pos);
                }
                switch (op.id)
                {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);
                        break;

                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);
                        break;

                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);
                        break;

                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);
                        break;

                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);
                        break;

                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);
                        break;
                }
            }
            return left;
        }

        // +, -, & operators
        private System.Linq.Expressions.Expression ParseAdditive()
        {
            System.Linq.Expressions.Expression left = ParseMultiplicative();
            while (token.id == TokenId.Plus || token.id == TokenId.Minus ||
                token.id == TokenId.Amphersand)
            {
                Token op = token;
                NextToken();
                System.Linq.Expressions.Expression right = ParseMultiplicative();
                switch (op.id)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                            goto case TokenId.Amphersand;
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateAdd(left, right);
                        break;

                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateSubtract(left, right);
                        break;

                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);
                        break;
                }
            }
            return left;
        }

        // *, /, %, mod operators
        private System.Linq.Expressions.Expression ParseMultiplicative()
        {
            System.Linq.Expressions.Expression left = ParseUnary();
            while (token.id == TokenId.Asterisk || token.id == TokenId.Slash ||
                token.id == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                Token op = token;
                NextToken();
                System.Linq.Expressions.Expression right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);
                switch (op.id)
                {
                    case TokenId.Asterisk:
                        left = System.Linq.Expressions.Expression.Multiply(left, right);
                        break;

                    case TokenId.Slash:
                        left = System.Linq.Expressions.Expression.Divide(left, right);
                        break;

                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = System.Linq.Expressions.Expression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }

        // -, !, not unary operators
        private System.Linq.Expressions.Expression ParseUnary()
        {
            if (token.id == TokenId.Minus || token.id == TokenId.Exclamation ||
                TokenIdentifierIs("not"))
            {
                Token op = token;
                NextToken();
                if (op.id == TokenId.Minus && (token.id == TokenId.IntegerLiteral ||
                    token.id == TokenId.RealLiteral))
                {
                    token.text = "-" + token.text;
                    token.pos = op.pos;
                    return ParsePrimary();
                }
                System.Linq.Expressions.Expression expr = ParseUnary();
                if (op.id == TokenId.Minus)
                {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                    expr = System.Linq.Expressions.Expression.Negate(expr);
                }
                else
                {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                    expr = System.Linq.Expressions.Expression.Not(expr);
                }
                return expr;
            }
            return ParsePrimary();
        }

        private System.Linq.Expressions.Expression ParsePrimary()
        {
            System.Linq.Expressions.Expression expr = ParsePrimaryStart();
            while (true)
            {
                if (token.id == TokenId.Dot)
                {
                    NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (token.id == TokenId.OpenBracket)
                {
                    expr = ParseElementAccess(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private System.Linq.Expressions.Expression ParsePrimaryStart()
        {
            switch (token.id)
            {
                case TokenId.Identifier:
                    return ParseIdentifier();

                case TokenId.StringLiteral:
                    return ParseStringLiteral();

                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();

                case TokenId.RealLiteral:
                    return ParseRealLiteral();

                case TokenId.OpenParen:
                    return ParseParenExpression();

                default:
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        private System.Linq.Expressions.Expression ParseStringLiteral()
        {
            ValidateToken(TokenId.StringLiteral);
            char quote = token.text[0];
            string s = token.text.Substring(1, token.text.Length - 2);
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'')
            {
                if (s.Length != 1)
                    throw ParseError(Res.InvalidCharacterLiteral);
                NextToken();
                return CreateLiteral(s[0], s);
            }
            NextToken();
            return CreateLiteral(s, s);
        }

        private System.Linq.Expressions.Expression ParseIntegerLiteral()
        {
            ValidateToken(TokenId.IntegerLiteral);
            string text = token.text;
            if (text[0] != '-')
            {
                ulong value;
                if (!UInt64.TryParse(text, out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                NextToken();
                if (value <= (ulong)Int32.MaxValue) return CreateLiteral((int)value, text);
                if (value <= (ulong)UInt32.MaxValue) return CreateLiteral((uint)value, text);
                if (value <= (ulong)Int64.MaxValue) return CreateLiteral((long)value, text);
                return CreateLiteral(value, text);
            }
            else
            {
                long value;
                if (!Int64.TryParse(text, out value))
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                NextToken();
                if (value >= Int32.MinValue && value <= Int32.MaxValue)
                    return CreateLiteral((int)value, text);
                return CreateLiteral(value, text);
            }
        }

        private System.Linq.Expressions.Expression ParseRealLiteral()
        {
            ValidateToken(TokenId.RealLiteral);
            string text = token.text;
            object value = null;
            char last = text[text.Length - 1];
            if (last == 'F' || last == 'f')
            {
                float f;
                if (Single.TryParse(text.Substring(0, text.Length - 1), out f)) value = f;
            }
            else
            {
                double d;
                if (Double.TryParse(text, out d)) value = d;
            }
            if (value == null) throw ParseError(Res.InvalidRealLiteral, text);
            NextToken();
            return CreateLiteral(value, text);
        }

        private System.Linq.Expressions.Expression CreateLiteral(object value, string text)
        {
            System.Linq.Expressions.ConstantExpression expr = System.Linq.Expressions.Expression.Constant(value);
            literals.Add(expr, text);
            return expr;
        }

        private System.Linq.Expressions.Expression ParseParenExpression()
        {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            System.Linq.Expressions.Expression e = ParseExpression();
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            NextToken();
            return e;
        }

        private System.Linq.Expressions.Expression ParseIdentifier()
        {
            ValidateToken(TokenId.Identifier);
            object value;
            if (keywords.TryGetValue(token.text, out value))
            {
                if (value is Type) return ParseTypeAccess((Type)value);
                if (value == (object)keywordIt) return ParseIt();
                if (value == (object)keywordIif) return ParseIif();
                if (value == (object)keywordNew) return ParseNew();
                NextToken();
                return (System.Linq.Expressions.Expression)value;
            }
            if (symbols.TryGetValue(token.text, out value) ||
                externals != null && externals.TryGetValue(token.text, out value))
            {
                System.Linq.Expressions.Expression expr = value as System.Linq.Expressions.Expression;
                if (expr == null)
                {
                    expr = System.Linq.Expressions.Expression.Constant(value);
                }
                else
                {
                    System.Linq.Expressions.LambdaExpression lambda = expr as System.Linq.Expressions.LambdaExpression;
                    if (lambda != null) return ParseLambdaInvocation(lambda);
                }
                NextToken();
                return expr;
            }
            if (it != null) return ParseMemberAccess(null, it);
            throw ParseError(Res.UnknownIdentifier, token.text);
        }

        private System.Linq.Expressions.Expression ParseIt()
        {
            if (it == null)
                throw ParseError(Res.NoItInScope);
            NextToken();
            return it;
        }

        private System.Linq.Expressions.Expression ParseIif()
        {
            int errorPos = token.pos;
            NextToken();
            System.Linq.Expressions.Expression[] args = ParseArgumentList();
            if (args.Length != 3)
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        private System.Linq.Expressions.Expression GenerateConditional(System.Linq.Expressions.Expression test, System.Linq.Expressions.Expression expr1, System.Linq.Expressions.Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
                throw ParseError(errorPos, Res.FirstExprMustBeBool);
            if (expr1.Type != expr2.Type)
            {
                System.Linq.Expressions.Expression expr1as2 = expr2 != nullLiteral ? PromoteExpression(expr1, expr2.Type, true) : null;
                System.Linq.Expressions.Expression expr2as1 = expr1 != nullLiteral ? PromoteExpression(expr2, expr1.Type, true) : null;
                if (expr1as2 != null && expr2as1 == null)
                {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null)
                {
                    expr2 = expr2as1;
                }
                else
                {
                    string type1 = expr1 != nullLiteral ? expr1.Type.Name : "null";
                    string type2 = expr2 != nullLiteral ? expr2.Type.Name : "null";
                    if (expr1as2 != null && expr2as1 != null)
                        throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }
            return System.Linq.Expressions.Expression.Condition(test, expr1, expr2);
        }

        private System.Linq.Expressions.Expression ParseNew()
        {
            NextToken();
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            System.Collections.Generic.List<DynamicProperty> properties = new System.Collections.Generic.List<DynamicProperty>();
            System.Collections.Generic.List<System.Linq.Expressions.Expression> expressions = new System.Collections.Generic.List<System.Linq.Expressions.Expression>();
            while (true)
            {
                int exprPos = token.pos;
                System.Linq.Expressions.Expression expr = ParseExpression();
                string propName;
                if (TokenIdentifierIs("as"))
                {
                    NextToken();
                    propName = GetIdentifier();
                    NextToken();
                }
                else
                {
                    System.Linq.Expressions.MemberExpression me = expr as System.Linq.Expressions.MemberExpression;
                    if (me == null) throw ParseError(exprPos, Res.MissingAsClause);
                    propName = me.Member.Name;
                }
                expressions.Add(expr);
                properties.Add(new DynamicProperty(propName, expr.Type));
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            Type type = DynamicExpression.CreateClass(properties);
            System.Linq.Expressions.MemberBinding[] bindings = new System.Linq.Expressions.MemberBinding[properties.Count];
            for (int i = 0; i < bindings.Length; i++)
                bindings[i] = System.Linq.Expressions.Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            return System.Linq.Expressions.Expression.MemberInit(System.Linq.Expressions.Expression.New(type), bindings);
        }

        private System.Linq.Expressions.Expression ParseLambdaInvocation(System.Linq.Expressions.LambdaExpression lambda)
        {
            int errorPos = token.pos;
            NextToken();
            System.Linq.Expressions.Expression[] args = ParseArgumentList();
            System.Reflection.MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
                throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
            return System.Linq.Expressions.Expression.Invoke(lambda, args);
        }

        private System.Linq.Expressions.Expression ParseTypeAccess(Type type)
        {
            int errorPos = token.pos;
            NextToken();
            if (token.id == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                    throw ParseError(errorPos, Res.TypeHasNoNullableForm, GetTypeName(type));
                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }
            if (token.id == TokenId.OpenParen)
            {
                System.Linq.Expressions.Expression[] args = ParseArgumentList();
                System.Reflection.MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                            return GenerateConversion(args[0], type, errorPos);
                        throw ParseError(errorPos, Res.NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return System.Linq.Expressions.Expression.New((System.Reflection.ConstructorInfo)method, args);

                    default:
                        throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }
            ValidateToken(TokenId.Dot, Res.DotOrOpenParenExpected);
            NextToken();
            return ParseMemberAccess(type, null);
        }

        private System.Linq.Expressions.Expression GenerateConversion(System.Linq.Expressions.Expression expr, Type type, int errorPos)
        {
            Type exprType = expr.Type;
            if (exprType == type) return expr;
            if (exprType.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                    return System.Linq.Expressions.Expression.Convert(expr, type);
                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    (IsNumericType(type)) || IsEnumType(type))
                    return System.Linq.Expressions.Expression.ConvertChecked(expr, type);
            }
            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
                return System.Linq.Expressions.Expression.Convert(expr, type);
            throw ParseError(errorPos, Res.CannotConvertValue,
                GetTypeName(exprType), GetTypeName(type));
        }

        private System.Linq.Expressions.Expression ParseMemberAccess(Type type, System.Linq.Expressions.Expression instance)
        {
            if (instance != null) type = instance.Type;
            int errorPos = token.pos;
            string id = GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen)
            {
                if (instance != null && type != typeof(string))
                {
                    Type enumerableType = FindGenericType(typeof(System.Collections.Generic.IEnumerable<>), type);
                    if (enumerableType != null)
                    {
                        Type elementType = enumerableType.GetGenericArguments()[0];
                        return ParseAggregate(instance, elementType, id, errorPos);
                    }
                }
                System.Linq.Expressions.Expression[] args = ParseArgumentList();
                System.Reflection.MethodBase mb;
                switch (FindMethod(type, id, instance == null, args, out mb))
                {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableMethod,
                            id, GetTypeName(type));
                    case 1:
                        System.Reflection.MethodInfo method = (System.Reflection.MethodInfo)mb;
                        if (!IsPredefinedType(method.DeclaringType))
                            throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                        if (method.ReturnType == typeof(void))
                            throw ParseError(errorPos, Res.MethodIsVoid,
                                id, GetTypeName(method.DeclaringType));
                        return System.Linq.Expressions.Expression.Call(instance, (System.Reflection.MethodInfo)method, args);

                    default:
                        throw ParseError(errorPos, Res.AmbiguousMethodInvocation,
                            id, GetTypeName(type));
                }
            }
            else
            {
                System.Reflection.MemberInfo member = FindPropertyOrField(type, id, instance == null);
                if (member == null)
                    throw ParseError(errorPos, Res.UnknownPropertyOrField,
                        id, GetTypeName(type));
                return member is System.Reflection.PropertyInfo ?
                    System.Linq.Expressions.Expression.Property(instance, (System.Reflection.PropertyInfo)member) :
                    System.Linq.Expressions.Expression.Field(instance, (System.Reflection.FieldInfo)member);
            }
        }

        private static Type FindGenericType(Type generic, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
                if (generic.IsInterface)
                {
                    foreach (Type intfType in type.GetInterfaces())
                    {
                        Type found = FindGenericType(generic, intfType);
                        if (found != null) return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        private System.Linq.Expressions.Expression ParseAggregate(System.Linq.Expressions.Expression instance, Type elementType, string methodName, int errorPos)
        {
            System.Linq.Expressions.ParameterExpression outerIt = it;
            System.Linq.Expressions.ParameterExpression innerIt = System.Linq.Expressions.Expression.Parameter(elementType, "");
            it = innerIt;
            System.Linq.Expressions.Expression[] args = ParseArgumentList();
            it = outerIt;
            System.Reflection.MethodBase signature;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
                throw ParseError(errorPos, Res.NoApplicableAggregate, methodName);
            Type[] typeArgs;
            if (signature.Name == "Min" || signature.Name == "Max")
            {
                typeArgs = new Type[] { elementType, args[0].Type };
            }
            else
            {
                typeArgs = new Type[] { elementType };
            }
            if (args.Length == 0)
            {
                args = new System.Linq.Expressions.Expression[] { instance };
            }
            else
            {
                args = new System.Linq.Expressions.Expression[] { instance, System.Linq.Expressions.Expression.Lambda(args[0], innerIt) };
            }
            return System.Linq.Expressions.Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
        }

        private System.Linq.Expressions.Expression[] ParseArgumentList()
        {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            System.Linq.Expressions.Expression[] args = token.id != TokenId.CloseParen ? ParseArguments() : new System.Linq.Expressions.Expression[0];
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            return args;
        }

        private System.Linq.Expressions.Expression[] ParseArguments()
        {
            System.Collections.Generic.List<System.Linq.Expressions.Expression> argList = new System.Collections.Generic.List<System.Linq.Expressions.Expression>();
            while (true)
            {
                argList.Add(ParseExpression());
                if (token.id != TokenId.Comma) break;
                NextToken();
            }
            return argList.ToArray();
        }

        private System.Linq.Expressions.Expression ParseElementAccess(System.Linq.Expressions.Expression expr)
        {
            int errorPos = token.pos;
            ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
            NextToken();
            System.Linq.Expressions.Expression[] args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                    throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
                System.Linq.Expressions.Expression index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                    throw ParseError(errorPos, Res.InvalidIndex);
                return System.Linq.Expressions.Expression.ArrayIndex(expr, index);
            }
            else
            {
                System.Reflection.MethodBase mb;
                switch (FindIndexer(expr.Type, args, out mb))
                {
                    case 0:
                        throw ParseError(errorPos, Res.NoApplicableIndexer,
                            GetTypeName(expr.Type));
                    case 1:
                        return System.Linq.Expressions.Expression.Call(expr, (System.Reflection.MethodInfo)mb, args);

                    default:
                        throw ParseError(errorPos, Res.AmbiguousIndexerInvocation,
                            GetTypeName(expr.Type));
                }
            }
        }

        private static bool IsPredefinedType(Type type)
        {
            foreach (Type t in predefinedTypes) if (t == type) return true;
            return false;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

        private static string GetTypeName(Type type)
        {
            Type baseType = GetNonNullableType(type);
            string s = baseType.Name;
            if (type != baseType) s += '?';
            return s;
        }

        private static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        private static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        private static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        private static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum) return 0;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;

                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;

                default:
                    return 0;
            }
        }

        private static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        private void CheckAndPromoteOperand(Type signatures, string opName, ref System.Linq.Expressions.Expression expr, int errorPos)
        {
            System.Linq.Expressions.Expression[] args = new System.Linq.Expressions.Expression[] { expr };
            System.Reflection.MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw ParseError(errorPos, Res.IncompatibleOperand,
                    opName, GetTypeName(args[0].Type));
            expr = args[0];
        }

        private void CheckAndPromoteOperands(Type signatures, string opName, ref System.Linq.Expressions.Expression left, ref System.Linq.Expressions.Expression right, int errorPos)
        {
            System.Linq.Expressions.Expression[] args = new System.Linq.Expressions.Expression[] { left, right };
            System.Reflection.MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            left = args[0];
            right = args[1];
        }

        private Exception IncompatibleOperandsError(string opName, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, int pos)
        {
            return ParseError(pos, Res.IncompatibleOperands,
                opName, GetTypeName(left.Type), GetTypeName(right.Type));
        }

        private System.Reflection.MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly |
                (staticAccess ? System.Reflection.BindingFlags.Static : System.Reflection.BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                System.Reflection.MemberInfo[] members = t.FindMembers(System.Reflection.MemberTypes.Property | System.Reflection.MemberTypes.Field,
                    flags, Type.FilterNameIgnoreCase, memberName);
                if (members.Length != 0) return members[0];
            }
            return null;
        }

        private int FindMethod(Type type, string methodName, bool staticAccess, System.Linq.Expressions.Expression[] args, out System.Reflection.MethodBase method)
        {
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly |
                (staticAccess ? System.Reflection.BindingFlags.Static : System.Reflection.BindingFlags.Instance);
            foreach (Type t in SelfAndBaseTypes(type))
            {
                System.Reflection.MemberInfo[] members = t.FindMembers(System.Reflection.MemberTypes.Method,
                    flags, Type.FilterNameIgnoreCase, methodName);
                int count = FindBestMethod(members.Cast<System.Reflection.MethodBase>(), args, out method);
                if (count != 0) return count;
            }
            method = null;
            return 0;
        }

        private int FindIndexer(Type type, System.Linq.Expressions.Expression[] args, out System.Reflection.MethodBase method)
        {
            foreach (Type t in SelfAndBaseTypes(type))
            {
                System.Reflection.MemberInfo[] members = t.GetDefaultMembers();
                if (members.Length != 0)
                {
                    System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> methods = members.
                        OfType<System.Reflection.PropertyInfo>().
                        Select(p => (System.Reflection.MethodBase)p.GetGetMethod()).
                        Where(m => m != null);
                    int count = FindBestMethod(methods, args, out method);
                    if (count != 0) return count;
                }
            }
            method = null;
            return 0;
        }

        private static System.Collections.Generic.IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (type.IsInterface)
            {
                System.Collections.Generic.List<Type> types = new System.Collections.Generic.List<Type>();
                AddInterface(types, type);
                return types;
            }
            return SelfAndBaseClasses(type);
        }

        private static System.Collections.Generic.IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        private static void AddInterface(System.Collections.Generic.List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
            }
        }

        private class MethodData
        {
            public System.Reflection.MethodBase MethodBase;
            public System.Reflection.ParameterInfo[] Parameters;
            public System.Linq.Expressions.Expression[] Args;
        }

        private int FindBestMethod(System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> methods, System.Linq.Expressions.Expression[] args, out System.Reflection.MethodBase method)
        {
            MethodData[] applicable = methods.
                Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
                Where(m => IsApplicable(m, args)).
                ToArray();
            if (applicable.Length > 1)
            {
                applicable = applicable.
                    Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
                    ToArray();
            }
            if (applicable.Length == 1)
            {
                MethodData md = applicable[0];
                for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
                method = md.MethodBase;
            }
            else
            {
                method = null;
            }
            return applicable.Length;
        }

        private bool IsApplicable(MethodData method, System.Linq.Expressions.Expression[] args)
        {
            if (method.Parameters.Length != args.Length) return false;
            System.Linq.Expressions.Expression[] promotedArgs = new System.Linq.Expressions.Expression[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                System.Reflection.ParameterInfo pi = method.Parameters[i];
                if (pi.IsOut) return false;
                System.Linq.Expressions.Expression promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null) return false;
                promotedArgs[i] = promoted;
            }
            method.Args = promotedArgs;
            return true;
        }

        private System.Linq.Expressions.Expression PromoteExpression(System.Linq.Expressions.Expression expr, Type type, bool exact)
        {
            if (expr.Type == type) return expr;
            if (expr is System.Linq.Expressions.ConstantExpression)
            {
                System.Linq.Expressions.ConstantExpression ce = (System.Linq.Expressions.ConstantExpression)expr;
                if (ce == nullLiteral)
                {
                    if (!type.IsValueType || IsNullableType(type))
                        return System.Linq.Expressions.Expression.Constant(null, type);
                }
                else
                {
                    string text;
                    if (literals.TryGetValue(ce, out text))
                    {
                        Type target = GetNonNullableType(type);
                        Object value = null;
                        switch (Type.GetTypeCode(ce.Type))
                        {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                value = ParseNumber(text, target);
                                break;

                            case TypeCode.Double:
                                if (target == typeof(decimal)) value = ParseNumber(text, target);
                                break;

                            case TypeCode.String:
                                value = ParseEnum(text, target);
                                break;
                        }
                        if (value != null)
                            return System.Linq.Expressions.Expression.Constant(value, type);
                    }
                }
            }
            if (IsCompatibleWith(expr.Type, type))
            {
                if (type.IsValueType || exact) return System.Linq.Expressions.Expression.Convert(expr, type);
                return expr;
            }
            return null;
        }

        private static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb)) return sb;
                    break;

                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b)) return b;
                    break;

                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s)) return s;
                    break;

                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us)) return us;
                    break;

                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i)) return i;
                    break;

                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui)) return ui;
                    break;

                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l)) return l;
                    break;

                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul)) return ul;
                    break;

                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f)) return f;
                    break;

                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d)) return d;
                    break;

                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e)) return e;
                    break;
            }
            return null;
        }

        private static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                System.Reflection.MemberInfo[] memberInfos = type.FindMembers(System.Reflection.MemberTypes.Field,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static,
                    Type.FilterNameIgnoreCase, name);
                if (memberInfos.Length != 0) return ((System.Reflection.FieldInfo)memberInfos[0]).GetValue(null);
            }
            return null;
        }

        private static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target) return true;
            if (!target.IsValueType) return target.IsAssignableFrom(source);
            Type st = GetNonNullableType(source);
            Type tt = GetNonNullableType(target);
            if (st != source && tt == target) return false;
            TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            switch (sc)
            {
                case TypeCode.SByte:
                    switch (tc)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Byte:
                    switch (tc)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int16:
                    switch (tc)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.UInt16:
                    switch (tc)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int32:
                    switch (tc)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.UInt32:
                    switch (tc)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int64:
                    switch (tc)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.UInt64:
                    switch (tc)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Single:
                    switch (tc)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;

                default:
                    if (st == tt) return true;
                    break;
            }
            return false;
        }

        private static bool IsBetterThan(System.Linq.Expressions.Expression[] args, MethodData m1, MethodData m2)
        {
            bool better = false;
            for (int i = 0; i < args.Length; i++)
            {
                int c = CompareConversions(args[i].Type,
                    m1.Parameters[i].ParameterType,
                    m2.Parameters[i].ParameterType);
                if (c < 0) return false;
                if (c > 0) better = true;
            }
            return better;
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        private static int CompareConversions(Type s, Type t1, Type t2)
        {
            if (t1 == t2) return 0;
            if (s == t1) return 1;
            if (s == t2) return -1;
            bool t1t2 = IsCompatibleWith(t1, t2);
            bool t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1) return 1;
            if (t2t1 && !t1t2) return -1;
            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
            return 0;
        }

        private System.Linq.Expressions.Expression GenerateEqual(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return System.Linq.Expressions.Expression.Equal(left, right);
        }

        private System.Linq.Expressions.Expression GenerateNotEqual(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return System.Linq.Expressions.Expression.NotEqual(left, right);
        }

        private System.Linq.Expressions.Expression GenerateGreaterThan(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            if (left.Type == typeof(string))
            {
                return System.Linq.Expressions.Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    System.Linq.Expressions.Expression.Constant(0)
                );
            }
            return System.Linq.Expressions.Expression.GreaterThan(left, right);
        }

        private System.Linq.Expressions.Expression GenerateGreaterThanEqual(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            if (left.Type == typeof(string))
            {
                return System.Linq.Expressions.Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    System.Linq.Expressions.Expression.Constant(0)
                );
            }
            return System.Linq.Expressions.Expression.GreaterThanOrEqual(left, right);
        }

        private System.Linq.Expressions.Expression GenerateLessThan(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            if (left.Type == typeof(string))
            {
                return System.Linq.Expressions.Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    System.Linq.Expressions.Expression.Constant(0)
                );
            }
            return System.Linq.Expressions.Expression.LessThan(left, right);
        }

        private System.Linq.Expressions.Expression GenerateLessThanEqual(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            if (left.Type == typeof(string))
            {
                return System.Linq.Expressions.Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    System.Linq.Expressions.Expression.Constant(0)
                );
            }
            return System.Linq.Expressions.Expression.LessThanOrEqual(left, right);
        }

        private System.Linq.Expressions.Expression GenerateAdd(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return GenerateStaticMethodCall("Concat", left, right);
            }
            return System.Linq.Expressions.Expression.Add(left, right);
        }

        private System.Linq.Expressions.Expression GenerateSubtract(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return System.Linq.Expressions.Expression.Subtract(left, right);
        }

        private System.Linq.Expressions.Expression GenerateStringConcat(System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return System.Linq.Expressions.Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        private System.Reflection.MethodInfo GetStaticMethod(string methodName, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        private System.Linq.Expressions.Expression GenerateStaticMethodCall(string methodName, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right)
        {
            return System.Linq.Expressions.Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        private void SetTextPos(int pos)
        {
            textPos = pos;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        private void NextChar()
        {
            if (textPos < textLen) textPos++;
            ch = textPos < textLen ? text[textPos] : '\0';
        }

        private void NextToken()
        {
            while (Char.IsWhiteSpace(ch)) NextChar();
            TokenId t;
            int tokenPos = textPos;
            switch (ch)
            {
                case '!':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else
                    {
                        t = TokenId.Exclamation;
                    }
                    break;

                case '%':
                    NextChar();
                    t = TokenId.Percent;
                    break;

                case '&':
                    NextChar();
                    if (ch == '&')
                    {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else
                    {
                        t = TokenId.Amphersand;
                    }
                    break;

                case '(':
                    NextChar();
                    t = TokenId.OpenParen;
                    break;

                case ')':
                    NextChar();
                    t = TokenId.CloseParen;
                    break;

                case '*':
                    NextChar();
                    t = TokenId.Asterisk;
                    break;

                case '+':
                    NextChar();
                    t = TokenId.Plus;
                    break;

                case ',':
                    NextChar();
                    t = TokenId.Comma;
                    break;

                case '-':
                    NextChar();
                    t = TokenId.Minus;
                    break;

                case '.':
                    NextChar();
                    t = TokenId.Dot;
                    break;

                case '/':
                    NextChar();
                    t = TokenId.Slash;
                    break;

                case ':':
                    NextChar();
                    t = TokenId.Colon;
                    break;

                case '<':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.LessThanEqual;
                    }
                    else if (ch == '>')
                    {
                        NextChar();
                        t = TokenId.LessGreater;
                    }
                    else
                    {
                        t = TokenId.LessThan;
                    }
                    break;

                case '=':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else
                    {
                        t = TokenId.Equal;
                    }
                    break;

                case '>':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.GreaterThanEqual;
                    }
                    else
                    {
                        t = TokenId.GreaterThan;
                    }
                    break;

                case '?':
                    NextChar();
                    t = TokenId.Question;
                    break;

                case '[':
                    NextChar();
                    t = TokenId.OpenBracket;
                    break;

                case ']':
                    NextChar();
                    t = TokenId.CloseBracket;
                    break;

                case '|':
                    NextChar();
                    if (ch == '|')
                    {
                        NextChar();
                        t = TokenId.DoubleBar;
                    }
                    else
                    {
                        t = TokenId.Bar;
                    }
                    break;

                case '"':
                case '\'':
                    char quote = ch;
                    do
                    {
                        NextChar();
                        while (textPos < textLen && ch != quote) NextChar();
                        if (textPos == textLen)
                            throw ParseError(textPos, Res.UnterminatedStringLiteral);
                        NextChar();
                    } while (ch == quote);
                    t = TokenId.StringLiteral;
                    break;

                default:
                    if (Char.IsLetter(ch) || ch == '@' || ch == '_')
                    {
                        do
                        {
                            NextChar();
                        } while (Char.IsLetterOrDigit(ch) || ch == '_');
                        t = TokenId.Identifier;
                        break;
                    }
                    if (Char.IsDigit(ch))
                    {
                        t = TokenId.IntegerLiteral;
                        do
                        {
                            NextChar();
                        } while (Char.IsDigit(ch));
                        if (ch == '.')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'E' || ch == 'e')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-') NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(ch));
                        }
                        if (ch == 'F' || ch == 'f') NextChar();
                        break;
                    }
                    if (textPos == textLen)
                    {
                        t = TokenId.End;
                        break;
                    }
                    throw ParseError(textPos, Res.InvalidCharacter, ch);
            }
            token.id = t;
            token.text = text.Substring(tokenPos, textPos - tokenPos);
            token.pos = tokenPos;
        }

        private bool TokenIdentifierIs(string id)
        {
            return token.id == TokenId.Identifier && String.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
        }

        private string GetIdentifier()
        {
            ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
            string id = token.text;
            if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
            return id;
        }

        private void ValidateDigit()
        {
            if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (token.id != t) throw ParseError(errorMessage);
        }

        private void ValidateToken(TokenId t)
        {
            if (token.id != t) throw ParseError(Res.SyntaxError);
        }

        private Exception ParseError(string format, params object[] args)
        {
            return ParseError(token.pos, format, args);
        }

        private Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
        }

        private static System.Collections.Generic.Dictionary<string, object> CreateKeywords()
        {
            System.Collections.Generic.Dictionary<string, object> d = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            d.Add("true", trueLiteral);
            d.Add("false", falseLiteral);
            d.Add("null", nullLiteral);
            d.Add(keywordIt, keywordIt);
            d.Add(keywordIif, keywordIif);
            d.Add(keywordNew, keywordNew);
            foreach (Type type in predefinedTypes) d.Add(type.Name, type);
            return d;
        }
    }

    internal static class Res
    {
        public const string DuplicateIdentifier = "The identifier '{0}' was defined more than once";
        public const string ExpressionTypeMismatch = "Expression of type '{0}' expected";
        public const string ExpressionExpected = "Expression expected";
        public const string InvalidCharacterLiteral = "Character literal must contain exactly one character";
        public const string InvalidIntegerLiteral = "Invalid integer literal '{0}'";
        public const string InvalidRealLiteral = "Invalid real literal '{0}'";
        public const string UnknownIdentifier = "Unknown identifier '{0}'";
        public const string NoItInScope = "No 'it' is in scope";
        public const string IifRequiresThreeArgs = "The 'iif' function requires three arguments";
        public const string FirstExprMustBeBool = "The first expression must be of type 'Boolean'";
        public const string BothTypesConvertToOther = "Both of the types '{0}' and '{1}' convert to the other";
        public const string NeitherTypeConvertsToOther = "Neither of the types '{0}' and '{1}' converts to the other";
        public const string MissingAsClause = "Expression is missing an 'as' clause";
        public const string ArgsIncompatibleWithLambda = "Argument list incompatible with lambda expression";
        public const string TypeHasNoNullableForm = "Type '{0}' has no nullable form";
        public const string NoMatchingConstructor = "No matching constructor in type '{0}'";
        public const string AmbiguousConstructorInvocation = "Ambiguous invocation of '{0}' constructor";
        public const string CannotConvertValue = "A value of type '{0}' cannot be converted to type '{1}'";
        public const string NoApplicableMethod = "No applicable method '{0}' exists in type '{1}'";
        public const string MethodsAreInaccessible = "Methods on type '{0}' are not accessible";
        public const string MethodIsVoid = "Method '{0}' in type '{1}' does not return a value";
        public const string AmbiguousMethodInvocation = "Ambiguous invocation of method '{0}' in type '{1}'";
        public const string UnknownPropertyOrField = "No property or field '{0}' exists in type '{1}'";
        public const string NoApplicableAggregate = "No applicable aggregate method '{0}' exists";
        public const string CannotIndexMultiDimArray = "Indexing of multi-dimensional arrays is not supported";
        public const string InvalidIndex = "Array index must be an integer expression";
        public const string NoApplicableIndexer = "No applicable indexer exists in type '{0}'";
        public const string AmbiguousIndexerInvocation = "Ambiguous invocation of indexer in type '{0}'";
        public const string IncompatibleOperand = "Operator '{0}' incompatible with operand type '{1}'";
        public const string IncompatibleOperands = "Operator '{0}' incompatible with operand types '{1}' and '{2}'";
        public const string UnterminatedStringLiteral = "Unterminated string literal";
        public const string InvalidCharacter = "Syntax error '{0}'";
        public const string DigitExpected = "Digit expected";
        public const string SyntaxError = "Syntax error";
        public const string TokenExpected = "{0} expected";
        public const string ParseExceptionFormat = "{0} (at index {1})";
        public const string ColonExpected = "':' expected";
        public const string OpenParenExpected = "'(' expected";
        public const string CloseParenOrOperatorExpected = "')' or operator expected";
        public const string CloseParenOrCommaExpected = "')' or ',' expected";
        public const string DotOrOpenParenExpected = "'.' or '(' expected";
        public const string OpenBracketExpected = "'[' expected";
        public const string CloseBracketOrCommaExpected = "']' or ',' expected";
        public const string IdentifierExpected = "Identifier expected";
    } // public static class DynamicQueryable
} // namespace System.Linq.Dynamic
