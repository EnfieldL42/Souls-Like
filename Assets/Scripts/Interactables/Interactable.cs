using UnityEngine;
using Unity.Netcode;

public class Interactable : NetworkBehaviour
{
    public string interactableText;
    [SerializeField] protected Collider interactableCollider;
    [SerializeField] protected bool hostOnlyInteractable = true;

    protected virtual void Awake()
    {
        if (interactableCollider == null)
        {
            interactableCollider = GetComponent<Collider>();
        }
    }

    protected virtual void Start()
    {
        
    }


    public virtual void Interact(PlayerManager player)
    {
        if (!player.IsOwner)
        {
            return;
        }

        interactableCollider.enabled = false;
        player.playerInteractionManager.RemoveInteractionFromList(this);
        PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();

        if(player != null)
        {
            if (!player.playerNetworkManager.IsHost && hostOnlyInteractable)
            {
                return;
            }
            if(!player.IsOwner)
            {
                return;
            }

            player.playerInteractionManager.AddInteractonToList(this);
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player != null)
        {
            if (!player.playerNetworkManager.IsHost && hostOnlyInteractable)
            {
                return;
            }
            if (!player.IsOwner)
            {
                return;
            }

            player.playerInteractionManager.RemoveInteractionFromList(this);
            PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
            

        }
    }

}
