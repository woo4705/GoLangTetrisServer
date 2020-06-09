using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNetwork;

/* 
 * v0.0.2-r12
 * Written by Veritas83
 * www.NigelTodman.com
 * /Scripts/Spawner.cs
 */

public class Spawner : MonoBehaviour {

    public static bool isGameStart { get; set; } = false;
    public static bool isGameRunning { get; set; } = false;
    
    public static GAME_RESULT GameResult { get; set; }

    
    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (isGameStart == true)
        {
            isGameRunning = true;
            isGameStart = false;
            spawnNext();
        }
	}

    // Groups (of Blocks that fall)
    public Group[] groups;
    public ShadowGroup[] shadowgroups;


    public void spawnNext()
    {
        if (isGameRunning != true)
        {
            return;
        }
        // Random Index
         int i = UnityEngine.Random.Range(0, groups.Length);
        // Spawn Group at current Position
        Group spawned = Instantiate(groups[i], transform.position, Quaternion.identity);
    //    ShadowGrid.SpawnShadow(shadowgroups[i],spawned);
    }

    
}
