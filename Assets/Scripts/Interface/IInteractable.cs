using UnityEngine;

public interface IInteractable
{
    bool CanInteract(InteractionContext context);
    void OnInteract(InteractionContext context);
    void OnDoubleInteract(InteractionContext context);
}