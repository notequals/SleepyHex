﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class Score
{
    public static Score current = null;

    public Level level;
    public int points;

    public int time = 0;

    public int hintsUsed = 0;

    public int stars;

    public int playCount = 0;

    public Score(Level level, int points)
    {
        this.level = level;
        this.points = points;

        GetStars();
    }

    public static string GetStarsPrefKey(string levelName)
    {
        return "Level_" + levelName + "_Stars";
    }

    public static int GetStoredStars(string levelName)
    {
        return PlayerPrefs.GetInt(GetStarsPrefKey(levelName), 0);
    }

    public int GetStoredStars()
    {
        var storedStars = PlayerPrefs.GetInt(GetStarsPrefKey(level.levelName), 0);

        return storedStars;
    }

    public void SetStoredStars()
    {
        var storedStars = GetStoredStars();

        if (stars > storedStars)
        {
            PlayerPrefs.SetInt(GetStarsPrefKey(level.levelName), stars);
        }
    }

    public void IncrementPlayCount()
    {
        string key = "Level_" + level.levelName + "_PlayCount";

        var playCount = PlayerPrefs.GetInt(key, 0) + 1;

        PlayerPrefs.SetInt(key, playCount);

        this.playCount = playCount;
    }

    public void GetStars()
    {
        if (level.hasSolution)
        {
            var worst = level.solution.worstScore;
            var best = level.solution.bestScore;

            if(points == best)
            {
                stars = 3;
            }
            else if(points == worst)
            {
                stars = 1;
            }
            else
            {
                var percent = (float)(points - worst) / (best - worst);

                stars = percent >= .5f ? 2 : 1;
            }
        }
    }
}
