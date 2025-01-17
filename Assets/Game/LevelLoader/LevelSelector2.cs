﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Amazon.S3.Model;

public class LevelSelector2 : MonoBehaviour
{
    public LevelDatabase levelDatabase;

    public enum SortType
    {
        DateModified,
        Name,
        Difficulty,
    }

    public Transform levelList;
    public Transform levelListParent;

    public LevelSelectButton levelSelectionButton;

    public LevelLoader levelLoader;

    public SortType sortType = SortType.Difficulty;

    [NonSerialized]
    public int difficultyFilter = -1;

    public DialogWindow difficultyPanel;

    public DialogueGroup dialogueGroup;

    public Dictionary<string, LevelSelectButton> buttonDatabase;

    public GameObject currentLevelIndicatorPrefab;
    GameObject currentLevelIndicator;

    //[NonSerialized]
    //public LevelTextAsset highestLevelPlayed;

    void Start()
    {
        Initialize();

        Invoke("PlayTutorial", 0);
    }

    void PlayTutorial()
    {
        if (PlayerPrefs.GetInt("PlayedTutorial", 0) == 0)
        {
            PlayerPrefs.SetInt("PlayedTutorial", 1);
            LoadLevel("tutorial 1");
        }
    }

    void OnEnable()
    {
        SoundManager.instance.PlayHomeMusic();

        Invoke("CheckDailyBonus", .01f);
    }

    void CheckDailyBonus()
    {
        if (GameManager.instance && GameManager.instance.dailyBonusManager != null)
        {
            GameManager.instance.dailyBonusManager.CheckDailyBonus();
        }
    }

    public void Initialize()
    {
        difficultyFilter = PlayerPrefs.GetInt("difficultyFilter", 5);

        //selectorPanel = GetComponent<DialogWindow>();
        
        dialogueGroup.ShowWindow("LevelSelect");
        dialogueGroup.ShowWindow("LevelDifficulty");


        if (difficultyFilter != -1)
        {
            //dialogueGroup.SetActive("LevelSelect");            

            //dialogueGroup.CloseWindow("LevelDifficulty","LevelSelect");

            dialogueGroup.CloseWindow();
            //difficultyPanel.Close();
        }

        LoadLevels();

        //WriteLevelID();
    }

    void LoadLevels()
    {
        if (!LevelSelector.isLoaded)
        {
            //LoadLevelLocal();

            LoadLevelByDatabase(levelDatabase);
            RefreshList();

            LevelSelector.isLoaded = true;
        }
        else
        {
            RefreshList();
        }
    }

    public static void LoadLevelByDatabase(LevelDatabase levelDatabase)
    {
        foreach (var group in levelDatabase.difficultyGroups)
        {
            foreach (var level in group.levels)
            {
                LevelSelector.AddLevel(level);
            }
        }

        LevelSelector.isLoaded = true;
    }

    public string GetCurrentLevel(PuzzleDifficulty difficulty)
    {
        string keyString = "CurrentLevel_" + difficulty.ToString();

        return PlayerPrefs.GetString(keyString, null);
    }

    public void SetCurrentLevel(PuzzleDifficulty difficulty, string levelName)
    {
        string keyString = "CurrentLevel_" + difficulty.ToString();
        PlayerPrefs.SetString(keyString, levelName);
    }

    public void SetDifficultyFilter(int difficulty)
    {
        difficultyFilter = difficulty;
        PlayerPrefs.SetInt("difficultyFilter", difficultyFilter);
        
        RefreshList();
        //SetCurrentLevel();
    }

    public void SetSortType(int sortType)
    {
        this.sortType = (SortType)sortType;
        RefreshList();
    }

    public void LoadLevel(string levelName)
    {
        if (PlayerPrefs.GetInt("PlayedTutorial", 0) == 0)
        {
            PlayerPrefs.SetInt("PlayedTutorial", 1);
            GameManager.instance.LoadLevel("tutorial 1");
        }
        else
        {
            GameManager.instance.LoadLevel(levelName);
        }

        dialogueGroup.SetActive("Game");
        //selectorPanel.Close();
    }

    public void SetButtonStars(Score score)
    {
        var storedStars = score.GetStoredStars();

        if (score.stars > storedStars)
        {
            LevelSelectButton button;

            if (buttonDatabase.TryGetValue(score.level.levelName, out button))
            {
                button.SetStars(score.stars);
            }
        }
    }

    public void SetCurrentLevelIndicator()
    {
        if (currentLevelIndicator != null)
        {
            Destroy(currentLevelIndicator);
        }

        string currentLevel = GetCurrentLevel((PuzzleDifficulty)difficultyFilter);

        if (currentLevel != null && currentLevel != "")
        {
            LevelSelectButton button;
            if (buttonDatabase.TryGetValue(currentLevel, out button))
            {
                currentLevelIndicator = Instantiate(currentLevelIndicatorPrefab, button.transform);

                SnapTo(button.GetComponent<RectTransform>());
            }
        }
        else
        {
            var firstLevel = LevelSelector.levelListDatabase.FirstOrDefault();

            if(firstLevel != null)
            {
                SetCurrentLevel((PuzzleDifficulty)difficultyFilter, firstLevel.levelName);

                LevelSelectButton button;
                if (buttonDatabase.TryGetValue(firstLevel.levelName, out button))
                {
                    currentLevelIndicator = Instantiate(currentLevelIndicatorPrefab, button.transform);

                    SnapTo(button.GetComponent<RectTransform>());
                }
            }
        }
    }

    void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        var contentPanel = levelList.GetComponent<RectTransform>();
        var scrollRect = levelListParent.GetComponent<ScrollRect>();

        var vector = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);

        vector.x = 0;
        vector.y -= target.rect.height / 2;

        contentPanel.anchoredPosition = vector;
    }

    public void RefreshList()
    {
        foreach (Transform child in levelList)
        {
            Destroy(child.gameObject);
        }

        IEnumerable<LevelTextAsset> filteredLevels =
            difficultyFilter > 0 ? LevelSelector.levelDatabase.Values.Where(level => Math.Floor(level.difficulty) == difficultyFilter)
                                        : new List<LevelTextAsset>();

        LevelSelector.levelListDatabase = filteredLevels.OrderBy(level => level.difficulty).ThenBy(level => level.levelID).ToList();

        Debug.Log("Num levels: " + LevelSelector.levelListDatabase.Count());

        buttonDatabase = new Dictionary<string, LevelSelectButton>();

        foreach (var level in LevelSelector.levelListDatabase)
        {
            if (level != null)
            {
                var newButton = Instantiate(levelSelectionButton, levelList);
                newButton.SetButton(level, this);

                buttonDatabase.Add(level.levelName, newButton);
            }
        }

        Invoke("SetCurrentLevelIndicator", .05f);
    }
}
