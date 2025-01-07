namespace AFCP.KASerializer.Atributes;

/// <summary>
/// When the property can have another types. This includes derivation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CanHaveOtherTypes : Attribute
{
}