using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

// DISCLAIMER : This file is not optimized at all and is for test purposes only

public class LoadNN : MonoBehaviour
{
    public TMP_Dropdown list;
    public Button btn;

    float time = 0f;
    private void Update()
    {
        time += Time.deltaTime;
        if (time > 1f)
        {
            time = 0f;
            UpdateList();
        }
    }

    private void Start()
    {
        UpdateList();
    }

    string temp = "";
    void UpdateList()
    {
        string[] fileEntries = Directory.GetFiles(CarManager.i.GetPath(""));
        list.ClearOptions();
        if (fileEntries.Length == 0)
        {
            btn.interactable = false;
            list.interactable = false;
            return;
        }
        btn.interactable = true;
        list.interactable = true;
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
        for (int j = 0; j < fileEntries.Length; j++)
        {
            int i = fileEntries[j].LastIndexOf("/") + 1;
            string s = fileEntries[j].Substring(i, fileEntries[j].Length - i);
            if (!s.Substring(s.Length - 3, 3).Contains(".nn"))
                continue;
            if (list.value == j)
                temp = fileEntries[j];
            data.Add(new TMP_Dropdown.OptionData(s));
        }
        list.AddOptions(data);
    }

    public void Load()
    {
        CarManager.i.LoadFromFile(temp);
    }
}
