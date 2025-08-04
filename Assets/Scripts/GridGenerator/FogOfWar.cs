using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public int textureSize = 256;
    public float hexWidth = 30f;   
    public float hexHeight = 34.5f; 
    public Texture2D fogTexture;
    public HexaWaveFonctCollapse hexaWfc;
    public Material materialTest;
    [Range(0, 1)]
    public float fogAlpha = 0f;

    void Start()
    {
        GenerateFogTextureFromHexGrid();
    }

    public void GenerateFogTextureFromHexGrid()
    {
        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0, 0, 0, fogAlpha);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
                fogTexture.SetPixel(x, y, clear);
        }

        foreach (Vector3 hexCoord in hexaWfc.HexGridDictionary.Keys)
        {
            Vector2Int texCoord = HexToTextureCoord(hexCoord);
            if (texCoord.x >= 0 && texCoord.x < textureSize && texCoord.y >= 0 && texCoord.y < textureSize)
            {
                fogTexture.SetPixel(texCoord.x, texCoord.y, Color.green);
            }
        }

        fogTexture.Apply();
        materialTest.SetTexture("_FogTex", fogTexture);
        materialTest.SetFloat("_FogAlpha", fogAlpha);
        
    }



    private Vector2Int HexToTextureCoord(Vector3 hexCoord)
    {
        Vector3 worldPos = hexaWfc.HexToWorldPosition(hexCoord);

        float mapWidth = (hexaWfc.radiusMap * 2 + 1) * hexWidth;
        float mapHeight = (hexaWfc.radiusMap * 2 + 1) * hexHeight;

        float centerX = mapWidth / 2f;
        float centerY = mapHeight / 2f;

        float localX = worldPos.x + centerX;
        float localY = worldPos.z + centerY;

        float texelPerUnitX = textureSize / mapWidth;
        float texelPerUnitY = textureSize / mapHeight;

        int texX = Mathf.RoundToInt(localX * texelPerUnitX);
        int texY = Mathf.RoundToInt(localY * texelPerUnitY);

        //  rotated by 90°
        int rotatedX = texY;
        int rotatedY = textureSize - 1 - texX;

        return new Vector2Int(rotatedX, rotatedY);
    }


    /// <summary>
    /// Révèle dynamiquement un hexagone à une position grille (Vector3 hexCoord).
    /// </summary>
    public void RevealHexAtHexCoord(Vector3 hexCoord)
    {
        Vector2Int texCoord = HexToTextureCoord(hexCoord);

        float worldToTexelX = textureSize / ((hexaWfc.radiusMap * 2 + 1) * hexWidth);
        float worldToTexelY = textureSize / ((hexaWfc.radiusMap * 2 + 1) * hexHeight);

        
        //int pixelRadius = Mathf.RoundToInt((hexWidth * 0.5f) * worldToTexelX);

        

        
        int pixelRadius = Mathf.RoundToInt((hexHeight * 0.5f) * worldToTexelY);


        RevealHex(texCoord.x, texCoord.y, pixelRadius);
    }

    /// <summary>
    /// Révèle un hexagone dans la texture en coordonnées pixels.
    /// </summary>
    public void RevealHex(int centerX, int centerY, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize)
                    continue;

                if (IsPointInHexagon(x, y, radius))
                {
               
                    //fogTexture.SetPixel(px, py, new Color(0, 0, 0, 0)); // Transparent
                    int rotatedX = py;
                    int rotatedY = textureSize - 1 - px;

                    fogTexture.SetPixel(rotatedX, rotatedY, new Color(0, 0, 0, 0)); // Transparent tourné 90°

                }
            }
        }

        fogTexture.Apply();
    }

    /// <summary>
    /// Test si un point local (x,y) est dans un hexagone pointy (approximation 2D).
    /// </summary>
    private bool IsPointInHexagon(int dx, int dy, int radius)
    {
        // Caméra en vue de haut : X/Z = écran, donc on inverse les axes
        float aspectRatio = hexWidth / hexHeight;

        float q = Mathf.Abs(dx) * aspectRatio; 
        float r = Mathf.Abs(dy);               

        return q + r * 0.57735f < radius;
    }






    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 centerHex = new Vector3(0, 0, 0); 
            RevealHexAtHexCoord(centerHex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            fogAlpha = Mathf.Clamp01(fogAlpha + 0.1f);
            materialTest.SetFloat("_FogAlpha", fogAlpha);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            fogAlpha = Mathf.Clamp01(fogAlpha - 0.1f);
            materialTest.SetFloat("_FogAlpha", fogAlpha);
        }
    }
}
