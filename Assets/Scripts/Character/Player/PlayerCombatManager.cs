using UnityEngine;
using Unity.Netcode;


public class PlayerCombatManager : CharacterCombatManager
{
    PlayerManager player;
    public WeaponItem currentWeaponBeingUsed;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
    }

    public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
    {
        if(player.IsOwner)
        {
            //perform action

            weaponAction.AttemptToPerformAction(player, weaponPerformingAction);

            //also perform hosts action to clients
            player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);
        }

    }

    public virtual void DrainStaminaBasedOnAttack()
    {
        if(!player.IsOwner)
        {
            return;
        }
        if(currentWeaponBeingUsed == null)
        {
            return;
        }

        float staminaDeducted = 0;

        switch(currentAttackType)
        {
            case AttackType.LightAttack01:
                staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                break;
            default: 
                break;
        }

        player.playerNetworkManager.currentStamina.Value -= Mathf.RoundToInt(staminaDeducted);
    }
}
