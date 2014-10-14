﻿/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressiveAnnotations
{
    internal static class Helper
    {
        public static void MakeTypesCompatible(Expression e1, Expression e2, out Expression oute1, out Expression oute2)
        {
            oute1 = e1;
            oute2 = e2;

            // promote numeric values to double - do all computations with higher precision (to be compatible with JavaScript, e.g. notation 1/2, should give 0.5 double not 0 int)
            if (oute1.Type != typeof(double) && oute1.Type != typeof(double?) && oute1.Type.IsNumeric())
                oute1 = oute1.Type.IsNullable()
                    ? Expression.Convert(oute1, typeof (double?))
                    : Expression.Convert(oute1, typeof (double));
            if (oute2.Type != typeof(double) && oute2.Type != typeof(double?) && oute2.Type.IsNumeric())
                oute2 = oute2.Type.IsNullable()
                    ? Expression.Convert(oute2, typeof(double?))
                    : Expression.Convert(oute2, typeof(double));

            // non-nullable operand is converted to nullable if necessary, and the lifted-to-nullable form of the comparison is used (C# rule, which is currently not followed by expression trees)
            if (oute1.Type.IsNullable() && !oute2.Type.IsNullable())
                oute2 = Expression.Convert(oute2, oute1.Type);
            else if (!oute1.Type.IsNullable() && oute2.Type.IsNullable())
                oute1 = Expression.Convert(oute1, oute2.Type);
        }

        public static bool IsDateTime(this Type type)
        {
            return type != null && (type == typeof(DateTime) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsDateTime()));
        }

        public static bool IsBool(this Type type)
        {
            return type != null && (type == typeof(bool) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsBool()));
        }

        public static bool IsGuid(this Type type)
        {
            return type != null && (type == typeof(Guid) || (type.IsNullable() && Nullable.GetUnderlyingType(type).IsGuid()));
        }

        public static bool IsString(this Type type)
        {
            return type != null && type == typeof(string);
        }

#if PORTABLE
        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            
            var numericTypes = new HashSet<Type>
            {
                typeof(sbyte),    //sbyte
                typeof(byte),     //byte
                typeof(short),    //short
                typeof(ushort),   //ushort
                typeof(int),    //int
                typeof(uint),   //uint
                typeof(long),    //long
                typeof(ulong),   //ulong
                typeof(float),   //float
                typeof(double),   //double
                typeof(decimal)   //decimal
            };
            return numericTypes.Any(t => t.Equals(type)) || type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
        }
#else
        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var numericTypes = new HashSet<TypeCode>
            {
                TypeCode.SByte,    //sbyte
                TypeCode.Byte,     //byte
                TypeCode.Int16,    //short
                TypeCode.UInt16,   //ushort
                TypeCode.Int32,    //int
                TypeCode.UInt32,   //uint
                TypeCode.Int64,    //long
                TypeCode.UInt64,   //ulong
                TypeCode.Single,   //float
                TypeCode.Double,   //double
                TypeCode.Decimal   //decimal
            };
            return numericTypes.Contains(Type.GetTypeCode(type)) || type.IsNullable() && Nullable.GetUnderlyingType(type).IsNumeric();
        }
#endif

        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

#if PORTABLE
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
#else
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
#endif
        }

        public static Type ToNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return typeof (Nullable<>).MakeGenericType(type);
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) 
                throw new ArgumentNullException("assembly");

            try
            {
#if PORTABLE
                return assembly.ExportedTypes;
#else
                return assembly.GetTypes();
#endif
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static string TrimStart(this string input, out int line, out int column)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var output = input.TrimStart();
            var redundancy = input.RemoveSuffix(output);
            var lastLineBreak = redundancy.LastIndexOf('\n');
            column = lastLineBreak > 0
                ? redundancy.Length - lastLineBreak
                : input.Length - output.Length;
            line = redundancy.CountLineBreaks();
            return output;
        }

        public static string Substring(this string input, int start, out int line, out int column)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (start < 0)
                throw new ArgumentOutOfRangeException("start", "Start index can not be negative.");

            var output = input.Substring(start);
            var redundancy = input.RemoveSuffix(output);
            var lastLineBreak = redundancy.LastIndexOf('\n');
            column = lastLineBreak > 0
                ? redundancy.Length - lastLineBreak
                : input.Length - output.Length;
            line = redundancy.CountLineBreaks();
            return output;
        }

        public static string RemoveSuffix(this string input, string suffix)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (suffix == null)
                throw new ArgumentNullException("suffix");

            return input.EndsWith(suffix)
                ? input.Substring(0, input.Length - suffix.Length)
                : input;
        }

        public static int CountLineBreaks(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

#if PORTABLE
            int count = 0;
            foreach (var c in input)
            {
                if (c == '\n')
                    count++;
            }
            return count;
#else
            return input.Count(n => n == '\n');
#endif
        }

        public static string TakeLine(this string input, int index)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Line indexes are 0-based.");

            return input.Split('\n').Skip(index).First().TrimEnd();
        }

        public static string ToOrdinal(this int num)
        {
            if (num <= 0) 
                return num.ToString(CultureInfo.InvariantCulture);

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }

        public static string Indicator(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            return string.Format(
@"
... {0} ...
    ^--- ", input);
        }
    }
}
