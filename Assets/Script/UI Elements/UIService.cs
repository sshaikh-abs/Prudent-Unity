using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class UIService_DataClass : DataClass
{
    //public TMP_Dropdown vesselDropdown;
    //public GameObject topBar;
}

public class UIService : BaseService<UIService_DataClass>
{
    //public void SetActive(bool value)
    //{
    //    data.topBar.gameObject.SetActive(value);
    //}

    public override void Kill() { }

    public override void Update() { }

    //public void SetDropdownOptions(List<string> options)
    //{
    //    data.vesselDropdown.onValueChanged.RemoveAllListeners();
    //    data.vesselDropdown.AddOptions(options);
    //    data.vesselDropdown.value = -1;
    //}

    //public void AddDropdownOnValueChangedListener(Action<int> action)
    //{
    //    data.vesselDropdown.onValueChanged.AddListener(i => 
    //    {
    //        action?.Invoke(i);
    //    });
    //}

    //public void SetDropdownValue(int value)
    //{
    //    data.vesselDropdown.value = value;
    //}
}
