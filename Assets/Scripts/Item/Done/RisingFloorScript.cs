﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingFloorScript : MonoBehaviour
{
    // Start is called before the first frame update
    private GameController stage;
    public Vector2Int activeRange;
    public int mod;
    public bool isActive;
    void Start()
    {
        stage = GameObject.Find("GameController")
                .GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mod == 0) mod = 1;
        int val = stage.currentRhytm % mod + 1;
        isActive = (val >= activeRange.x && val <= activeRange.y);
        int x = (int)gameObject.transform.position.x, y = (int)gameObject.transform.position.y;
        GameObject goj;
        if (!isActive && (goj = stage.GetMapGameObject(x, y)) != null)
        {
            if (goj.GetComponent<BlockScript>() != null)
            {
                stage.SetMapGameObject(x, y, null);
                Destroy(goj);
            }
            else
            {
<<<<<<< HEAD
                stage.RestartStage();

||||||| c8f5e7a
                //restart 

=======
                   stage.RestartStage();
                
>>>>>>> 06114a68e0c76f6f82887b01a1281ce6c6d027f7
            }
        }

        GetComponent<Animator>().SetBool("Trigger", isActive);

    }
}
