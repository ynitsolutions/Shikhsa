using Shikhsa.Enums;

namespace Shikhsa.Attributes
{
    //[AttributeUsage(AttributeTargets.Method)]
    //public class PermissionAttribute : Attribute
    //{
    //    public PermissionType PermissionType { get; }

    //    public PermissionAttribute(PermissionType permissionType)
    //    {
    //        PermissionType = permissionType;
    //    }
    //}
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipPermissionAttribute : Attribute
    {
    }
}
