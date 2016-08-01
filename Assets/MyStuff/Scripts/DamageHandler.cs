using UnityEngine;
using System.Collections;

public class DamageHandler : MonoBehaviour {
    // Use this for initialization
    
    public float GetDamageEfficiency(TypeDamage damageType, TypeArmor armorType)
    {
        switch (damageType)
        {
            case TypeDamage.Slicing:
                return(GetDamageEfficiencySlicing(armorType));
            case TypeDamage.Blunt:
                return (GetDamageEfficiencyBlunt(armorType));
            case TypeDamage.Piercing:
                return (GetDamageEfficiencyPiercing(armorType));
            case TypeDamage.Magic:
                return (GetDamageEfficiencyMagic(armorType));
            case TypeDamage.Siege:
                return (GetDamageEfficiencySiege(armorType));
            default:
                return 1.0f;
        }
    }

    public float GetDamageEfficiencySlicing(TypeArmor armorType)
    {
        switch (armorType)
        {
            case TypeArmor.Flesh:
                return 2.0f;
            case TypeArmor.Light:
                return 1.3f;
            case TypeArmor.Medium:
                return 0.8f;
            case TypeArmor.Heavy:
                return 0.3f;
            case TypeArmor.Ethereal:
                return 0.1f;
            case TypeArmor.Magic:
                return 0.7f;
            case TypeArmor.Divine:
                return 0.2f;
            case TypeArmor.Structure:
                return 0.2f;
            default:
                return 1.0f;
        }
    }

    public float GetDamageEfficiencyBlunt(TypeArmor armorType)
    {
        switch (armorType)
        {
            case TypeArmor.Flesh:
                return 1.0f;
            case TypeArmor.Light:
                return 0.7f;
            case TypeArmor.Medium:
                return 0.9f;
            case TypeArmor.Heavy:
                return 1.3f;
            case TypeArmor.Ethereal:
                return 0.1f;
            case TypeArmor.Magic:
                return 0.5f;
            case TypeArmor.Divine:
                return 0.2f;
            case TypeArmor.Structure:
                return 0.7f;
            default:
                return 1.0f;
        }
    }

    public float GetDamageEfficiencyPiercing(TypeArmor armorType)
    {
        switch (armorType)
        {
            case TypeArmor.Flesh:
                return 1.3f;
            case TypeArmor.Light:
                return 1.3f;
            case TypeArmor.Medium:
                return 0.9f;
            case TypeArmor.Heavy:
                return 0.5f;
            case TypeArmor.Ethereal:
                return 0.1f;
            case TypeArmor.Magic:
                return 1.3f;
            case TypeArmor.Divine:
                return 0.2f;
            case TypeArmor.Structure:
                return 0.1f;
            default:
                return 1.0f;
        }
    }

    public float GetDamageEfficiencyMagic(TypeArmor armorType)
    {
        switch (armorType)
        {
            case TypeArmor.Flesh:
                return 0.4f;
            case TypeArmor.Light:
                return 0.7f;
            case TypeArmor.Medium:
                return 0.9f;
            case TypeArmor.Heavy:
                return 1.5f;
            case TypeArmor.Ethereal:
                return 2.8f;
            case TypeArmor.Magic:
                return 1.0f;
            case TypeArmor.Divine:
                return 0.2f;
            case TypeArmor.Structure:
                return 0.6f;
            default:
                return 1.0f;
        }
    }

    public float GetDamageEfficiencySiege(TypeArmor armorType)
    {
        switch (armorType)
        {
            case TypeArmor.Flesh:
                return 0.4f;
            case TypeArmor.Light:
                return 0.7f;
            case TypeArmor.Medium:
                return 0.9f;
            case TypeArmor.Heavy:
                return 1.5f;
            case TypeArmor.Ethereal:
                return 2.8f;
            case TypeArmor.Magic:
                return 1.0f;
            case TypeArmor.Divine:
                return 0.2f;
            case TypeArmor.Structure:
                return 3.2f;
            default:
                return 1.0f;
        }
    }
}

public enum TypeArmor { Flesh, Light, Medium, Heavy, Ethereal, Magic, Divine, Structure };
public enum TypeDamage { Slicing, Blunt, Piercing, Magic, Siege };
