using System;

namespace UtilAttribute
{
    
    [AttributeUsage(AttributeTargets.Method)]
    public class MustCallBaseAttribute : Attribute
    {
    }
}