﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public class PathManager : MonoBehaviour
{
    [NonSerialized]
    public UIGameSlot selectedSlot;

    public Transform slotListTop;

    public LineRenderer linePrefab;

    public GameObject startPrefab;
    public GameObject endPrefab;

    GameObject startIcon;
    GameObject endIcon;

    Path path;
    LineRenderer line;

    public Text sumText;

    bool isMouseDown = false;

    LevelManager levelManager;

    void Start()
    {
        levelManager = GameManager.instance.levelManager;

        TouchInputManager.instance.touchStart += OnTouchStart;
        TouchInputManager.instance.touchEnd += OnTouchEnd;

        sumText.text = "";
    }

    private void OnTouchStart(Touch touch)
    {
        isMouseDown = true;
    }

    private void OnTouchEnd(Touch touch)
    {
        isMouseDown = false;
    }

    public void ClearPath()
    {
        if (line != null)
        {
            Destroy(line.gameObject);
        }

        if (startIcon != null)
        {
            Destroy(startIcon);
        }

        if (endIcon != null)
        {
            Destroy(endIcon);
        }

        path = null;

        ResetAllBlanks();
        UpdateSumText();
    }

    public void OnGameSlotPressed(UIGameSlot gameSlot)
    {
        var slot = gameSlot.uiSlot.slot;

        if(path != null)
        {
            var lastPoint = path.GetLastPoint();
            if (lastPoint.slot == slot)
            {
                return;
            }
        }

        selectedSlot = gameSlot;

        ClearPath();

        if (!slot.isNumber)
        {
            return;
        }

        path = new Path(slot);

        UpdateSumText();

        line = Instantiate(linePrefab, slotListTop);
        line.positionCount += 1;
        line.SetPosition(line.positionCount - 1, gameSlot.transform.position);

        startIcon = Instantiate(startPrefab, slotListTop);
        startIcon.transform.position = gameSlot.transform.position;
    }

    public void OnGameSlotEnter(UIGameSlot gameSlot)
    {
        if (!isMouseDown)
        {
            return;
        }

        selectedSlot = gameSlot;

        if (path != null)
        {
            var slot = gameSlot.uiSlot.slot;
            var previous = path.GetPreviousPoint();

            if (slot != null && previous != null && previous.slot == slot) //retracting
            {
                var lastPoint = path.GetLastPoint();
                RemovePoint(lastPoint, lastPoint.previous);

                path.RemovePoint(lastPoint);
                line.positionCount -= 1;

                UpdateSumText();
            }
            else
            {
                if (path.AddPoint(slot))
                {
                    var lastPoint = path.GetLastPoint();
                    AddPoint(lastPoint.previous, lastPoint);

                    line.positionCount += 1;
                    line.SetPosition(line.positionCount - 1, gameSlot.transform.position);

                    UpdateSumText();
                    
                    if(path.waypoints.Count == levelManager.GetCurrentLevel().map.Values.Count)
                    {
                        Invoke("CheckSolution", .25f);
                    }                   
                }
            }
        }
    }

    void CheckSolution()
    {
        levelManager.Check();
    }

    public Path GetPath()
    {
        return path;
    }

    public void UpdateSumText()
    {
        if(path != null)
        {
            sumText.text = path.GetTotalPoints().ToString();
        }
        else
        {
            sumText.text = "";
        }

        UpdateFill();
    }

    public void UpdateFill()
    {
        var gridManager = levelManager.GetGridManager();

        if (gridManager != null)
        {            
            foreach (var slot in gridManager.GetUISlots())
            {
                var filled = false;

                if (path != null)
                {
                    filled = path.waypointsHash.Contains(slot.slot);
                }

                slot.SetFilled(filled);
            }
        }
    }

    void ResetAllBlanks()
    {
        var gridManager = levelManager.GetGridManager();

        if (gridManager != null)
        {
            foreach (var slot in gridManager.GetUISlots())
            {
                if (slot.slot.number == (int)SpecialSlot.Blank)
                {
                    slot.SetBlankNumber((int)SpecialSlot.Blank);
                }
                else if(slot.slot.number == (int)SpecialSlot.Reverse)
                {
                    slot.SetIconState(0);
                }
            }
        }
    }

    void AddPoint(PathSlot start, PathSlot end)
    {
        var gridManager = levelManager.GetGridManager();
        var slot = gridManager.GetUISlot(end.slot.position);

        if (slot != null)
        {
            if (end.slot.number == (int)SpecialSlot.Blank)
            {
                slot.SetBlankNumber(end.number);
            }
            else if(slot.slot.number == (int)SpecialSlot.Reverse)
            {
                if (end.isDescending)
                {
                    slot.SetIconState(1);
                }
                else
                {
                    slot.SetIconState(2);
                }
            }
        }
    }

    void RemovePoint(PathSlot start, PathSlot end)
    {
        var gridManager = levelManager.GetGridManager();
        var slot = gridManager.GetUISlot(start.slot.position);

        if (slot != null)
        {
            if (start.slot.number == (int)SpecialSlot.Blank)
            {
                slot.SetBlankNumber((int)SpecialSlot.Blank);
            }
            else if (slot.slot.number == (int)SpecialSlot.Reverse)
            {
                slot.SetIconState(0);
            }
        }
    }
}
