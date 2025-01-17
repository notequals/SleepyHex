﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class LevelManager : LevelLoader
{
    public Button solveButton;
    public Button rateButton;

    public LevelSolutionViewer levelSolutionViewer;

    public Transform dialogParent;

    public SolutionPopup solutionPopupPrefab;

    public Image backgroundImage;

    public static Level currentLevel = null;
    public static string levelNameToLoad = null;

    public Text levelUIText;
    //public Text difficultyText;

    int hintsUsed = 0;
    float solveStartTime;

    void Start()
    {
        if (rateButton)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                amazonHelper = AmazonS3Helper.instance;
                rateButton.interactable = true;
            }
            else
            {
                rateButton.interactable = false;
            }
        }

        /*if (levelNameToLoad != null)
        {
            LoadLevel(levelNameToLoad);
        }*/
    }

    public override bool isValidSlot(Slot slot)
    {
        return slot.number >= 0;
    }

    public override void InitUISlot(UISlot newSlot)
    {
        var gameSlot = newSlot.gameObject.AddComponent<UIGameSlot>();
        gameSlot.uiSlot = newSlot;
        gameSlot.pathManager = GameManager.instance.pathManager;
    }

    public override void LoadLevelFeatures(Level level)
    {
        levelUIText.text = level.GetDifficultyString() + " " + level.levelID.ToString();
        //difficultyText.text = level.GetDifficultyString();
                
        SoundManager.instance.OnLevelLoaded();

        level.SetLastPlayedLevel();

        GameManager.instance.levelSelector.SetCurrentLevel(level.GetPuzzleDifficulty(), level.levelName);
        GameManager.instance.levelSelector.SetCurrentLevelIndicator();

        GameManager.instance.tutorialManager.ShowTutorial(level);

        GameManager.instance.hintManager.ResetHintsUsed();

        GameManager.instance.pathManager.SetNewLevel();

        ChangeRandomColor();

        ResetLevelStats();
    }

    void ResetLevelStats()
    {
        solveStartTime = Time.time;
        hintsUsed = 0;
    }

    void ChangeRandomColor()
    {
        if (backgroundImage == null)
        {
            return;
        }

        float h;
        float s;
        float v;

        Color.RGBToHSV(backgroundImage.color, out h, out s, out v);

        h = UnityEngine.Random.Range(0f, 1f);

        var color = Color.HSVToRGB(h, s, v);
        color.a = .4f;

        backgroundImage.color = color;
    }

    public string GetNextLevel()
    {
        var current = currentLevel;

        if (current != null)
        {
            LevelTextAsset currentLevel;
            if (LevelSelector.levelDatabase.TryGetValue(current.levelName, out currentLevel))
            {
                if (currentLevel.nextLevel != null && currentLevel.nextLevel != "")
                {
                    return currentLevel.nextLevel;
                }
            }
        }

        return null;
    }

    public void Solve()
    {
        if (level.hasSolution)
        {
            levelSolutionViewer.ShowSolution(level.solution);
        }
    }

    public void Check()
    {
        if (level.hasSolution)
        {
            var path = GameManager.instance.pathManager.GetPath();
            if (path != null)
            {
                var solution = level.solution;
                var points = path.GetTotalPoints();

                var newScore = new Score(level, points);
                newScore.time = (int)Math.Round(Time.time - solveStartTime);

                GameManager.instance.scoreManager.SetStars(newScore);

                string nextLevelName = GetNextLevel();

                if (nextLevelName != null && nextLevelName != "")
                {
                    GameManager.instance.levelSelector.SetCurrentLevel(level.GetPuzzleDifficulty(), nextLevelName);
                    GameManager.instance.levelSelector.SetCurrentLevelIndicator();
                }

                //Score.current = new Score(level, score);
                //SceneChanger.ChangeScene("Score");
            }
        }
    }
}