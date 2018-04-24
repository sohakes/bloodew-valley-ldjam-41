using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TileInfo
{
    //Probably not gonna use honestly.

    public readonly VegetablesEnum currentVeggie;
    public int growthStage;

    public TileInfo(TileEnum tileType, int growthStage)
    {
        //currentVeggie = veggie;
        //this.growthStage = growthStage;
    }


}
