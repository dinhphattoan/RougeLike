using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    #region Procedure landmass generation
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;
    public bool generateTerrain = false;
    public bool generateGrid = false;
    #endregion
    #region Terrain drawing
    public TerrainType[] regions;
    [Header("Tree distribution's Attributes")]
    public float TerrainnoiseScale;
    public int Terrainoctaves;
    [Range(0, 1)]
    public float Terrainpersistance;
    public float Terrainlacunarity;
    public TerrainType treeType;
    public List<TerrainType> TreeexcludeRegions = new List<TerrainType>();
    [Header("Rock distribution's Attributes")]
    public float Rocknoisescale;
    public int Rockoctaves;
    [Range(0, 1)]
    public float Rockpersistance;
    public float Rocklacunarity;
    public TerrainType rockType;
    public List<TerrainType> RockexcludeRegions = new List<TerrainType>();
    public Color[] finalizedMap;
    //Determine the curveness of the map, the ground will be a wall if the curve is too big, and the ground will still be a ground if the curve is too smaller than value
    public float maxCurveMagnitude = 0.1f;
    //Left->Top->Right->Bottom
    List<bool[]> tileCornerRule = new List<bool[]>(){
        // Region tile 
        new bool[4]{false,false,true,true},
        new bool[4]{true,false,true,true},
        new bool[4]{true,false,false,true},
        new bool[4]{false,true,true,true},
        new bool[4]{true,true,true,true},
        new bool[4]{true,true,false,true},
        new bool[4]{false,true,true,false},
        new bool[4]{true,true,true,false},
        new bool[4]{true,true,false,false},
        //For horizontal tile corner
        new bool[4]{false,false,true,false},
        new bool[4]{true,false,true,false},
        new bool[4]{true,false,false,false},
        //For vertical tile corner
        new bool[4]{false,false,false,true},
        new bool[4]{false,true,false,true},
        new bool[4]{false,true,false,false},
        //Independent tile
        new bool[4]{false,false,false,false}
    };
    #endregion
    #region Preload resources
    [Header("Resources gathered from resources objects")]
    public List<Sprite> listTextureGrass = new List<Sprite>();
    public List<Sprite> listTextureSand = new List<Sprite>();
    public List<Sprite> listTexutreWater = new List<Sprite>();
    public List<Sprite> listTextureElevation = new List<Sprite>();
    public List<GameObject> listTextureTree = new List<GameObject>();
    public List<GameObject> listTextureRock = new List<GameObject>();
    #endregion
    public Tilemap tilemap;
    public Grid grid;
    
    #region Terrain filling
    [Header("Generate Map attribute")]
    public Transform MapPivot;
    public List<GameObject> instantiatedSamples = new List<GameObject>();
    //Store Gameobject root
    public GameObject Treeterrain;
    public GameObject rockTerrain;
    public GameObject playerSpawnPoint;
    [Header("Distribution function section:")]
    [Header("Regions Distribution's rate (Poisson Disk)")]
    //Parameters for poisson disk scatterer
    public float minDist = 1;    // The smallest radius
    public int recursiveCount = 30;
    [Header("Tree's distribution rate (Poisson Disk)")]
    //Parameters for poisson disk scattererr
    public float minTreeDist = 5;    // The smallest radius
    public int recursiveTreeCount = 30;
    [Header("Rock's distribution rate (Poisson Disk)")]
    //Parameters for poisson disk scattererr
    public float minRockDist = 5;    // The smallest radius
    public int recursiveRockCount = 30;
    #endregion
    


    /// <summary>
    /// Build map landmass
    /// </summary>
    public int[,] ColorMapToInt(Color[] colorMap, List<TerrainType> terrainTypes)
    {
        int[,] intMap = new int[mapWidth, mapHeight];

        for (int i = 0; i < mapHeight; i++)
            for (int j = 0; j < mapWidth; j++)
            {
                for (int k = 0; k < terrainTypes.Count; k++)
                {
                    if (colorMap[i * mapWidth + j].ToHexString() == terrainTypes[k].colour.ToHexString())
                    {
                        intMap[j, i] = k;
                    }
                }
            }
        SetPlayerSpawn();
        return intMap;
    }




    /// <summary>
    /// Generates a map using noise and colour maps. The map is displayed using a MapDisplay object.
    /// </summary>
    /// <remarks>
    /// The map is generated using a noise map, which is generated using the Noise.GenerateNoiseMap method.
    /// The colour map is generated by iterating over each pixel in the noise map and assigning a colour based on the height value.
    /// The display mode of the map is determined by the drawMode property.
    /// If the drawMode is NoiseMap, the noise map is displayed using the MapDisplay.DrawTexture method.
    /// If the drawMode is ColourMap, the colour map is displayed using the MapDisplay.DrawTexture method.
    /// If the drawMode is Mesh, the terrain mesh is generated using the MeshGenerator.GenerateTerrainMesh method and displayed using the MapDisplay.DrawMesh method.
    /// </remarks>
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {


            //Instantiate GameObject to store terrain object (for well oriented purpose only)
            Treeterrain.transform.parent = this.transform;
            rockTerrain.transform.parent = this.transform;
            //Clear prev terrain object
            var tempArray = new GameObject[Treeterrain.transform.childCount];

            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = Treeterrain.transform.GetChild(i).gameObject;
            }

            foreach (var child in tempArray)
            {
                DestroyImmediate(child);
            }
            tempArray = new GameObject[rockTerrain.transform.childCount];

            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = rockTerrain.transform.GetChild(i).gameObject;
            }

            foreach (var child in tempArray)
            {
                DestroyImmediate(child);
            }
            instantiatedSamples.Clear();
            //Placing grid
            if (generateGrid)
            {
                int[,] intMap = ColorMapToInt(colourMap, regions.ToList());
                DrawMapGrid(intMap, noiseMap);
            }
            //Generate Terrain
            // if (generateTerrain)
            //     ApplyTerrain();

            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            GenerateTerrain(colourMap);
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));

        }
    }
    /// <summary>
    /// Purpose: Build terrain scatter surround the map
    /// Approach: Merge the generated terrains noise map into landmass, then merge the landmass
    ///	Approach: draw paste on the landmass map with patterns that is meet the condition (Some pattern that scatter in fobidded region will be excluded)
    /// </summary>
    /// <param name="colorMap"></param>
    public void GenerateTerrain(Color[] colorMap)
    {
        float[,] noiseMapTerrain = Noise.GenerateNoiseMap(mapWidth, mapHeight, UnityEngine.Random.Range(0, 1000), TerrainnoiseScale, Terrainoctaves, Terrainpersistance, Terrainlacunarity, offset);


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMapTerrain[x, y];
                if (currentHeight <= treeType.height && !IsExcludeRegion(colorMap[y * mapWidth + x], TreeexcludeRegions))
                {
                    colorMap[y * mapWidth + x] = treeType.colour;
                    break;
                }
            }
        }


    }
    private bool IsExcludeRegion(Color color, List<TerrainType> terrainTypes)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            if (terrainType.colour.ToHexString() == color.ToHexString()) return true;
        }
        return false;
    }
    public void GenerateRock(Color[] colorMap)
    {
        float[,] noiseMapTerrain = Noise.GenerateNoiseMap(mapWidth, mapHeight, UnityEngine.Random.Range(0, 1000), Rocknoisescale, Rockoctaves, Rockpersistance, Rocklacunarity, offset);


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMapTerrain[x, y];

                if (currentHeight <= rockType.height && !IsExcludeRegion(colorMap[y * mapWidth + x], RockexcludeRegions))
                {
                    colorMap[y * mapWidth + x] = rockType.colour;
                    break;
                }
            }
        }
    }






    /// <summary>
    /// Select Appropirate tile that fit the corners
    /// </summary>
    /// <param name="intMap"></param>
    /// <param name="pos"></param>
    /// <returns>A integer of selected index, -1 if not found</returns>
    int GroundTileSelector(int[,] intMap, (int, int) pos)
    {
        //Left->Top->Right->Bottom
        bool[] boolFourCornerCheck = new bool[4] { false, false, false, false };
        //Left Corner
        if (pos.Item1 > 0)
            boolFourCornerCheck[0] = intMap[pos.Item1 - 1, pos.Item2] >= intMap[pos.Item1, pos.Item2];
        else boolFourCornerCheck[0] = false;
        //Top Corner
        if (pos.Item2 > 0)
        {
            boolFourCornerCheck[1] = intMap[pos.Item1, pos.Item2 - 1] >= intMap[pos.Item1, pos.Item2];
        }
        else boolFourCornerCheck[1] = false;
        //Right Corner
        if (pos.Item1 < intMap.GetLength(0) - 1)
            boolFourCornerCheck[2] = intMap[pos.Item1 + 1, pos.Item2] >= intMap[pos.Item1, pos.Item2];
        else boolFourCornerCheck[2] = false;
        //Down Corner
        if (pos.Item2 < intMap.GetLength(1) - 1)
        {
            boolFourCornerCheck[3] = intMap[pos.Item1, pos.Item2 + 1] >= intMap[pos.Item1, pos.Item2];
        }
        else boolFourCornerCheck[3] = false;



        //Check if the tile is in the list
        for (int i = 0; i < tileCornerRule.Count; i++)
        {
            bool flag = true;
            for (int j = 0; j < 4; j++)
            {
                if (tileCornerRule[i][j] != boolFourCornerCheck[j])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                if (i == 6)
                {
                    Debug.Log("true");
                }
                return i;
            }
        }
        return -1;
    }













    /// <summary>
    /// Draw map from given int map
    /// </summary>
    /// <param name="intMap"></param> <summary>
    /// 
    /// </summary>
    /// <param name="intMap"></param>
    public void DrawMapGrid(int[,] intMap, float[,] noiseMap)
    {
        //Clear all prev tilemap
        GameObject[] tempArray = new GameObject[grid.transform.childCount];
        for (int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = grid.transform.GetChild(i).gameObject;
        }

        foreach (var child in tempArray)
        {
            DestroyImmediate(child);
        }
        Vector3 tileMapPos = new Vector3(0, 0, 0 - regions.GetLength(0));
        //Must give a list of texture according to the regions value
        List<List<Sprite>> listOfListRegions = new List<List<Sprite>>(){
            listTexutreWater,listTexutreWater,listTextureSand,listTextureGrass,listTextureGrass
        };
        int elevationLevel = 1;
        for (int i = 0; i < regions.GetLength(0); i++)
        {
            if (i >= 3)
            {
                var gameObjectTileMapElevation = DrawMapTileElevationFromTwoRegion(intMap, noiseMap, i, i - 1, listTextureElevation, elevationLevel++);
                gameObjectTileMapElevation.transform.position = tileMapPos;
                tileMapPos = new Vector3(tileMapPos.x, tileMapPos.y, tileMapPos.z - 1);
                gameObjectTileMapElevation.transform.SetParent(grid.transform);
            }
            var gameObjectTileMap = DrawMapTileFromRegion(intMap, i, listOfListRegions[i]);
            gameObjectTileMap.transform.position = tileMapPos;
            tileMapPos = new Vector3(tileMapPos.x, tileMapPos.y, tileMapPos.z - 1);
            gameObjectTileMap.transform.SetParent(grid.transform);

        }
        grid.transform.position = new Vector3(grid.transform.position.x, grid.transform.position.y, mapHeight + 5);
    }






    /// <summary>
    /// Draw new tilemap that specify for the region
    /// </summary>
    /// <param name="intMap"></param>
    /// <param name="regionInt"></param>
    /// <param name="listTextures"></param>
    /// <returns>New tile map's game object</returns>
    GameObject DrawMapTileFromRegion(int[,] intMap, int regionInt, List<Sprite> listTextures)
    {
        var gameObject = new GameObject() { name = "TileMap level: " + regionInt };
        var temptilemap = gameObject.AddComponent<TilemapRenderer>().GetComponent<Tilemap>();

        for (int i = 0; i < intMap.GetLength(0); i++)
        {
            for (int j = 0; j < intMap.GetLength(1); j++)
            {
                Sprite sprite = null;
                if (intMap[i, j] == regionInt)
                {
                    sprite = listTextures[GroundTileSelector(intMap, (i, j))];
                }


                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                temptilemap.SetTile(new Vector3Int(i, j, 0), tile);
            }

        }
        return gameObject;
    }
    /// <summary>
    /// Draw Map Tile's Elevation from two region
    /// </summary>
    /// <param name="intMap">int map</param>
    /// <param name="noiseMap">a noise map that specify a map height</param>
    /// <param name="upperRegionValue">upper height land</param>
    /// <param name="lowerRegionValue">lower height land</param>
    /// <param name="listTextures">list of elevation tile</param>
    /// <param name="elevationLevel">specific elevation level</param>
    /// <returns>New tile map's game object</returns>
    GameObject DrawMapTileElevationFromTwoRegion(int[,] intMap, float[,] noiseMap, int upperRegionValue, int lowerRegionValue, List<Sprite> listTextures, int elevationLevel)
    {
        var gameObject = new GameObject() { name = "Elevation Level: " + elevationLevel };
        var temptilemap = gameObject.AddComponent<TilemapRenderer>().GetComponent<Tilemap>();
        bool prevIsWall = false;
        for (int i = 0; i < intMap.GetLength(0); i++)
        {
            for (int j = 0; j < intMap.GetLength(1) - 1; j++)
            {
                Sprite sprite = null;
                if (intMap[i, j] == lowerRegionValue && intMap[i, j + 1] == upperRegionValue)
                {

                    if (noiseMap[i, j + 1] - noiseMap[i, j] >= maxCurveMagnitude)
                    {
                        if (prevIsWall)
                        {
                            sprite = listTextures[1];
                        }
                        else
                        {
                            sprite = listTextures[0];
                            prevIsWall = true;
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (prevIsWall&&i>0)
                    {
                        prevIsWall = false;
                        sprite = listTextures[2];
                        var temptilebase = ScriptableObject.CreateInstance<Tile>();
                        temptilebase.sprite = sprite;
                        temptilemap.SetTile(new Vector3Int(i - 1, j, 0), temptilebase);
                        sprite = null;
                    }

                }
                var tilebase = ScriptableObject.CreateInstance<Tile>();
                tilebase.sprite = sprite;
                temptilemap.SetTile(new Vector3Int(i, j, 0), tilebase);

            }

        }
        return gameObject;
    }
    void ApplyTileElevation(float[,] noiseMap, int[,] intMap)
    {

    }
    void ApplyTerrain()
    {
        PoissonDiscSampler poissonDiscSampler = new PoissonDiscSampler()
        {
            minDist = minDist,
            width = mapWidth - 0.5f,
            height = mapHeight - 0.5f,
            recursiveCount = recursiveCount //default,
        };


        List<Vector2> listSamples = poissonDiscSampler.Sample();
        for (int i = 0; i < listSamples.Count; i++)
        {

            if (IsInRegion(listSamples[i], tilemap, listTextureGrass.GetRange(3, 3)))
            {
                GameObject gameObject = Instantiate(listTextureTree[UnityEngine.Random.Range(0, listTextureTree.Count)]);
                gameObject.transform.position = new Vector3(listSamples[i].x, listSamples[i].y, listSamples[i].y);
                gameObject.transform.parent = Treeterrain.transform;
                instantiatedSamples.Add(gameObject);
            }

        }
        ApplyScatteredTerrain();
    }
    void ApplyScatteredTerrain()
    {
        PoissonDiscSampler poissonDiscSampler = new PoissonDiscSampler()
        {
            minDist = minTreeDist,
            width = mapWidth - 0.5f,
            height = mapHeight - 0.5f,
            recursiveCount = recursiveRockCount //default,
        };
        //Tree
        List<Vector2> listSamples = poissonDiscSampler.Sample();
        for (int i = 0; i < listSamples.Count; i++)
        {

            if (IsInRegion(listSamples[i], tilemap, listTextureGrass.GetRange(0, 3)))
            {
                GameObject gameObject = Instantiate(listTextureTree[UnityEngine.Random.Range(0, listTextureTree.Count)]);
                gameObject.transform.position = new Vector3(listSamples[i].x, listSamples[i].y, listSamples[i].y);
                gameObject.transform.parent = Treeterrain.transform;
                instantiatedSamples.Add(gameObject);
            }

        }
        //Rock
        poissonDiscSampler.minDist = minRockDist;
        poissonDiscSampler.recursiveCount = recursiveRockCount;
        listSamples = poissonDiscSampler.Sample();
        for (int i = 0; i < listSamples.Count; i++)
        {

            if (IsInRegion(listSamples[i], tilemap, listTextureGrass.GetRange(0, 3)))
            {
                GameObject gameObject = Instantiate(listTextureRock[UnityEngine.Random.Range(0, listTextureRock.Count)]);
                gameObject.transform.position = new Vector3(listSamples[i].x, listSamples[i].y, listSamples[i].y);
                gameObject.transform.parent = rockTerrain.transform;
                instantiatedSamples.Add(gameObject);
            }

        }
    }
    bool IsInRegion(Vector2 desiredPos, Tilemap tilemap, List<Sprite> listRegion)
    {
        TileBase tileBase = tilemap.GetTile(tilemap.WorldToCell(desiredPos));
        if (tileBase != null)
        {
            Sprite sprite = (tileBase as Tile).sprite;
            if (sprite != null)
            {
                for (int i = 0; i < listRegion.Count; i++)
                {
                    if (sprite == listRegion[i])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
    void SetPlayerSpawn()
    {
        float height = UnityEngine.Random.Range(mapHeight / 4, mapHeight - (mapHeight / 4));
        playerSpawnPoint.transform.position = new Vector3(UnityEngine.Random.Range(mapWidth / 4, mapWidth - (mapWidth / 4)), height, height);
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;

}