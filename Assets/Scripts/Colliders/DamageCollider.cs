using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [Header("Collider")]
    [SerializeField] protected Collider damageCollider;

    [Header("Damage")]
    public int physicalDamage = 0;
    public int magicDamage = 0;

    [Header("Contact Point")]
    protected Vector3 contactPoint;

    [Header("Characters Damaged")]
    protected List<CharacterManager> charactersDamaged = new List<CharacterManager>();

    public AICharacterCombatManager parentCombatManager;
    public CharacterManager character;


    protected virtual void Awake()
    {

    }

    private void Start()
    {
        parentCombatManager = GetComponentInParent<AICharacterCombatManager>();
        character = GetComponentInParent<CharacterManager>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
        //CharacterManager target = other.GetComponent<CharacterManager>();

        if (damageTarget != null)
        {
            contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            //check for friendly fire

            //check if target is blocking

            if(character.hasMultipleColliders)
            {
                // Prevent multiple hits from different colliders in the same attack
                if (parentCombatManager.damagedCharactersThisAttack.Contains(damageTarget))
                    return;
            }

            if(damageTarget.characterGroup == character.characterGroup)
            {
                return;
            }


            DamageTarget(damageTarget);
        }
        parentCombatManager.damagedCharactersThisAttack.Add(damageTarget);
    }

    protected virtual void DamageTarget(CharacterManager damageTarget)
    {
        //prevent dmg from being hit twice in a single attack -> add a lust that checks before applying dmg

        if (charactersDamaged.Contains(damageTarget))
        {
            return;
        }

        charactersDamaged.Add(damageTarget);

        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicDamage = magicDamage;
        damageEffect.contactPoint = contactPoint;

        damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    public virtual void EnableDamageCollider()
    {
        damageCollider.enabled = true;
    }

    public virtual void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        charactersDamaged.Clear(); //reset list of character that got hit on that swing
    }
}
