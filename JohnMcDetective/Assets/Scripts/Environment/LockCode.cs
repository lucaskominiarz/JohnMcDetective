using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LockCode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text[] texts;
    [SerializeField] private MapData _mapData;
    private Dictionary<NPCType, int> _code;

    private bool _isClicked;
    private bool _isOpen;

    private void Start()
    {
        foreach (NPCType npcType in _mapData.entityCount.Keys)
        {
            _code[npcType] = 0;
        }
    }

    public void SetMapData(MapData newMapDatas)
    {
        _mapData = newMapDatas;
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

    public void ChangeCombination(NPCType type, int value)
    {
        _code[type] += value;
        texts[(int)type].text = _code[type].ToString();
        if (CheckCombination())
            OpenLock();
    }

    void OpenLock()
    {
        // lancer anim
    }

    void ChangeWorld()
    {
        Camera.main.GetComponent<ScrollView>().MoveCam(_mapData.spiritStartPosition) ;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isClicked)
        {
            if (_isOpen)
            {
                ChangeWorld();
                return;
            }
            transform.DOScale(new Vector3(.8f, .8f, .8f), 05f);// changer
            // scale diminuer
            
            
        }
        else
        {
            transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 05f); // changer
            // scale augmenter
        }

        _isClicked = !_isClicked;
    }
}