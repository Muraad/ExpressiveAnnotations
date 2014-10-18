using System;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Attributes
{
#if PORTABLE
    public abstract class ConditionAttribute : Attribute
    {
        public abstract Tuple<bool, string, string> IsValid(object value, string memberName = "");

        public virtual object TypeId
        {
            get { return null ; } // distinguishes instances based on provided expressions
        }
    }
#endif
}
