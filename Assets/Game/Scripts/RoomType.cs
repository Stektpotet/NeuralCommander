using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Regex = System.Text.RegularExpressions.Regex;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: make sure the spawn lsit only have allow top as first entries. because of SpawnGenerator behaviour

[CreateAssetMenu(fileName = "New Room Type", menuName = "Room Type")]
public class RoomType : ScriptableObject
{
    [System.Serializable]
    public struct SpawnListItem
    {
        [Range(0, 360)]
        [SerializeField] private int angleIteration;
        [MinMaxSlider(0, 64)]
        [SerializeField] private Vector2Int spawnLimits;
        [SerializeField] private Spawnable spawnable;

        public int AngleIteration => angleIteration;
        public int Min => spawnLimits.x;
        public int Max => spawnLimits.y;
        public Spawnable Spawnable => spawnable;

        public SpawnListItem(int angleIter, Vector2Int spawnLim, Spawnable spawnitem)
        {
            angleIteration = angleIter;
            spawnLimits = spawnLim;
            spawnable = spawnitem;
        }
    }

    ///<summary>
    ///Each RoomPieceSet may have 1-N pieces of all the base pieces
    ///Allowing the generator to pick randomly from the 1-N pieces of a certain in a given set
    ///</summary>
    ///
    [System.Serializable]
    public struct RoomSet //TODO change this into a scriptableObject on its own? 🤔
    {
        internal string Name { get { return name; } set { name = value; } }
        [SerializeField] private string name;
        public GameObject[] Floors;
        public GameObject[] Corridors_EastWest; //TODO Name these more conviniently
        public GameObject[] Corridors_NorthSouth;
        public GameObject[] Corners_NorthEast;
        public GameObject[] Corners_SouthEast;
        public GameObject[] Corners_NorthWest;
        public GameObject[] Corners_SouthWest;
        public GameObject[] Walls_East;
        public GameObject[] Walls_West;
        public GameObject[] Walls_North;
        public GameObject[] Walls_South;
        public GameObject[] Doors_North;
        public GameObject[] Doors_South;
        public GameObject[] Doors_East;
        public GameObject[] Doors_West;

        //A Piece Set-specific list of extra items (additional items to the spawnList)
        public SpawnListItem[] extraSpawnList;
    }

#if UNITY_EDITOR

    private void Autofill()
    {

        string path = EditorUtility.OpenFolderPanel("Select TileSet Folder", Application.dataPath + "/LevelGeneration/RoomTypes/", "");
        string[] files = System.IO.Directory.GetFiles(path);

        List<GameObject> Floors = new List<GameObject>();
        List<GameObject> Corridors_EastWest = new List<GameObject>();
        List<GameObject> Corridors_NorthSouth = new List<GameObject>();
        List<GameObject> Corners_NorthEast = new List<GameObject>();
        List<GameObject> Corners_SouthEast = new List<GameObject>();
        List<GameObject> Corners_NorthWest = new List<GameObject>();
        List<GameObject> Corners_SouthWest = new List<GameObject>();
        List<GameObject> Walls_East = new List<GameObject>();
        List<GameObject> Walls_West = new List<GameObject>();
        List<GameObject> Walls_North = new List<GameObject>();
        List<GameObject> Walls_South = new List<GameObject>();
        List<GameObject> Doors_North = new List<GameObject>();
        List<GameObject> Doors_South = new List<GameObject>();
        List<GameObject> Doors_East = new List<GameObject>();
        List<GameObject> Doors_West = new List<GameObject>();
        

        Regex northRegex = new Regex(@"(?:((?i)north(?-i))|(?<=[ _])N)");
        Regex southRegex = new Regex(@"(?:((?i)south(?-i))|(?<=[ _N])S)");
        Regex eastRegex =  new Regex(@"(?:((?i)east(?-i))|(?<=[ _NS])E)");
        Regex westRegex =  new Regex(@"(?:((?i)west(?-i))|(?<=[ _NSE])W)");
        
        Regex floorRegex = new Regex(@"((?i)floor(?-i))");

        Regex corridorRegex = new Regex(@"((?i)corridor(?-i))");
        Regex cornerRegex = new Regex(@"((?i)corner(?-i))");
        Regex wallRegex = new Regex(@"((?i)wall(?-i))");
        Regex doorRegex = new Regex(@"((?i)door(?-i))");
        

        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;

            if (floorRegex.IsMatch(file))
            {
                Debug.Log(file.Substring(Application.dataPath.Length - 6));
                Floors.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                continue;
            }

            if (wallRegex.IsMatch(file))
            {
                Debug.Log(file.Substring(Application.dataPath.Length - 6));
                if (northRegex.IsMatch(file))
                {
                    Walls_North.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (southRegex.IsMatch(file))
                {
                    Walls_South.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (eastRegex.IsMatch(file))
                {
                    Walls_East.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (westRegex.IsMatch(file))
                {
                    Walls_West.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
            }

            if (doorRegex.IsMatch(file))
            {
                Debug.Log(file.Substring(Application.dataPath.Length - 6));
                if (northRegex.IsMatch(file))
                {
                    Doors_North.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (southRegex.IsMatch(file))
                {
                    Doors_South.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (eastRegex.IsMatch(file))
                {
                    Doors_East.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (westRegex.IsMatch(file))
                {
                    Doors_West.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
            }

            if (corridorRegex.IsMatch(file))
            {
                Debug.Log(file.Substring(Application.dataPath.Length - 6));
                if (northRegex.IsMatch(file) && southRegex.IsMatch(file))
                {
                    Corridors_NorthSouth.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
                if (eastRegex.IsMatch(file) && westRegex.IsMatch(file))
                {
                    Corridors_EastWest.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                    continue;
                }
            }

            if (cornerRegex.IsMatch(file))
            {
                Debug.Log(file.Substring(Application.dataPath.Length - 6));
                if (northRegex.IsMatch(file))
                {
                    if (eastRegex.IsMatch(file))
                    {
                        Corners_NorthEast.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                        continue;
                    }
                    if (westRegex.IsMatch(file))
                    {
                        Corners_NorthWest.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                        continue;
                    }
                }
                if (southRegex.IsMatch(file))
                {
                    if (eastRegex.IsMatch(file))
                    {
                        Corners_SouthEast.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                        continue;
                    }
                    if (westRegex.IsMatch(file))
                    {
                        Corners_SouthWest.Add(AssetDatabase.LoadAssetAtPath<GameObject>(file.Substring(Application.dataPath.Length - 6)));
                        continue;
                    }
                }
            }
        }

        roomSets[roomSets.Length - 1] = new RoomSet()
        {
            Name = name = path.Substring(path.LastIndexOf('/')+1),
            Floors = Floors.ToArray(),
            Walls_North = Walls_North.ToArray(),
            Walls_South = Walls_South.ToArray(),
            Walls_East = Walls_East.ToArray(),
            Walls_West = Walls_West.ToArray(),

            Corners_NorthEast = Corners_NorthEast.ToArray(),
            Corners_NorthWest = Corners_NorthWest.ToArray(),
            Corners_SouthEast = Corners_SouthEast.ToArray(),
            Corners_SouthWest = Corners_SouthWest.ToArray(),

            Corridors_NorthSouth = Corridors_NorthSouth.ToArray(),
            Corridors_EastWest = Corridors_EastWest.ToArray(),

            Doors_North = Doors_North.ToArray(),
            Doors_South = Doors_South.ToArray(),
            Doors_East = Doors_East.ToArray(),
            Doors_West = Doors_West.ToArray()
        };
        
    }

#endif


    [Header("Spawnlist")]
    public List<SpawnListItem> spawnList = new List<SpawnListItem>();
    [Header("Room Pieces")]
    //A room may pick a random set i.e. let's say we have 2-5 types of kitchen sets...
#if UNITY_EDITOR
    [ContextMenuItem("Insert New From Folder", "Autofill")]
#endif
    public RoomSet[] roomSets;

    [Header("Generator Properties ")]

    [MinMaxSlider(1,10)]
    public Vector2Int doorLimits = new Vector2Int(1,4);
    
    [Header("Drunken Walk implementation")]

    [Range(0.01f, 1.0f)]
    public float originBias = 0.5f;
    [MinMaxSlider(4, 200)]
    public Vector2Int tileLimits = new Vector2Int(50,150);

    [Header("Spanning Rectangles implementation")]
    [MinMaxSlider(8, 40)]
    public Vector2Int sizeLimits = new Vector2Int(10,30);
    [Tooltip("This determins how many room clusters the brute force implementation should use when generating")]
    public int roomClusters = 10;
    
}
