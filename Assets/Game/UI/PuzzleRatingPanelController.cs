﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class PuzzleRatingPanelController : MonoBehaviour
{
    LevelManager levelManager;
    
    void Start()
    {
        levelManager = GameManager.instance.levelManager;
    }

    public void SetLevelDifficulty(float difficulty)
    {
        var level = levelManager.GetCurrentLevel();

        if(level != null)
        {
            level.difficulty = (float)Math.Round(difficulty, 2);

            levelManager.Save(false, false);
        }
    }
}
