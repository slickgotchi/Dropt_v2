using System;

namespace CarlosLab.UtilityIntelligence
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CategoryAttribute : Attribute
    {
        public readonly string Category;
        
        public CategoryAttribute(string category)
        {
            Category = category;
        }
    }
}