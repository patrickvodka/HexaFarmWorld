/*
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

public partial class S0_CustomBiomeSpawn
{
    
 private Cell[] baseCellsCopy = Array.Empty<Cell>();

    private void OnValidate()
    {
        AutoWeight();
    }

    private void AutoWeight()
    {
        if (baseCellsCopy.Length == baseCells.Length)
            WeightValues();
        else
            AddOrRemoveWeightedValue();

        baseCellsCopy = new Cell[baseCells.Length];
        Array.Copy(baseCells, baseCellsCopy, baseCells.Length);
    }

    private void WeightValues()
    {
        if (!CellInspectorModified(out var modifiedIndex))
            return;

        var oldTotalLeft = 1 - baseCellsCopy[modifiedIndex].chanceToAppear;
        var newTotalLeft = 1 - baseCells[modifiedIndex].chanceToAppear;
        var modifiedCellWasFullRate = oldTotalLeft == 0;

        for (var i = 0; i < baseCells.Length; i++)
        {
            if (i == modifiedIndex)
                continue;

            var currentCellWasDisabled = baseCellsCopy[i].chanceToAppear == 0;
            if (modifiedCellWasFullRate && currentCellWasDisabled)
            {
                baseCells[i].chanceToAppear = newTotalLeft / (baseCells.Length - 1);
            }
            else
            {
                var occupiedPercentLastTime = baseCellsCopy[i].chanceToAppear / oldTotalLeft;
                baseCells[i].chanceToAppear = newTotalLeft * occupiedPercentLastTime;
            }

            baseCells[i].chanceToAppear = Mathf.Clamp(baseCells[i].chanceToAppear, 0, 1);
        }
    }

    private bool CellInspectorModified(out int modifiedIndex)
    {
        modifiedIndex = -1;

        for (var i = 0; i < baseCells.Length; i++)
        {
            var chancesDiff = baseCells[i].chanceToAppear - baseCellsCopy[i].chanceToAppear;
            if (Math.Abs(chancesDiff) <= float.Epsilon)
                continue;
            modifiedIndex = i;
            return true;
        }

        return false;
    }

    private void AddOrRemoveWeightedValue()
    {
        var cellAdded = baseCells.Length > baseCellsCopy.Length;

        if (cellAdded)
        {
            for (var i = baseCellsCopy.Length; i < baseCells.Length; i++)
            {
                baseCells[i].chanceToAppear = 0;
                baseCells[i].prefab = null;
            }
        }
        else
        {
            var chancesToDivide = GetRemovedCellChances();
            chancesToDivide /= baseCells.Length;
            for (var i = 0; i < baseCells.Length; i++)
                baseCells[i].chanceToAppear += chancesToDivide;
        }
    }

    private float GetRemovedCellChances()
    {
        return baseCellsCopy
            .Where(cell => !baseCells.Contains(cell))
            .Select(cell => cell.chanceToAppear)
            .Sum();
    }
}
#endif
*/

