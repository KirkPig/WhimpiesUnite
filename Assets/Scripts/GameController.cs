﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject levelComplete;
    public string stageName;
    public TextAsset csvFile;
    public TextAsset spawnFile;
    public TextAsset buttonFile;
    public EndBatScript endBat;
    public EndPlantScript endPlant;
    public EndSlimeScript endSlime;
    public GameObject MainCamera;
    public int gateStatus;
    public int key;
    public int currentRhytm;
    public float drum;
    public const float drumTime = 1;
    public CharacterManager characterManager;
    public GameObject BatVision;
    private GameObject[][] Map = new GameObject[32][];
    private GameObject Bat;
    private List<Vector3> bat_vision;
    public bool isEnd;

    private void Awake()
    {
        gateStatus = 0;
        for (int i = 0;i< 32;i++)
        {
            Map[i] = new GameObject[32];
        }
        characterManager = new CharacterManager();
    }

    // Start is called before the first frame update
    void Start()
    {


        /*
         * 
         */

        


    }

    public void RestartStage()
    {
        GameManager.instance.RestartGame(stageName);
    }

    public void BackButtonClicked()
    {
        GameManager.instance.BackMainMenu();
    }

    public void LoadGame()
    {
        GenerateMap();
        Spawn();
        GenerateButton();


        drum = 0f;
        key = 0;
    }

    private void GenerateButton()
    {
        string[,] A = ReadCSV.ReadCSVFileNoReverse(buttonFile.text);
        int n = (int) float.Parse(A[0, 0]), it = 1;
        for(int i = 0; i < n; i++)
        {
            Debug.Log("i=" + i.ToString());
            int x = int.Parse(A[it, 0]), y = int.Parse(A[it, 1]);
            GameObject button = Instantiate(TilePrefabs.ButtonPrefab, new Vector3((float)x, (float)y, transform.position.z), Quaternion.identity);
            button.name = "Button" + x.ToString() + "_" + y.ToString();
            it++;
            Debug.Log(button.name);
            //get isAlarm
            int isAlarm = int.Parse( A[it, 0] );
            ButtonScript buttonScript = button.GetComponent<ButtonScript>();
            if(isAlarm == 1)
            {
                buttonScript.isAlarm = true;
                it++;
                buttonScript.alarmDestination = new Vector3(float.Parse(A[it, 0]), float.Parse(A[it, 1]));
            }
            it++;
            buttonScript.conveyer = new List<Vector2Int>();
            int conveyerLength = int.Parse(A[it, 0]);
            Debug.Log("conveyer length:" + conveyerLength.ToString());
            it++;
            for(int j = 0; j < conveyerLength; j ++, it ++)
            {
                int begx = int.Parse(A[it, 0]), begy = int.Parse(A[it, 1]);
                Debug.Log("BegX:" + begx.ToString() + "BegY:" + begy.ToString());
                buttonScript.conveyer.Add(new Vector2Int(begx, begy));
                int endx = int.Parse(A[it, 2]), endy = int.Parse(A[it, 3]);
                Debug.Log("endX:" + endx.ToString() + "endY:" + endy.ToString());
                buttonScript.conveyer.Add(new Vector2Int(endx, endy));
                int dx = (endx - begx) == 0 ? 0 : (endx - begx < 0 ? -1 : 1);
                int dy = (endy - begy) == 0 ? 0 : (endy - begy < 0 ? -1 : 1);
                Debug.Log("dx:" + dx.ToString() + "dy:" + dy.ToString());
                GameObject conveyerPrefab;
                if (dx == 1) conveyerPrefab = TilePrefabs.ConveyerRight;
                else if (dx == -1) conveyerPrefab = TilePrefabs.ConveyerLeft;
                else if (dy == 1) conveyerPrefab = TilePrefabs.ConveyerUp;
                else conveyerPrefab = TilePrefabs.ConveyerDown;
                for (;begx != endx + dx|| begy != endy + dy; begx += dx, begy += dy)
                {
                   Instantiate(conveyerPrefab, new Vector3((float) begx, (float) begy, transform.position.z), Quaternion.identity);
                }
            }
            buttonScript.cooldownTime = float.Parse(A[it, 0]);
            it++;
        }
    }
    private void Spawn()
    {
        string[,] A = ReadCSV.ReadCSVFileNoReverse(spawnFile.text);

        // Debug.Log(A[0, 0]);


        for(int i = 0; i < 6; i += 2)
        {
            float pos_x =  float.Parse(A[0, i]), pos_y = float.Parse(A[0, i +1]);
            if (pos_x == -1f) continue;
            if (i == 0)
            {
                GameObject Plant = Instantiate(TilePrefabs.PlantPrefab, new Vector3(pos_x, pos_y, transform.position.z), Quaternion.identity);
                Map[(int)pos_x][(int)pos_y] = Plant;
                characterManager.setCharacterPlant(Plant);
            }
            else if(i == 2)
            {
                Bat = Instantiate(TilePrefabs.BatPrefab, new Vector3(pos_x, pos_y, transform.position.z), Quaternion.identity);
                Map[(int)pos_x][(int)pos_y] = Bat;
                characterManager.setCharacterBat(Bat);
            }
            else
            {
                GameObject Slime = Instantiate(TilePrefabs.SlimePrefab, new Vector3(pos_x, pos_y, transform.position.z), Quaternion.identity);
                Map[(int)pos_x][(int)pos_y] = Slime;
                characterManager.setCharacterSlime(Slime);
            }
        }
    }
    private void GenerateMap()
    {

        Instantiate(TilePrefabs.FloorPrefab, new Vector3(-0.5f, -0.5f, 0), Quaternion.identity) ;
        string[,] A = ReadCSV.ReadCSVFile(csvFile.text);

        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                int tile_i_j = (int)Mathf.Floor(float.Parse(A[i, j]));
                switch (tile_i_j)
                {
                    case 1:
                        GameObject newWater = Instantiate(TilePrefabs.WaterPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newWater.name = "Water" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newWater;
                        break;
                    case 2:
                        GameObject newLava = Instantiate(TilePrefabs.LavaPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newLava.name = "Lava" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newLava;
                        break;
                    case 3:
                        GameObject newWall = Instantiate(TilePrefabs.WallPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newWall.name = "Wall" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newWall;
                        break;
                    case 7:
                        GameObject newKey = Instantiate(TilePrefabs.KeyPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newKey.name = "Key" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newKey;
                        break;
                    case 8:
                        if (A[i, j] == "8.1")
                        {
                            GameObject newDoor1 = Instantiate(TilePrefabs.Door1Prefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newDoor1.name = "newDoor1 " + i.ToString() + "_" + j.ToString();
                            Map[i][j] = newDoor1;
                            break;
                        }
                        if (A[i, j] == "8.2")
                        {
                            GameObject newDoor2 = Instantiate(TilePrefabs.Door2Prefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newDoor2.name = "newDoor1 " + i.ToString() + "_" + j.ToString();
                            Map[i][j] = newDoor2;
                            break;
                        }
                        if (A[i, j] == "8.3")
                        {
                            GameObject newDoor3 = Instantiate(TilePrefabs.Door3Prefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newDoor3.name = "newDoor1 " + i.ToString() + "_" + j.ToString();
                            Map[i][j] = newDoor3;
                            break;
                        }
                        if (A[i, j] == "8.4")
                        {
                            GameObject newDoor4 = Instantiate(TilePrefabs.Door4Prefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newDoor4.name = "newDoor1 " + i.ToString() + "_" + j.ToString();
                            Map[i][j] = newDoor4;
                            break;
                        }
                        if (A[i, j] == "8.5")
                        {
                            GameObject newDoor5 = Instantiate(TilePrefabs.Door5Prefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newDoor5.name = "newDoor1 " + i.ToString() + "_" + j.ToString();
                            Map[i][j] = newDoor5;
                            break;
                        }
                        break;
                    case 9:
                        GameObject newRisingFloor1 = Instantiate(TilePrefabs.RisingFloor, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newRisingFloor1.name = "newRisingFloor1" + i.ToString() + "_" + j.ToString();
                        newRisingFloor1.GetComponent<RisingFloorScript>().mod = 6;
                        string s = A[i, j].Split("."[0])[1];
                        if (s.Length == 2) newRisingFloor1.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(s[0] - '0', s[1] - '0');
                        else newRisingFloor1.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(s[0] - '0', s[0] - '0');
                        break;
                    case 10:
                        GameObject newRisingFloor2 = Instantiate(TilePrefabs.RisingFloor, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newRisingFloor2.name = "newRisingFloor1" + i.ToString() + "_" + j.ToString();
                        newRisingFloor2.GetComponent<RisingFloorScript>().mod = 3;
                        string ss = A[i, j].Split("."[0])[1];
                        if (ss.Length == 2) newRisingFloor2.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(ss[0] - '0', ss[1] - '0');
                        else newRisingFloor2.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(ss[0] - '0', ss[0] - '0');
                        break;
                    case 11:
                        GameObject newRisingFloor3 = Instantiate(TilePrefabs.RisingFloor, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newRisingFloor3.name = "newRisingFloor1" + i.ToString() + "_" + j.ToString();
                        newRisingFloor3.GetComponent<RisingFloorScript>().mod = 2;
                        string sss = A[i, j].Split("."[0])[1];
                        if (sss.Length == 2) newRisingFloor3.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(sss[0] - '0', sss[1] - '0');
                        else newRisingFloor3.GetComponent<RisingFloorScript>().activeRange = new Vector2Int(sss[0] - '0', sss[0] - '0');
                        break;
                    case 12:
                        GameObject newBlock = Instantiate(TilePrefabs.BlockPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newBlock.name = "Block" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newBlock;
                        break;
                    case 13:
                        GameObject newAlarm = Instantiate(TilePrefabs.AlarmPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newAlarm.name = "Alarm" + i.ToString() + "_" + j.ToString();
                        Map[i][j] = newAlarm;
                        break;
                    case 14:
                        GameObject newOpenGate = Instantiate(TilePrefabs.GatePrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newOpenGate.name = "Gate" + i.ToString() + "_" + j.ToString();
                        newOpenGate.GetComponent<GateScript>().state = false;

                        Map[i][j] = newOpenGate;
                        break;
                    case 15:
                        GameObject newCloseGate = Instantiate(TilePrefabs.GatePrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                        newCloseGate.name = "Gate" + i.ToString() + "_" + j.ToString();
                        newCloseGate.GetComponent<GateScript>().state = true;
                        Map[i][j] = newCloseGate;
                        break;
                    case 16:
                        if (A[i, j] == "16.2")
                        {

                            GameObject newClock = Instantiate(TilePrefabs.ClockPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newClock.name = "Clock" + i.ToString() + "_" + j.ToString();
                            newClock.GetComponent<ClockScript>().maxTime = 2;
                            Map[i][j] = newClock;

                        }
                        else if (A[i, j] == "16.3")
                        {

                            GameObject newClock = Instantiate(TilePrefabs.ClockPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newClock.name = "Clock" + i.ToString() + "_" + j.ToString();
                            newClock.GetComponent<ClockScript>().maxTime = 3;
                            Map[i][j] = newClock;

                        }
                        break;

                    case 17:
                        if (A[i, j] == "17.1")
                        {
                            GameObject newEndBat = Instantiate(TilePrefabs.EndBatPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newEndBat.name = "EndBat" + i.ToString() + "_" + j.ToString();
                            endBat = newEndBat.GetComponent<EndBatScript>();
                            // Map[i][j] = newEndBat;
                        }
                        else if (A[i, j] == "17.2")
                        {
                            GameObject newEndPlant = Instantiate(TilePrefabs.EndPlantPrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newEndPlant.name = "EndPlant" + i.ToString() + "_" + j.ToString();
                            endPlant = newEndPlant.GetComponent<EndPlantScript>();
                            // Map[i][j] = newEndPlant;
                        }
                        else if (A[i, j] == "17.3")
                        {
                            GameObject newEndSlime = Instantiate(TilePrefabs.EndSlimePrefab, new Vector3(i, j, transform.position.z), Quaternion.identity);
                            newEndSlime.name = "EndSlime" + i.ToString() + "_" + j.ToString();
                            endSlime = newEndSlime.GetComponent<EndSlimeScript>();
                            // Map[i][j] = newEndSlime;
                        }
                        break;

                }
            }
        }
    }

    public void AddGate(int val)
    {
        gateStatus += val;
    }

    public GameObject GetMapGameObject(int i, int j)
    {
        //Debug.Log("request" + i.ToString() + j.ToString());
        return Map[i][j];
    }
    public void SetMapGameObject(int i, int j, GameObject smth)
    {
        Map[i][j] = smth;
    }

    // Update is called once per frame
    void Update()
    {
        
        Transform t = characterManager.selectedCharacter.transform;
        MainCamera.transform.position = new Vector3(t.position.x, t.position.y, MainCamera.transform.position.z);
        drum += Time.deltaTime;
        if (drum > drumTime)
        {
            currentRhytm = (currentRhytm + 1) % 6;
            drum = 0;
            if(characterManager.selectedCharacter == Bat){
                getNoise();
            }
        }
        if (characterManager.selectedCharacter == Bat)
        {
            BatVision.SetActive(true);
        }
        else
        {
            BatVision.SetActive(false);
        }
        isEnd = ((endBat == null || endBat.isEndBat) && (endPlant == null || endPlant.isEndPlant) && (endSlime == null || endSlime.isEndSlime));

        levelComplete.SetActive(isEnd);
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartStage();
        }

    }

    void getNoise(){
        bat_vision = new List<Vector3>();
        for (int i = 0;i< 32;i++)
        {
            for (int j = 0;j< 32;j++)
            {
                if(Map[i][j]!=null){
                    var alarm = Map[i][j].GetComponent<AlarmScript>();
                    if(alarm != null && alarm.pressed)
                    {
                        bat_vision.Add(Map[i][j].transform.position);
                    }
                    var clock = Map[i][j].GetComponent<ClockScript>();
                    if(clock != null && clock.time == 1)
                    {
                        bat_vision.Add(Map[i][j].transform.position);
                    }
                    //var plant = Map[i][j].GetComponent<PlantCharacter>();
                }
            }
        }
        Bat.GetComponent<BatCharacter>().getVision(bat_vision);
    }
}
