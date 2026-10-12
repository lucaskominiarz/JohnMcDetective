using System;
using System.Collections.Generic;
using UnityEngine;

public class LockCode : MonoBehaviour
{
    private MapData _mapData;
    private Dictionary<NPCType, int> _code;

    private void Start()
    {
        foreach (NPCType npcType in _mapData.entityCount.Keys)
        {
            _code[npcType] = 0;
        }
    }

    bool CheckCombination()
    {
        foreach (NPCType npcType in _mapData.entityCount.Keys)
        {
            if (_code[npcType] != _mapData.entityCount[npcType])
                return false;
        }
        return true;
    }

    void ChangeCombination(NPCType type, int value)
    {
        _code[type] = value;
        if (CheckCombination())
            OpenLock();
    }

    void OpenLock()
    {
        // lancer anim
    }

    void ChangeWorld()
    {
        Camera.main.GetComponent<ScrollView>().MoveCam(_mapData.spiritStartPosition);
    }
}
