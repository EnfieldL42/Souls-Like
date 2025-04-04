using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : CharacterManager
{
    [Header("DEBUG MENU")]
    [SerializeField] bool respawnCharacter = false;
    [SerializeField] bool switchRightWeapon = false;
    [SerializeField] bool switchLeftWeapon = false;


    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerNetworkManager playerNetworkManager;
    [HideInInspector] public PlayerInventoryManager playerInventoryManager;
    [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
    [HideInInspector] public PlayerCombatManager PlayerCombatManager;

    protected override void Awake()
    {
        base.Awake(); //ok so its like an awake that happens after the main awake in the parent class(charactermanager) /(not too sure why this is useful just yet)


        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerNetworkManager = GetComponent<PlayerNetworkManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        PlayerCombatManager = GetComponent<PlayerCombatManager>();

    }

    protected override void LateUpdate()
    {
        if(!IsOwner)
        {
            return;
        }

        base.LateUpdate();

        PlayerCamera.instance.HandleAllCameraActions();
    }

    protected override void Update()
    {
        base.Update();

        if(!IsOwner)//if we dont own the gameobject we cant control it
        {
            return;
        }

        playerLocomotionManager.HandleAllMovement();
        playerStatsManager.RegenerateStamina();

        DebugMenu();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsOwner)
        {
            PlayerInputManager.instance.player = this;
            PlayerCamera.instance.player = this;
            WorldSaveGameManager.instance.player = this;

            //update health and stamina when vitality/endurance stats changes
            playerNetworkManager.vitality.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
            playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;


            //updates ui stat bar when health/stamina changes
            playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
            playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
            playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaRegenTimer;

        }

        //STATS
        playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.CheckHP;

        //EQUIPMENT
        playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChange;
        playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
        playerNetworkManager.currentWeaponBeingUsed.OnValueChanged += playerNetworkManager.OnCurrentWeaponBeingUsedIDChange;


        //for clients, reload character data for newly instantiated characters
        if(IsOwner && !IsServer)
        {
            LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);
        }

    }

    public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
    {
        if(IsOwner)
        {
            PlayerUIManager.instance.playerUIPopUpManager.SendYouDiePopUp();    
        }    

        //check for players that are alive, if 0 then respawn


        return base.ProcessDeathEvent(manuallySelectDeathAnimation);



    }

    public override void ReviveCharacter()
    {
        base.ReviveCharacter();

        if(IsOwner)
        {
            //isDead.Value = false;
            playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
            playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
            //restore mana

            //play rebirth effects
            playerAnimatorManager.PlayTargetActionAnimation("Empty", true);
        }
    }

    public void SaveGameDataToCurrentCharacterData(ref CharacterSaveData currentCharacterData)
    {
        currentCharacterData.sceneIndex = SceneManager.GetActiveScene().buildIndex;

        currentCharacterData.characterName = playerNetworkManager.characterName.Value.ToString();
        currentCharacterData.xPosition = transform.position.x;
        currentCharacterData.yPosition = transform.position.y;
        currentCharacterData.zPosition = transform.position.z;

        currentCharacterData.currentHealth = playerNetworkManager.currentHealth.Value;
        currentCharacterData.currentStamina = playerNetworkManager.currentStamina.Value;

        currentCharacterData.vitality = playerNetworkManager.vitality.Value;
        currentCharacterData.endurance = playerNetworkManager.endurance.Value;


    }

    public void LoadGameDataFromCurrentCharacterData(ref CharacterSaveData currentCharacterData)
    {
        playerNetworkManager.characterName.Value = currentCharacterData.characterName;
        Vector3 myPosition = new Vector3(currentCharacterData.xPosition, currentCharacterData.yPosition, currentCharacterData.zPosition);
        transform.position = myPosition;

        playerNetworkManager.vitality.Value = currentCharacterData.vitality;
        playerNetworkManager.endurance.Value = currentCharacterData.endurance;

        playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnLevel(playerNetworkManager.vitality.Value);
        playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnLevel(playerNetworkManager.endurance.Value);
        playerNetworkManager.currentHealth.Value = currentCharacterData.currentHealth;
        playerNetworkManager.currentStamina.Value = currentCharacterData.currentStamina;

        PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(playerNetworkManager.maxHealth.Value);
    }

    private void DebugMenu()
    {
        if(respawnCharacter && isDead.Value)
        {
            isDead.Value = false;
            respawnCharacter = false;
            ReviveCharacter();
        }
        else
        {
            respawnCharacter = false; 
        }

        if(switchRightWeapon)
        {
            switchRightWeapon = false;
            playerEquipmentManager.SwitchRightWeapon();
        }
        if(switchLeftWeapon)
        {
            switchLeftWeapon = false;
            playerEquipmentManager.SwitchLeftWeapon();
        }


    }



}
