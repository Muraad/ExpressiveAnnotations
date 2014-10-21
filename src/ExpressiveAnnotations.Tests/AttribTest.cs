﻿using System;
using System.Collections.Generic;
#if !PORTABLE
using System.ComponentModel.DataAnnotations;
#endif
using System.Linq;
using System.Reflection;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class AttribTest
    {
        [TestMethod]
        public void verify_attributes_uniqueness()
        {
#if PORTABLE
            Portable.GetAssemblies = System.AppDomain.CurrentDomain.GetAssemblies;
#endif
            var attributes = typeof (Model).GetProperty("Value1")
#if PORTABLE
                .GetCustomAttributes<ConditionAttribute>()
#else
                .GetCustomAttributes()
#endif
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.AreEqual(2, attributes.Count); // ignores redundant attributes of the same type id, because they do nothing new (exact type name, exact expression)
            Assert.IsTrue(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));

            attributes = typeof (Model).GetProperty("Value2")
#if PORTABLE
                .GetCustomAttributes<ConditionAttribute>()
#else
                .GetCustomAttributes()
#endif
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.AreEqual(4, attributes.Count); // all type ids are unique (despite the same type names of some attributes, they contain different expressions)
            Assert.IsTrue(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));
        }

#if !PORTABLE
        [TestMethod]
        public void verify_validation_execution_for_derived_types()
        {
            // just assure that no exception is thrown during validation related to types casting and cached funcs
            var firstDerived = new FirstDerived();
            var secondDerived = new SecondDerived();

            var firstContext = new ValidationContext(firstDerived);
            var secondContext = new ValidationContext(secondDerived);

            // initialize annotated property value with null, for deep go-through of RequiredIf logic
            firstDerived.Value1 = null;
            secondDerived.Value1 = null;
            // put into cache different funcs...
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
            // ...and use already cached ones accordingly
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);

            // initialize annotated property value with non-null, for deep go-through of AssertThat logic
            firstDerived.Value1 = new object();
            secondDerived.Value1 = new object();
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
        }
#endif

        public abstract class ModelBase
        {
            [RequiredIf("Value2")]
            [AssertThat("Value2")]
            public object Value1 { get; set; }

            public bool Value2 { get; set; }
        }

        public class FirstDerived : ModelBase { }
        public class SecondDerived : ModelBase { }

        private class Model
        {
            [RequiredIf("1 > 0")]
            [RequiredIf("1 > 0")]
            [AssertThat("1 > 0")]
            [AssertThat("1 > 0")]
            public int Value1 { get; set; }

            [RequiredIf("1 > 0")]
            [RequiredIf("2 > 0")]
            [AssertThat("3 > 0")]
            [AssertThat("4 > 0")]
            public int Value2 { get; set; }
        }
    }

    public static class Helper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
