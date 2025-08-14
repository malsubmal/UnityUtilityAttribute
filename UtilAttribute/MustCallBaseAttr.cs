using System;

namespace AttributeRe
{
    
    [AttributeUsage(AttributeTargets.Method)]
    public class MustCallBaseAttribute : Attribute
    {
    }
}