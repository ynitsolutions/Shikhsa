using System;

namespace Shikhsa.Models.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MapNameAttribute : Attribute
    {
        // DataList lookup
        public string? DataListName { get; }

        // Property where resolved name/text will be stored
        public string NameProperty { get; }

        // Normal entity lookup
        public Type? LookupType { get; }

        public string? LookupKeyProperty { get; }

        public string? LookupNameProperty { get; }


        // DataListItem lookup
        public MapNameAttribute(
            string dataListName,
            string nameProperty)
        {
            DataListName = dataListName;
            NameProperty = nameProperty;
        }


        // Entity lookup
        public MapNameAttribute(
            Type lookupType,
            string nameProperty,
            string lookupKeyProperty,
            string lookupNameProperty)
        {
            LookupType = lookupType;
            NameProperty = nameProperty;
            LookupKeyProperty = lookupKeyProperty;
            LookupNameProperty = lookupNameProperty;
        }
    }
}