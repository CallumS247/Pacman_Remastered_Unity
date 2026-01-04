using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Abilities/Ability")]
public class Ability : ScriptableObject
{
    public string abilityName;
    public string description;
    public Sprite icon; 
    public float cooldownTime;
    public int requiredLevel;
    public AbilityType abilityType;
}

public enum AbilityType
{
    SpeedBoost,
    Freeze,
    DoublePoints,
    Frighten,
    Nuke,
    Teleport,
    ExtraLife
}
