/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Copyright (c) 2014 Muraad Nofal
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;

namespace ExpressiveAnnotations.Attributes
{
    public class DisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DisplayAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
