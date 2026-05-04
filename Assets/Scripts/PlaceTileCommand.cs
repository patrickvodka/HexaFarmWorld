using UnityEngine;

public class PlaceTileCommand : ICommand
{
    private Vector3 pos;

    public PlaceTileCommand(Vector3 pos)
    {
        this.pos = pos;
    }

    public void Execute()
    {
        TilePlacementSystem.Instance.Place(pos);
    }
}