using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BiomeSpawnRate",menuName = "Map", order = 2)]
public partial class SO_CustomBiomeSpawn : ScriptableObject
{
    public struct BiomeData
    {
        public E_BiomeType.BiomeType biomeRate;
        [Range(0, 1)] public float chanceToAppear;
    }


    public E_BiomeType.BiomeType[] BiomeTypess;
    //public E_BiomeType.BiomeType[] biomerate = Array.Empty<E_BiomeType.BiomeType>();
    public BiomeData[] allBiomes = Array.Empty<BiomeData>();
    //public Dictionary<E_BiomeType.BiomeType,float> BiomeRate = new Dictionary<E_BiomeType.BiomeType, float>();
}
