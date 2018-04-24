using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class VegetableInfo {
    [System.Serializable]
    public abstract class SpecificVeggieInfo
    {
        public abstract int maxGrowth { get; }
        public abstract float chancePerc { get; }
        public abstract int coins { get; }
    }

    [System.Serializable]
    public class CarrotInfo : SpecificVeggieInfo
    {
        public override int maxGrowth { get { return 1; } }
        public override float chancePerc { get { return 0.4f; } }
        public override int coins { get { return 200; } }
    }
    [System.Serializable]
    public class TomatoInfo : SpecificVeggieInfo
    {
        public override int maxGrowth { get { return 2; } }
        public override float chancePerc { get { return 0.5f; } }
        public override int coins { get { return 500; } }
    }

    public readonly VegetablesEnum currentVeggie;    
    public int growthStage;

    public VegetableInfo(VegetablesEnum veggie, int growthStage)
    {
        currentVeggie = veggie;
        this.growthStage = growthStage;
    }

    public bool stillSeed()
    {
        return growthStage < 1;
    }

    public float veggieHeightAdd()
    {
        if(currentVeggie == VegetablesEnum.TOMATO && growthStage == 1)
        {
            return -0.0583f;
        }
        return 0;
    }

    public SpecificVeggieInfo getInfo()
    {
        //I think it's the worst possible way to do it
        switch(currentVeggie)
        {
            case VegetablesEnum.CARROT:
                return new CarrotInfo();
            case VegetablesEnum.TOMATO:
                return new TomatoInfo();
        }
        return null;
    }

    public bool isCollectable()
    {
        return getInfo().maxGrowth <= growthStage;
    }

	
}
