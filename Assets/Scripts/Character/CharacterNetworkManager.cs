using UnityEngine;
using Unity.Netcode;
using static UnityEngine.Rendering.DebugUI;

public class CharacterNetworkManager : NetworkBehaviour
{
    CharacterManager character;

    [Header("Active")]
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Position")]
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkPositionVelocity;
    public float networkPositionSmoothTime = 0.1f;
    public float networkRotationSmoothTime = 0.1f;

    [Header("Animator")]
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> horizontalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> verticalMovement = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> moveAmount = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Target")]
    public NetworkVariable<ulong> currentTargetNetworkObjectID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Flags")]
    public NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isLockedOn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isChargingAttack = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isBlocking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Resources")]
    public NetworkVariable<float> currentStamina = new NetworkVariable<float> (0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> maxStamina = new NetworkVariable<float> (0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats")]
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> endurance = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public virtual void CheckHP(int oldValue, int newValue)
    {
        if(currentHealth.Value <= 0)
        {
            StartCoroutine(character.ProcessDeathEvent());

        }
        if(character.IsOwner)//prevents over healing
        {
            if(currentHealth.Value > maxHealth.Value)
            {
                currentHealth.Value = maxHealth.Value;
            }
        }
    }

    public void OnLockOnTargetIDChange(ulong oldID, ulong newID)
    {
        if(!IsOwner)
        {
            character.characterCombatManager.currentTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[newID].gameObject.GetComponent<CharacterManager>();
        }
    }

    public void OnIsLockedOnChanged(bool old, bool isLockedOn)
    {

        if (!isLockedOn)
        {
            character.characterCombatManager.currentTarget = null;
        }
    }

    public void OnIsChargingAttackChanged(bool oldStatus, bool newStatus)
    {

        character.animator.SetBool("isChargingAttack", isChargingAttack.Value);
    }

    public void OnIsMovingChanged(bool oldStatus, bool newStatus)
    {

        character.animator.SetBool("isMoving", isMoving.Value);
    }

    public virtual void OnIsActiveChanged(bool oldStatus, bool newStatus)
    {
        gameObject.SetActive(isActive.Value);
    }

    public virtual void OnIsBlockingOnChanged(bool oldStatus, bool newStatus)
    {
         character.animator.SetBool("isBlocking", isBlocking.Value);
    }


    [ServerRpc]//function called from a client to the server/host
    public void NotifyTheServerOfActionAnimationServerRpc(ulong clientID, string animationID, bool applyRootMotion)
    {
        //if plauer is host, activate client rpc
        if(IsServer)
        {
            PlayActionAnimationForAllClientsClientRpc(clientID, animationID, applyRootMotion);
        }
    }

    [ClientRpc]//client rpc is sent to all clients(from the server)
    public void PlayActionAnimationForAllClientsClientRpc(ulong clientID, string animationID, bool applyRootMotion)
    {
        //make sure we dont run the function on the character that sent it (dont play the animation twice for the player playing)
        if (clientID != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationFromServer(animationID, applyRootMotion);
        }
    }

    private void PerformActionAnimationFromServer(string animationID, bool applyRootMotion)
    {
        character.applyRootMotion = applyRootMotion;

        character.animator.CrossFade(animationID, 0.2f);
    }

    //ATTACK ANIMATION

    [ServerRpc]//function called from a client to the server/host
    public void NotifyTheServerOfAttackActionAnimationServerRpc(ulong clientID, string animationID, bool applyRootMotion)
    {
        //if plauer is host, activate client rpc
        if (IsServer)
        {
            PlayAttackActionAnimationForAllClientsClientRpc(clientID, animationID, applyRootMotion);
        }
    }

    [ClientRpc]//client rpc is sent to all clients(from the server)
    public void PlayAttackActionAnimationForAllClientsClientRpc(ulong clientID, string animationID, bool applyRootMotion)
    {
        //make sure we dont run the function on the character that sent it (dont play the animation twice for the player playing)
        if (clientID != NetworkManager.Singleton.LocalClientId)
        {
            PerformAttackActionAnimationFromServer(animationID, applyRootMotion);
        }
    }

    private void PerformAttackActionAnimationFromServer(string animationID, bool applyRootMotion)
    {
        character.applyRootMotion = applyRootMotion;

        character.animator.CrossFade(animationID, 0.2f);
    }

    //DAMAGE

    [ServerRpc(RequireOwnership = false)]
    public void NotifyTheServerOfCharacterDamageServerRpc(
        ulong damagedCharacterID,
        ulong characterCausingDamageID,
        float physicalDamage, 
        float magicDamage, 
        float poiseDamage, 
        float angleHitFrom, 
        float contactPointX, 
        float contactPointY, 
        float contactPointZ)
    {
        if (IsServer)
        {
            NotifyTheServerOfCharacterDamageClientRpc(damagedCharacterID, characterCausingDamageID, physicalDamage, magicDamage, poiseDamage, angleHitFrom, contactPointX, contactPointY, contactPointZ);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    public void NotifyTheServerOfCharacterDamageClientRpc(
        ulong damagedCharacterID,
        ulong characterCausingDamageID,
        float physicalDamage,
        float magicDamage,
        float poiseDamage,
        float angleHitFrom,
        float contactPointX,
        float contactPointY,
        float contactPointZ)
    {
        ProcessCharacterDamageFromServer(damagedCharacterID, characterCausingDamageID, physicalDamage, magicDamage, poiseDamage, angleHitFrom, contactPointX, contactPointY, contactPointZ); 
    }

    public void ProcessCharacterDamageFromServer(
        ulong damagedCharacterID,
        ulong characterCausingDamageID,
        float physicalDamage,
        float magicDamage,
        float poiseDamage,
        float angleHitFrom,
        float contactPointX,
        float contactPointY,
        float contactPointZ)
    {
        CharacterManager damagedCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[damagedCharacterID].gameObject.GetComponent<CharacterManager>();
        CharacterManager characterCausingDamage = NetworkManager.Singleton.SpawnManager.SpawnedObjects[characterCausingDamageID].gameObject.GetComponent<CharacterManager>();

        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);

        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicDamage = magicDamage;
        damageEffect.poiseDamage = poiseDamage;
        damageEffect.angleHitFrom = angleHitFrom;
        damageEffect.contactPoint = new Vector3(contactPointX, contactPointY, contactPointZ);
        damageEffect.characterCausingDamage = characterCausingDamage;

        damagedCharacter.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }
}
