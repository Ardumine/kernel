using System;

namespace Kernel.AFCP.KASerializer.Atributes;


[AttributeUsage(AttributeTargets.Property)]
public class CanHaveOtherTypes : Attribute
{
}


[AttributeUsage(AttributeTargets.Property)]
public class IgnoreParse : Attribute
{
}


/// <summary>
/// When you have an array of unmanaged types like int, float, byte, etc...
/// You can also use struct, but make sure it's properies are unmanaged types, like char, byte, float, etc... String and arrays or list are not unmanaged types.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FastParseStructArray : Attribute
{
}



[AttributeUsage(AttributeTargets.Property)]
public class IsLongLength : Attribute
{
}


[AttributeUsage(AttributeTargets.Class)]
public class CanBeDerived : Attribute
{
}