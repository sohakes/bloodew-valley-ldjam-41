using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Game
{

    public static Game current;
    public static bool stopped = false;
    public int currentDay;
    public int currentMoney;
    public int currentLife;
    public int currentHunger;
    public Dictionary<TerrainGrid.IntVector2, VegetableInfo> veggieGrid;
    public Dictionary<TerrainGrid.IntVector2, TileEnum> tileGrid;
    public float currentTime;
    public bool dawnMessage = false;
    public bool battleInProgress = false;
    public bool battleFinished = false;
    public int veggiesLeft = 0;
    public bool hasHermesBoot = false;
    public bool hasJunimoHut = false;
    public bool hasLightGloves = false;
    public static bool grandpaBattle = false;



    public Game()
    {

    }

    public bool sleep()
    {
        if (currentMoney < 2000 && currentDay > 1)
        {
            Manager.instance.clearMessages();
            Manager.instance.setAdviceMessage("You didn't have 2000g, you died");
            Manager.instance.die();
            return false;
        }
        if (currentDay > 1) currentMoney -= 2000;
        currentDay += 1;
        foreach(KeyValuePair<TerrainGrid.IntVector2, VegetableInfo> entry in veggieGrid)
        {
            if(tileGrid.ContainsKey(entry.Key) && tileGrid[entry.Key] == TileEnum.WATERED)
            {
                entry.Value.growthStage += 1;
                tileGrid[entry.Key] = TileEnum.TILLED;

            }
        }  
        
        battleInProgress = false;
        battleFinished = false;
        dawnMessage = false;
        currentTime = 6 * 60;
        SaveLoad.Save();
        return true;
    }

    private void initGame()
    {
        currentDay = 1;
        currentMoney = 0;
        currentLife = 10;
        currentHunger = 100;
        //grid = new Dictionary<TerrainGrid.IntVector2, Vegetable>();
        veggieGrid = new Dictionary<TerrainGrid.IntVector2, VegetableInfo>();
        tileGrid = new Dictionary<TerrainGrid.IntVector2, TileEnum>();
        current.currentTime = 6 * 60;

    }

    public static void newGame()
    {
        MonoBehaviour.print("New game created");
        current = new Game();
        current.initGame();
    }

    public void loadGame()
    {
        //grid = new Dictionary<TerrainGrid.IntVector2, Vegetable>(); //It's not serialized like the otehrs on load
    }


}