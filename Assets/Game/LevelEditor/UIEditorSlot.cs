﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class UIEditorSlot : MonoBehaviour
{
    public UISlot uiSlot;

    public Button button;

    public LevelEditor levelEditor;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnSlotPressed);
    }

    public void OnSlotPressed()
    {
        if (levelEditor)
        {
            levelEditor.OnEditorSlotPressed(this);
        }
    }
}
