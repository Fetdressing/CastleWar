using UnityEngine;
using System.Collections;

public class DamageHandler : MonoBehaviour {
    // Use this for initialization
    public Sprite defaultSprite;

    public Sprite fleshArmorSprite;
    private string fleshArmor_ToolTip = "Flesh\nStrong resistance versus magic, weak resistance versus slicing attacks.";

    public Sprite lightArmorSprite;
    private string lightArmor_ToolTip = "Light\nStrong resistance versus blunt, weak resistance versus piercing and slicing attacks.";

    public Sprite mediumArmorSprite;
    private string mediumArmor_ToolTip = "Medium\nWeak resistance versus slicing attacks.";

    public Sprite heavyArmorSprite;
    private string heavyArmor_ToolTip = "Heavy\nStrong resistance versus slicing and piercing, weak resistance versus magic attacks.";

    public Sprite etherealArmorSprite;
    private string etherealArmor_ToolTip = "Ethereal\nWeak resistance versus magic.";

    public Sprite magicArmorSprite;
    private string magicArmor_ToolTip = "Magic\nStrong resistance versus blunt, weak resistance versus piercing attacks.";

    public Sprite divineArmorSprite;
    private string divineArmor_ToolTip = "Divine\nStrong resistance versus all attacks.";

    public Sprite structureArmorSprite;
    private string structureArmor_ToolTip = "Structure\nWeak resistance versus siege attacks.";


    public Sprite slicingDamageSprite;
    private string slicingDamage_ToolTip = "Slicing\nStrong versus flesh and light armor.";

    public Sprite bluntDamageSprite;
    private string bluntDamage_ToolTip = "Blunt\nStrong versus heavy armor.";

    public Sprite piercingDamageSprite;
    private string piercingDamage_ToolTip = "Piercing\nStrong versus magic armor.";

    public Sprite magicDamageSprite;
    private string magicDamage_ToolTip = "Magic\nStrong versus heavy armor.";

    public Sprite siegeDamageSprite;
    private string siegeDamage_ToolTip = "Siege\nStrong versus structures.";


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
                return 0.6f;
            case TypeArmor.Ethereal:
                return 0.1f;
            case TypeArmor.Magic:
                return 1.0f;
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
                return 1.4f;
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
                return 0.6f;
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
                return defaultSprite;
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
                return defaultSprite;
        }
    }

    public Sprite GetDefaultSprite()
    {
        return defaultSprite;
    }

    public string GetArmorToolTip(TypeArmor tA)
    {
        switch (tA)
        {
            case TypeArmor.Flesh:
                return fleshArmor_ToolTip;
            case TypeArmor.Light:
                return lightArmor_ToolTip;
            case TypeArmor.Medium:
                return mediumArmor_ToolTip;
            case TypeArmor.Heavy:
                return heavyArmor_ToolTip;
            case TypeArmor.Ethereal:
                return etherealArmor_ToolTip;
            case TypeArmor.Magic:
                return magicArmor_ToolTip;
            case TypeArmor.Divine:
                return divineArmor_ToolTip;
            case TypeArmor.Structure:
                return structureArmor_ToolTip;
            default:
                Debug.Log("Ej fastställd TypeArmor");
                return "";
        }
    }
    public string GetDamageToolTip(TypeDamage tA)
    {
        switch (tA)
        {
            case TypeDamage.Slicing:
                return slicingDamage_ToolTip;
            case TypeDamage.Blunt:
                return bluntDamage_ToolTip;
            case TypeDamage.Piercing:
                return piercingDamage_ToolTip;
            case TypeDamage.Magic:
                return magicDamage_ToolTip;
            case TypeDamage.Siege:
                return siegeDamage_ToolTip;
            default:
                Debug.Log("Ej fastställd TypeArmor");
                return "";
        }
    }
}

public enum TypeArmor { Flesh, Light, Medium, Heavy, Ethereal, Magic, Divine, Structure };
public enum TypeDamage { Slicing, Blunt, Piercing, Magic, Siege };
