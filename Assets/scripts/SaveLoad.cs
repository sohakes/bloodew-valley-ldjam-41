using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//taken from https://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934

public static class SaveLoad
{

    public static Game savedGames;

    //it's static so we can call it from anywhere
    public static void Save()
    {
        savedGames = Game.current;
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd"); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
        {
            MonoBehaviour.print("Loading from: " + Application.persistentDataPath + "/savedGames.gd");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            SaveLoad.savedGames = (Game)bf.Deserialize(file);
            Game.current = savedGames;
            file.Close();
        }
    }

    public static bool canLoad()
    {
        return File.Exists(Application.persistentDataPath + "/savedGames.gd");
    }
}
