﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public DialogWindow rateMeWindow;

    GlobalStatsManager statsManager;

    void Start()
    {
        statsManager = GameManager.instance.globalStatsManager;
    }

    public void ShowRateMeWindow()
    {
        if (statsManager.GetUserRatedGame()) //if user already rated the game
        {
            return;
        }

        var playCount = statsManager.GetGlobalPlayCount();

        if (playCount >= 25 && playCount % 10 == 0) //every 10 levels
        {
            rateMeWindow.Show();            
        }
    }

    public void UserPressedRateButton()
    {
        statsManager.SetUserRatedGame();

        GameManager.instance.hintManager.AddHint(10); //add 10 free hints
    }
}