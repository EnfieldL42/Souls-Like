using UnityEngine;

public class WeaponItem : Item
{
    //animator controller override (change attakc animations based on weapon you are currently using)

    [Header("Weapon Model")]
    public GameObject weaponModel;

    [Header("Weapon Requirements")]
    public int strengthREQ = 0;
    public int dexREQ = 0;
    public int intREQ = 0;
    public int faithREQ = 0;

    [Header("Weapon Base Damage")]
    public int physicalDamage = 0;
    public int magicDamage = 0;

    [Header("Weapon Base Poise Damage")]
    public float poiseDamage = 10;
    //offensive poise bonus dmg when attacking


    [Header("Damage Modifier")]
    public float lightAttackModifer01 = 1.1f;
    public float lightAttackModifer02 = 1.2f;
    public float lightAttackModifer03 = 1.3f;
    public float lightAttackModifer04 = 1.4f;

    public float heavyAttackModifer01 = 1.4f;
    public float heavyAttackModifer02 = 1.6f;
    public float heavyAttackModifer03 = 1.8f;

    public float chargedAttackModifer01 = 2.0f;
    public float chargedAttackModifer02 = 2.2f;
    public float chargedAttackModifer03 = 2.4f;
    //light attach modifier
    //heavy attack modifier
    //critial damage modifier

    [Header("Stamina Costs Modifiers")]
    public int baseStaminaCost = 20;
    public float meleeAttackStaminaCostMultiplier = 0.9f;
    public float lightAttackStaminaCostMultiplier = 0.9f;
    public float heavyAttackStaminaCostMultiplier = 0.9f;
    public float chargedAttackStaminaCostMultiplier = 0.9f;
    //running attack stamina cost modifier (low importance)
    //heavy attack stamina cost modier


    [Header("Actions")]
    public WeaponItemAction oneHandedRBAction;
    public WeaponItemAction oneHandedRTAction;

    //ash of war (not happening)

    //blocking sounds

    [Header("Whooshing sounds")]
    public AudioClip[] whooshes;

}
