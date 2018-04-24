using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Most of the code gotten from https://unityshark.blogspot.com.br/2016/08/draw-grid-on-terrain-in-unity.html


public class TerrainGrid : MonoBehaviour
{
    private float cellSize = 1;
    public GameObject tilledCell;
    public Material wateredMaterial;
    public Material tilledMaterial;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float yOffset = 0.5f;
    public Material cellMaterialValid;
    public Material cellMaterialInvalid;
    public GameObject vegetables;
    public Vegetable vegetable;
    public GameObject thePlayer;
    public float maxDistance = 1;

    private GameObject[] _cells;
    private float[] _heights;
    private int gridX;
    private int gridY;
    private Game current;
    private Dictionary<TerrainGrid.IntVector2, Vegetable> objectVeggieGrid;
    private Dictionary<TerrainGrid.IntVector2, GameObject> objectTileGrid;

    [System.Serializable]
    public class IntVector2
    {
        public int x;
        public int y;

        public IntVector2(int ax, int ay)
        {
            x = ax;
            y = ay;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            IntVector2 p = (IntVector2)obj;
            return (x == p.x) && (y == p.y);
        }
        public override int GetHashCode()
        {
            return x ^ y;
        }
    }



    void Start()
    {
        objectVeggieGrid = new Dictionary<TerrainGrid.IntVector2, Vegetable>();
        objectTileGrid = new Dictionary<TerrainGrid.IntVector2, GameObject>();
        current = Game.current;
        _cells = new GameObject[gridHeight * gridWidth];
        _heights = new float[(gridHeight + 1) * (gridWidth + 1)];
        //DontDestroyOnLoad(vegetables);
        Bounds b = tilledCell.GetComponentInChildren<Renderer>().bounds;
        cellSize = b.max.x - b.min.x;

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                _cells[z * gridWidth + x] = CreateChild();
            }
        }

        //MonoBehaviour.print(Game.current.veggieGrid.Count);

        //Now it reinserts everything in the right place. Hopefully
        foreach(KeyValuePair<IntVector2, VegetableInfo> entry in Game.current.veggieGrid)
        {
            //Actually just the last parameter was ok but whatever now
            createVegetable(entry.Key, entry.Value);
        }
        foreach (KeyValuePair<IntVector2, TileEnum> entry in Game.current.tileGrid)
        {
            createTile(entry.Key, entry.Value);
        }
    }

    void Update()
    {
        if (Game.current.currentLife <= 0)
        {
            this.enabled = false;
        }
        UpdateSize();
        UpdatePosition();
        UpdateHeights();
        UpdateCells();
        UpdateInput();
        UpdateSpawnEvilVeggies();
    }

    void UpdateSpawnEvilVeggies()
    {
        if (Game.current.battleInProgress == true || Game.current.currentTime < 18 * 60 
            || Game.current.battleFinished || Game.current.currentDay == 5) return;
        Game.current.battleInProgress = true;
        Manager.instance.battleStart();
        List<Vegetable> carrots = new List<Vegetable>();
        List<Vegetable> tomatoes = new List<Vegetable>();
        foreach (KeyValuePair<IntVector2, Vegetable> entry in objectVeggieGrid)
        {
            if (Game.current.veggieGrid[entry.Key].currentVeggie == VegetablesEnum.CARROT)
            {
                carrots.Add(entry.Value);
            }
            else
            {
                tomatoes.Add(entry.Value);
            }
        }
        float leftTomatoes = tomatoes.Count;
        float leftCarrots = carrots.Count;
        float tomatoesNeeded =  Mathf.Ceil(leftTomatoes * new VegetableInfo.TomatoInfo().chancePerc);
        float carrotsNeeded = Mathf.Ceil(leftCarrots * new VegetableInfo.CarrotInfo().chancePerc);
        //MonoBehaviour.print("leftt, leftc, neededt, neededc " + leftTomatoes + " " + leftCarrots + " " + tomatoesNeeded + " " + carrotsNeeded);
        foreach(Vegetable tomato in tomatoes)
        {
            if (tomatoesNeeded == 0) break;
            //MonoBehaviour.print("smaller than " + (tomatoesNeeded / leftTomatoes));
            if(Random.Range(0f,1f) <= (tomatoesNeeded / leftTomatoes))
            {
                tomatoesNeeded--;
                EvilCarrot eviltomato = Object.Instantiate(Manager.instance.tomato.evilVersion);
                Vector3 pos = tomato.transform.position;
                pos.y = 0;
                eviltomato.transform.position = pos;
                Game.current.veggiesLeft++;
            }
            leftTomatoes--;
            //MonoBehaviour.print("leftt, leftc, neededt, neededc " + leftTomatoes + " " + leftCarrots + " " + tomatoesNeeded + " " + carrotsNeeded);

        }

        foreach (Vegetable carrot in carrots)
        {
            if (carrotsNeeded == 0) break;
            //MonoBehaviour.print("smaller than " + (carrotsNeeded / leftCarrots));
            if (Random.Range(0, 1) <= (carrotsNeeded / leftCarrots))
            {
                carrotsNeeded--;
                EvilCarrot evilcarrot = Object.Instantiate(Manager.instance.carrot.evilVersion);
                evilcarrot.transform.position = carrot.transform.position;
                Game.current.veggiesLeft++;
            }
            leftCarrots--;
            
            //MonoBehaviour.print("leftt, leftc, neededt, neededc " + leftTomatoes + " " + leftCarrots + " " + tomatoesNeeded + " " + carrotsNeeded);

        }

    }

    void createTile(IntVector2 tilePos, TileEnum type)
    {
        GameObject sample = Instantiate(tilledCell);
        if(type == TileEnum.TILLED)
        {
            sample.GetComponentInChildren<Renderer>().material = tilledMaterial;
        }else
        {
            sample.GetComponentInChildren<Renderer>().material = wateredMaterial;
        }
        //sample.transform.parent = transform;
        sample.name = type.ToString();
        sample.transform.position = new Vector3(tilePos.x * cellSize + cellSize / 2, 0, tilePos.y * cellSize + cellSize / 2);
        objectTileGrid[tilePos] = sample;
        if (!Game.current.tileGrid.ContainsKey(tilePos))
        {
            Game.current.tileGrid[tilePos] = type;
        }
    }

    void handVeggie(IntVector2 tilePos)
    {
        if (Game.current.tileGrid.ContainsKey(tilePos))
        {
            if (Game.current.veggieGrid.ContainsKey(tilePos))
            {
                if (!Game.current.veggieGrid[tilePos].isCollectable()) return;
                SoundEffects.sf.playGold();
                Game.current.currentMoney += Game.current.veggieGrid[tilePos].getInfo().coins;
                Game.current.veggieGrid.Remove(tilePos);
                Destroy(objectVeggieGrid[tilePos].gameObject);
                objectVeggieGrid.Remove(tilePos);
            }
            Game.current.tileGrid.Remove(tilePos);
            Destroy(objectTileGrid[tilePos]);
            objectTileGrid.Remove(tilePos);
            Game.current.tileGrid[tilePos] = TileEnum.TILLED;
            createTile(tilePos, TileEnum.TILLED);
            return;
        }
    }

    void hoeTile(IntVector2 tilePos)
    {
        SoundEffects.sf.playHoeTile();
        if (Game.current.tileGrid.ContainsKey(tilePos))
        {
            //MonoBehaviour.print("Removing tile");
            if (Game.current.veggieGrid.ContainsKey(tilePos))
            {
                MonoBehaviour.print("Contains veggie" + objectVeggieGrid.ContainsKey(tilePos));
                if (Game.current.veggieGrid[tilePos].isCollectable())
                {
                    Game.current.currentMoney += Game.current.veggieGrid[tilePos].getInfo().coins;
                }
                Game.current.veggieGrid.Remove(tilePos);
                Destroy(objectVeggieGrid[tilePos].gameObject);
                objectVeggieGrid.Remove(tilePos);
            }
            Game.current.tileGrid.Remove(tilePos);
            Destroy(objectTileGrid[tilePos]);
            objectTileGrid.Remove(tilePos);
            return;
        }
        
        Game.current.tileGrid[tilePos] = TileEnum.TILLED;
        createTile(tilePos, TileEnum.TILLED);        
    }

    void waterTile(IntVector2 tilePos)
    {
        if (!Game.current.tileGrid.ContainsKey(tilePos)) return;
        SoundEffects.sf.playWater();
        Game.current.tileGrid[tilePos] = TileEnum.WATERED;
        objectTileGrid[tilePos].GetComponentInChildren<Renderer>().material = wateredMaterial;
    }

    void createVegetable(IntVector2 posgrid, VegetableInfo veggieInfo)
    {
        if (objectVeggieGrid.ContainsKey(posgrid) || !Game.current.tileGrid.ContainsKey(posgrid))
        {
            return;
        }
        Vegetable v = veggieInfo.stillSeed() ? Manager.instance.seed : Manager.instance.enumToVeg[veggieInfo.currentVeggie];
        Vegetable carrot = Instantiate(v);
        carrot.transform.parent = vegetables.transform;
        Vector3 carrPos;
        carrPos.x = posgrid.x * cellSize + cellSize / 2;
        carrPos.z = posgrid.y * cellSize + cellSize / 2;
        carrPos.y = veggieInfo.veggieHeightAdd();
        carrot.transform.position = carrPos;
        objectVeggieGrid.Add(posgrid, carrot);
        if (!Game.current.veggieGrid.ContainsKey(posgrid))
        {
            Game.current.veggieGrid.Add(posgrid, veggieInfo);
        }
        //MonoBehaviour.print("Creating veggie in pos" + posgrid.x + " " + posgrid.y);
    }

    void UpdateInput()
    {
        bool fire1 = Input.GetButtonDown("Fire1");
        bool fire2 = Input.GetButtonDown("Fire2");
        bool fire = fire1 || fire2;
        if (fire && IsFireValid())
        {
            IntVector2 posgrid = new IntVector2(gridX, gridY);

            if(Game.current.currentTime > 18*60)
            {
                Manager.instance.setAdviceMessageImmediate("You can't use this at night!");
                return;
            }

            switch (Manager.currentItem)
            {
                case InventoryEnum.HAND:
                    handVeggie(posgrid);
                    break;
                case InventoryEnum.HOE:
                    hoeTile(posgrid);
                    break;
                case InventoryEnum.CAN:
                    waterTile(posgrid);
                    break;
                case InventoryEnum.CARROTSEED:
                    createVegetable(posgrid, new VegetableInfo(VegetablesEnum.CARROT, 0));
                    break;
                case InventoryEnum.TOMATOSEED:
                    createVegetable(posgrid, new VegetableInfo(VegetablesEnum.TOMATO, 0));
                    break;
            }
        }
    }

    GameObject CreateChild()
    {
        /*GameObject go = new GameObject();

        go.name = "Grid Cell";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshFilter>().mesh = CreateMesh();

        return go;*/

        GameObject sample = Instantiate(tilledCell);
        sample.transform.parent = transform;
        sample.name = "Grid Cell";
        sample.transform.localPosition = Vector3.zero;
        Color c = sample.GetComponentInChildren<Renderer>().material.color;
        c.a = 0.5f;
        sample.GetComponentInChildren<Renderer>().material.color = c;
        return sample;
    }

    void UpdateSize()
    {
        int newSize = gridHeight * gridWidth;
        int oldSize = _cells.Length;

        if (newSize == oldSize)
            return;

        GameObject[] oldCells = _cells;
        _cells = new GameObject[newSize];

        if (newSize < oldSize)
        {
            for (int i = 0; i < newSize; i++)
            {
                _cells[i] = oldCells[i];
            }

            for (int i = newSize; i < oldSize; i++)
            {
                Destroy(oldCells[i]);
            }
        }
        else if (newSize > oldSize)
        {
            for (int i = 0; i < oldSize; i++)
            {
                _cells[i] = oldCells[i];
            }

            for (int i = oldSize; i < newSize; i++)
            {
                _cells[i] = CreateChild();
            }
        }

        _heights = new float[(gridHeight + 1) * (gridWidth + 1)];
    }

    Vector3 getGridCell(Vector3 point)
    {
        Vector3 position;
        position.x = Mathf.Floor(point.x / cellSize) * cellSize;
        position.z = Mathf.Floor(point.z / cellSize) * cellSize;
        position.y = 0;
        return position;
    }

    void UpdatePosition()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Terrain"));
        Vector3 position = hitInfo.point;

        //position.x -= hitInfo.point.x % cellSize + gridWidth * cellSize / 2;
        //position.z -= hitInfo.point.z % cellSize + gridHeight * cellSize / 2;
        position.x = Mathf.Floor(hitInfo.point.x / cellSize) * cellSize + cellSize / 2;
        position.z = Mathf.Floor(hitInfo.point.z / cellSize) * cellSize + cellSize / 2;
        position.y = 0;

        gridX = (int) Mathf.Floor(hitInfo.point.x / cellSize);
        gridY = (int) Mathf.Floor(hitInfo.point.z / cellSize);

        //MonoBehaviour.print("updating?" + gridX + " " + gridY + " " + position.x + " " + position.z);


        transform.position = position;
    }

    void UpdateHeights()
    {
        RaycastHit hitInfo;
        Vector3 origin;

        for (int z = 0; z < gridHeight + 1; z++)
        {
            for (int x = 0; x < gridWidth + 1; x++)
            {
                origin = new Vector3(x * cellSize, 200, z * cellSize);
                Physics.Raycast(transform.TransformPoint(origin), Vector3.down, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Terrain"));

                _heights[z * (gridWidth + 1) + x] = hitInfo.point.y;
            }
        }
    }

    void UpdateCells()
    {
        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject cell = _cells[z * gridWidth + x];
                //MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();
                //MeshFilter meshFilter = cell.GetComponent<MeshFilter>();
                bool validCell = IsCellValid(x, z);

                //meshRenderer.material = validCell ? cellMaterialValid : cellMaterialInvalid;
                if (validCell) UpdateMesh(cell, x, z);

                cell.GetComponentInChildren<Renderer>().enabled = IsFireValid();
                
            }
        }
    }

    bool IsFireValid()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Buildings", "Terrain"));
        return hitInfo.collider != null && hitInfo.distance < maxDistance 
            && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Terrain");
    }

    bool IsCellValid(int x, int z)
    {
        RaycastHit hitInfo;
        Vector3 origin = new Vector3(x * cellSize + cellSize / 2, 200, z * cellSize + cellSize / 2);
        Physics.Raycast(transform.TransformPoint(origin), Vector3.down, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Buildings"));

        return hitInfo.collider == null;
    }

    GameObject CreateMesh()
    {
        /*Mesh mesh = new Mesh();

        mesh.name = "Grid Cell";
        mesh.vertices = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.uv = new Vector2[] { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0) };

        return mesh;*/
        return null;
    }

    void UpdateMesh(GameObject mesh, int x, int z)
    {
        /*mesh.vertices = new Vector3[] {
            MeshVertex(x, z),
            MeshVertex(x, z + 1),
            MeshVertex(x + 1, z),
            MeshVertex(x + 1, z + 1),
        };*/
        mesh.transform.localPosition = new Vector3(0, 0, 0);

    }

    Vector3 MeshVertex(int x, int z)
    {
        return new Vector3(x * cellSize, _heights[z * (gridWidth + 1) + x] + yOffset, z * cellSize);
    }
}