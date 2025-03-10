namespace Content.Server.Emp;

/// <summary>
/// Upon collision will EMP area around it.
/// </summary>
[RegisterComponent]
[Access(typeof(EmpSystem))]
public sealed partial class EmpOnCollideComponent : Component
{
    /// <summary>
    /// EMP range.
    /// </summary>
    [DataField]
    public float Range = 1.0f;

    /// <summary>
    /// How much energy will be consumed per battery in range.
    /// </summary>
    [DataField]
    public float EnergyConsumption;

    /// <summary>
    /// How long it disables entity in seconds.
    /// </summary>
    [DataField]
    public float DisableDuration = 60f;

    /// <summary>
    /// Fixture we track for the collision.
    /// </summary>
    [DataField]
    public string Fixture = "projectile";
}
