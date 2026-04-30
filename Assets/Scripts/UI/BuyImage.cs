using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyImage : MonoBehaviour, IClickable
{
    /*[HideInInspector]*/public SO_ShopSlot shopSlot;
      public bool OnSelected()
    {
        Debug.Log($"{gameObject} + { shopSlot.Cost[0]}");
        return true;
    }

    public void OnDoubleClicked()
    {
        
    }
}
