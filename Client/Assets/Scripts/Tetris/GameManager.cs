using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;

using System.Threading;
using System.Linq;
using GameNetwork;
using System.Text;

/* 
 * v0.0.2-r12
 * Written by Veritas83
 * www.NigelTodman.com
 * /Scripts/GameManager.cs
 */
public class GameManager : MonoBehaviour {
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    private static GameManager instance = null;
    public Int32 ScoreValue = 0;
    public Int32 LineValue = 0;
    public bool InputAllowed = true;
    public Single FallSpeed = 1f;
    public Int32 GameLevel = 0;
    public bool isGameOverNtfArrived {get; set; } = false;
    public GAME_RESULT GameResult {get; set; }
    
    public GameObject[] GameOverPanel;

    
    void Awake()
    {
        if(instance)
        {
            return;
        }
        instance = this;
    }
    // Use this for initialization


    private void Update()
    {
        if (isGameOverNtfArrived == true)
        {
            GameOverFn(GameResult);
            StartCoroutine(
                CountDownToContinue(GameObject.Find("Countdown_Text").GetComponent<Text>(), 3)
                );
            isGameOverNtfArrived = false;
        }
    }


    public void ScreenFlash()
    {
        Debug.Log("ScreenFlash() fired!");
        StartCoroutine("LerpColor");
        GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
        go.GetComponent<Camera>().backgroundColor = Color.white;
        go.GetComponent<Camera>().backgroundColor = Color.Lerp(Color.black,Color.white,2.5f);
        go.GetComponent<Camera>().backgroundColor = Color.black;
    }
    
    
    IEnumerator LerpColor()
    {
        for (int c = 0; c <= 4; c++) { 
        float t = 0f;
        float duration = 0.06f;
        float smoothness = 0.01f;
        float increment = smoothness / duration;
        Debug.Log("LerpColor() fired!");
        GameObject go = GameObject.FindGameObjectWithTag("MainCamera");
        go.GetComponent<Camera>().backgroundColor = Color.white;
        while (t <= 1)
        {
            go.GetComponent<Camera>().backgroundColor = Color.Lerp(Color.gray, Color.white, t);
            t += increment;
            Debug.Log("T Value: " + t.ToString());
            yield return new WaitForSeconds(smoothness);
        }
        go.GetComponent<Camera>().backgroundColor = Color.black;
        yield return true;
        }
    }
    
    
    
    //GameOverPanel
    public void GameOverFn(GAME_RESULT GameResult)
    {
        Debug.Log("GameOverFn() fired!");

        GameObject gameUI = GameObject.FindGameObjectWithTag("gsui");
        Instantiate(GameOverPanel[0], new Vector2(0,0),Quaternion.identity);
        GameObject gameOverPanel = GameObject.Find("GameOverPanel(Clone)");
        gameOverPanel.transform.parent = gameUI.transform;
        gameOverPanel.transform.SetPositionAndRotation(new Vector2(650, 350), Quaternion.identity);
        
        
        GameObject gameOverText = GameObject.Find("WIN_or_LOSE");

        if (GameResult == GAME_RESULT.WIN)
        {
            gameOverText.GetComponent<Text>().text = "YOU WIN!!";
        }
        else
        {
            gameOverText.GetComponent<Text>().text = "YOU LOSE...";
        }

    }
    
    

    IEnumerator CountDownToContinue(Text countdownText, float waitSecond)
    {
        if (!countdownText)
        {
            yield return null;
        }

        float accurated_time = 0;
        int remain_time = (int)waitSecond;

        
        while (accurated_time <= waitSecond)
        {
            accurated_time += Time.deltaTime;
            Debug.Log("accurated_time: "+accurated_time);
            remain_time = (int) (waitSecond - accurated_time);
            
            Debug.Log("accurated_time: "+accurated_time);
            
            countdownText.text = "CONTINUE..."+(remain_time+1);
            yield return null;

        }

        GameSceneManager.isRemoteUserInfoNeedUpdate = true;
        SceneManager.LoadScene("Game");
        yield break;

    }

}
