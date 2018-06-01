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

    [NonSerialized]
    public LevelTextAsset highestLevelPlayed;

    void Start()
    {
        Initialize();

        Invoke("PlayTutorial", 0);
    }

    void PlayTutorial()
    {
        if (PlayerPrefs.GetInt("PlayedTutorial", 0) == 0)
        {
            LoadLevel("tutorial 1");
        }
    }

    void OnEnable()
    {
        SoundManager.instance.PlayHomeMusic();
    }

    public void Initialize()
    {
        difficultyFilter = PlayerPrefs.GetInt("difficultyFilter", -1);

        //selectorPanel = GetComponent<DialogWindow>();

        if (difficultyFilter != -1)
        {
            //dialogueGroup.SetActive("LevelSelect");
            difficultyPanel.Close();
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
        foreach(var level in levelDatabase.levels)
        {
            LevelSelector.AddLevel(level);
        }

        LevelSelector.isLoaded = true;
    }

    public void SetDifficultyFilter(int difficulty)
    {
        difficultyFilter = difficulty;
        PlayerPrefs.SetInt("difficultyFilter", difficultyFilter);

        highestLevelPlayed = null;

        RefreshList();
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
            GameManager.instance.LoadLevel("tutorial 1");
        }
        else
        {
            GameManager.instance.LoadLevel(levelName);
        }     
               
        //selectorPanel.Close();
        dialogueGroup.SetActive("Game");
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

    public void SetCurrentLevel()
    {
        if(currentLevelIndicator != null)
        {
            Destroy(currentLevelIndicator);
        }

        //string lastPlayedLevel = Level.GetLastPlayedLevel();

        if (highestLevelPlayed != null)
        {
            string highestLevel = highestLevelPlayed.levelName;

            LevelSelectButton button;
            if (buttonDatabase.TryGetValue(highestLevel, out button))
            {
                currentLevelIndicator = Instantiate(currentLevelIndicatorPrefab, button.transform);

                SnapTo(button.GetComponent<RectTransform>());
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

        Invoke("SetCurrentLevel", .05f);
    }    
}
