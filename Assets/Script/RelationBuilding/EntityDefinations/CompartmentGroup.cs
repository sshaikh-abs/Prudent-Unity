using System.Collections.Generic;

[System.Serializable]
public class CompartmentGroup
{
    /// <summary>
    /// Name of the compartment group (typically based on compartment function).
    /// </summary>
    public string name;
    /// <summary>
    /// data means all the compartments.
    /// </summary>
    public List<Compartment> data = null;
}
