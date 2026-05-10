using System;
using UnityEngine;

public class UnitSelectionSystem : MonoBehaviour
{
    public static UnitSelectionSystem Instance;

    public UnitMovement SelectedUnit { get; private set; }

    public event Action<UnitMovement> UnitSelected;
    public event Action<UnitMovement> UnitDeselected;

    private void Awake()
    {
        Instance = this;
    }

    public void Select(UnitMovement unit)
    {
        if (SelectedUnit == unit)
            return;

        if (SelectedUnit != null)
        {
            UnitDeselected?.Invoke(SelectedUnit);
        }

        SelectedUnit = unit;

        UnitSelected?.Invoke(unit);

        Debug.Log($"Selected {unit.name}");
    }

    public void Deselect()
    {
        if (SelectedUnit == null)
            return;

        UnitDeselected?.Invoke(SelectedUnit);

        SelectedUnit = null;
    }

    public void MoveSelected(Vector3 hex)
    {
        if (SelectedUnit == null)
            return;

        SelectedUnit.MoveTo(hex);
    }
}