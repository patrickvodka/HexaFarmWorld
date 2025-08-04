using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{


    public void ChangeCurrentSelection(GameObject ObjToChangeTo)
    {
        hexaTilePainter.tileToPlace = ObjToChangeTo;
    }
}
