using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyImage : MonoBehaviour, IClickable
{
    public void OnSelected()
    {
        Debug.Log($"{gameObject}");
    }

    public void OnDoubleClicked()
    {
        
    }
}
