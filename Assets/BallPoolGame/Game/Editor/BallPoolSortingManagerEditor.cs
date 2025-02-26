using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BallPool;

/// <summary>
/// The balls sorting manager, select the game object with component BallsSortingManager and sort balls.
/// </summary>
public class BallPoolSortingManagerEditor : EditorWindow
{
    [MenuItem ("Window/Ball Pool/Sort 8-balls")]
    static void SortEightBallsRule () 
    {
        GameObject activeGameObject = Selection.activeGameObject;
        if (activeGameObject)
        {
            BallPoolBallsSortingManager sortingManager = activeGameObject.GetComponent<BallPoolBallsSortingManager>();
            if (sortingManager != null)
            {
                sortingManager.SortEightBalls();
            }
            else
            {
                Debug.LogWarning("Please select the gameobject with component CmBallsSortingManager");
            }
        }
        else
        {
            Debug.LogWarning("Please select the gameobject with component CmBallsSortingManager");
        }
    }
    
    [MenuItem ("Window/Ball Pool/Sort 9-balls")]
    static void SortNineBallsRule() 
    {
        GameObject activeGameObject = Selection.activeGameObject;
        if (activeGameObject)
        {
            BallPoolBallsSortingManager sortingManager = activeGameObject.GetComponent<BallPoolBallsSortingManager>();
            if (sortingManager != null)
            {
                sortingManager.SortNineBalls();
            }
            else
            {
                Debug.LogWarning("Please select the gameobject with component CmBallsSortingManager");
            }
        }
        else
        {
            Debug.LogWarning("Please select the gameobject with component CmBallsSortingManager");
        }
    }
}
