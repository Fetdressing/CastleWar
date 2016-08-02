using UnityEngine;
using System.Collections;

public class DamageHandler : MonoBehaviour {
    // Use this for initialization
    public Sprite fleshArmorSprite;
    public Sprite lightArmorSprite;
    public Sprite mediumArmorSprite;
    public Sprite heavyArmorSprite;
    public Sprite etherealArmorSprite;
    public Sprite magicArmorSprite;
    public Sprite divineArmorSprite;
    public Sprite structureArmorSprite;

    public Sprite slicingDamageSprite;
    public Sprite bluntDamageSprite;
    public Sprite piercingDamageSprite;
    public Sprite magicDamageSprite;
    public Sprite siegeDamageSprite;

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

    public Sprite GetArmorSprite(TypeArmor tA)
    {
        switch(tA)
        {
            case TypeArmor.Flesh:
                return fleshArmorSprite;
            case TypeArmor.Light:
                return lightArmorSprite;
            case TypeArmor.Medium:
                return mediumArmorSprite;
            case TypeArmor.Heavy:
                return heavyArmorSprite;
            case TypeArmor.Ethereal:
                return etherealArmorSprite;
            case TypeArmor.Magic:
                return magicArmorSprite;
            case TypeArmor.Divine:
                return divineArmorSprite;
            case TypeArmor.Structure:
                return structureArmorSprite;
            default:
                Debug.Log("Ej fastställd TypeArmor");
                return fleshArmorSprite;
        }
    }

    public Sprite GetDamageSprite(TypeDamage tD)
    {
        switch (tD)
        {
            case TypeDamage.Slicing:
                return slicingDamageSprite;
            case TypeDamage.Blunt:
                return bluntDamageSprite;
            case TypeDamage.Piercing:
                return piercingDamageSprite;
            case TypeDamage.Magic:
                return magicDamageSprite;
            case TypeDamage.Siege:
                return siegeDamageSprite;
            default:
                Debug.Log("Ej fastställd TypeDamage");
                return slicingDamageSprite;
        }
    }
}

public enum TypeArmor { Flesh, Light, Medium, Heavy, Ethereal, Magic, Divine, Structure };
public enum TypeDamage { Slicing, Blunt, Piercing, Magic, Siege };
