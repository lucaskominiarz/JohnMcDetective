using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LockCode : MonoBehaviour
{
    [SerializeField] private TMP_Text[] texts;
    [SerializeField] private List<NPCType> npcTypes;
    [SerializeField] private List<int> correctCounts;
    

    private int[] _selectedIndices;
    private Dictionary<NPCType, int> _targetCombination = new Dictionary<NPCType, int>();
    private bool _isOpen;
    

    private void Start()
    {
        _selectedIndices = new int[texts.Length];

        for (int i = 0; i < Mathf.Min(npcTypes.Count, correctCounts.Count); i++)
        {
            _targetCombination.TryAdd(npcTypes[i], correctCounts[i]);
        }
        
        UpdateDisplay();
        _isOpen = CheckCombination();
    }

    private bool CheckCombination()
    {
        Dictionary<NPCType, int> currentCounts = new Dictionary<NPCType, int>();

        foreach (NPCType type in npcTypes)
        {
            currentCounts.TryAdd(type, 0);
        }

        for (int i = 0; i < _selectedIndices.Length; i++)
        {
            NPCType selectedType = npcTypes[_selectedIndices[i]];
            currentCounts[selectedType]++;
        }

        foreach (var kvp in _targetCombination)
        {
            if (currentCounts[kvp.Key] != kvp.Value)
                return false;
        }

        return true;
    }

    public void NextNpcType(int slotIndex)
    {
        ChangeSlot(slotIndex, 1);
    }

    public void PreviousNpcType(int slotIndex)
    {
        ChangeSlot(slotIndex, -1);
    }

    private void ChangeSlot(int slotIndex, int direction)
    {
        if (npcTypes == null || npcTypes.Count == 0 || slotIndex < 0 || slotIndex >= texts.Length) return;

        int count = npcTypes.Count;
        _selectedIndices[slotIndex] = (_selectedIndices[slotIndex] + direction % count + count) % count;

        UpdateDisplay();

        if (CheckCombination())
            OpenLock();
        else
            _isOpen = false;
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = npcTypes[_selectedIndices[i]].ToString();
        }
    }

    private void OpenLock()
    {
        _isOpen = true;
        
    }

    private void ChangeWorld()
    {
        Camera.main.GetComponent<ScrollView>().ChangeWorld();
    }

    private bool TryOpen()
    {
        if (_isOpen)
        {
            gameObject.SetActive(false);
            ChangeWorld();
            return true;
        }
        return false;
    }

    public void CallTryOpen()
    {
        TryOpen();
    }

    public void OpenUi()
    {
        if (TryOpen())
            return;
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
}