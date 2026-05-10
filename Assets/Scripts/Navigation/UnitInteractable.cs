using UnityEngine;
public class UnitInteractable : MonoBehaviour, IInteractable
{
    private UnitMovement movement;

    private void Awake()
    {
        movement = GetComponent<UnitMovement>();
    }

    public bool CanInteract(InteractionContext context)
    {
        return true;
    }

    public void OnInteract(InteractionContext context)
    {
        UnitSelectionSystem.Instance.Select(movement);
    }

    public void OnDoubleInteract(InteractionContext context)
    {
    }
}