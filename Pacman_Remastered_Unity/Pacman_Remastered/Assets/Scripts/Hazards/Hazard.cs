using UnityEngine;

[CreateAssetMenu(fileName = "NewHazard", menuName = "Hazards/Hazard")]
public class Hazard : ScriptableObject
{
    public string hazardName;
    public string description;
    public Sprite icon;
    public HazardType hazardType;
}

public enum HazardType
{
    Nearsight,
    DmgZone,
    InvertControls,
    GhostImmune,
    PowerDrain,
    Countdown
}
