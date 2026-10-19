using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LockCode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text[] texts;
    [SerializeField] private List<NPCType> npcTypes;
    [SerializeField] private List<int> npcNumbers;
    private Dictionary<NPCType, int> _code = new Dictionary<NPCType, int>();

    private bool _isClicked;
    private bool _isOpen;

    private void Start()
    {
        foreach (NPCType npcType in npcTypes)
        {
            _code.TryAdd(npcType, 0);
        }
    }

    bool CheckCombination()
    {
        for (int i = 0; i < npcTypes.Count; i++)
        {
            if (_code[npcTypes[i]] != npcNumbers[i])
                return false;
        }
        return true;
    }

    public void ChangeCombination(NPCType type, int value)
    {
        int index = 0;
        for (int i = 0; i < npcTypes.Count; i++)
        {
            if (npcTypes[i] == type)
            {
                index = i;
            }
        }
        _code[type] += value;
        texts[index].text = _code[type].ToString();
        if (CheckCombination())
            OpenLock();
        else
            _isOpen = false;
    }

    void OpenLock()
    {
        _isOpen = true;
        // lancer anim
    }

    void ChangeWorld()
    {
        Camera.main.GetComponent<ScrollView>().ChangeWorld();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isOpen) // a enlever
        {
            ChangeWorld();
            return;
        }
        /*
        if (_isClicked)
        {
            if (_isOpen)
            {
                ChangeWorld();
                return;
            }
            // scale diminuer
            
            
        }
        else
        {
            // scale augmenter
        }
    */
        _isClicked = !_isClicked;
    }
}