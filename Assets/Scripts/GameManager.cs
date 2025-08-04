using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  partial class GameManager : MonoBehaviour
{

    public HexaWaveFonctCollapse wfc;
    public HexaTilePainter hexaTilePainter;
    private static GameManager _instance;


    public static GameManager Instance
    {
        get
        {
            // Si l'instance n'existe pas encore, la créer
            if (_instance == null)
            {
                // Rechercher une instance existante dans la scène
                _instance = FindObjectOfType<GameManager>();

                // Si aucune instance n'existe dans la scène, créer une nouvelle instance
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("GameManager");
                    _instance = singletonObject.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }



}
