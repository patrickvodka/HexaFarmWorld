using System.Collections.Generic;
using UnityEngine;

public class BaseTile : MonoBehaviour, IClickable
{
    public CellType cellType;
    public CeilClass ceilClass;
    // bordures pour l'inspector
    [HideInInspector]public List<int> Border_1= new List<int>();
    [HideInInspector] public List<int> Border_2= new List<int>();
    [HideInInspector]public List<int> Border_3= new List<int>();
    [HideInInspector]public List<int> Border_4= new List<int>();
    [HideInInspector]public List<int> Border_5= new List<int>();
    [HideInInspector]public List<int> Border_6= new List<int>();
    [Min(0)]
    private List<int>[] tableauDeInt = new List<int>[6] ;

    // Méthode pour initialiser la tuile
    public void Initialize(Vector3 hexCoord)
    {
        if (ceilClass == null || ceilClass.hexCoord == Vector3.zero)
        {
            ceilClass = new CeilClass(hexCoord);
        }

        if (cellType.borders == null)
        {
            
        }

        cellType.prefab = transform.gameObject;
    }

    //Editor only
    public void ReturnInput()
    {
        SetupArray();
            for (int i = 0; i < 6; i++)
            {
                cellType.borders[i] = tableauDeInt[i];
            }
           
    }
    public void SaveInput()
    {
        
        Border_1 = cellType.borders[0];
        Border_2 = cellType.borders[1];
        Border_3 = cellType.borders[2];
        Border_4 = cellType.borders[3];
        Border_5 = cellType.borders[4];
        Border_6 = cellType.borders[5];
    }

    private void SetupArray()
    {
        tableauDeInt[0] = Border_1;
        tableauDeInt[1] = Border_2;
        tableauDeInt[2] = Border_3;
        tableauDeInt[3] = Border_4;
        tableauDeInt[4] = Border_5;
        tableauDeInt[5] = Border_6;
    }

    #region Interface, all interfaces fonctions 

     public void OnSelected()
    {
        Debug.Log($"{gameObject}");
    }


    public void OnDoubleClicked()
    {

    }

    #endregion

}


