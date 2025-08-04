#if UNITY_EDITOR
using UnityEngine;

public class FogManager : MonoBehaviour
{
    public Texture2D fogTexture;
    public Vector4 fogParams = new Vector4(1, 1, 0, 0);
    public Material[] targetMaterials;

    void Update()
    {
        foreach (var mat in targetMaterials)
        {
            mat.SetTexture("_FogTex", fogTexture);
            mat.SetVector("_FogParams", fogParams);
        }
    }
}
#endif