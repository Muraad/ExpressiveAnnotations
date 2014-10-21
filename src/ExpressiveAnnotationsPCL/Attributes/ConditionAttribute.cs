/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Copyright (c) 2014 Muraad Nofal
 * Licensed MIT: http://opensource.org/licenses/MIT */

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
