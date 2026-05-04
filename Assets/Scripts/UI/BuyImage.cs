using UnityEngine;

public class BuyImage : MonoBehaviour, IInteractable
{
    public SO_ShopSlot shopSlot;

    public bool CanInteract(InteractionContext context) => true;

    public void OnInteract(InteractionContext context)
    {
        CommandInvoker.Instance.Execute(new BuyItemCommand(shopSlot));
    }

    public void OnDoubleInteract(InteractionContext context)
    {
        Debug.Log("Double click shop (optionnel)");
    }
}