using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{

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
    [Header("Terrain Resourcess")]
    public GameObject resourcesWater;
    public GameObject resourcesGrass;
    public GameObject resourcesTree;
    public GameObject resourcesRock;
    [Header("Resources gathered from resources objects")]
    public List<GameObject> listTextureGrass = new List<GameObject>();
    public List<GameObject> listTexutreWater = new List<GameObject>();
    public List<GameObject> listTextureTree = new List<GameObject>();
    public List<GameObject> listTextureRock = new List<GameObject>();
    public Tilemap tilemap;
    public Grid grid;

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
                tilemap.ClearAllTiles();
                DrawMapGrid(intMap);
            }
            //Generate Terrain
            if (generateTerrain)
                ApplyTerrain();

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
    // </summary>

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

    void getResources(List<GameObject> List, GameObject resources)
    {


        for (int i = 0; i < resources.transform.childCount; i++)
        {
            Transform child = resources.transform.GetChild(i);
            List.Add(child.gameObject);
        }
        List.Sort((x, y) => x.name.CompareTo(y.name));
    }

    public void DrawMapGrid(int[,] intMap)
    {
        //Remove all prev platforms
        listTextureGrass.Clear();
        listTexutreWater.Clear();
        listTextureTree.Clear();
        int childCount = transform.childCount;
        //Instantiate all platforms
        getResources(listTexutreWater, resourcesWater);
        getResources(listTextureGrass, resourcesGrass);
        getResources(listTextureTree, resourcesTree);
        getResources(listTextureRock, resourcesRock);
        for (int i = 0; i < intMap.GetLength(0); i++)
        {
            for (int j = 0; j < intMap.GetLength(1); j++)
            {
                Sprite sprite = null;

                //Water level 2 Platforms
                if (intMap[i, j] == 0)
                {

                    sprite = listTexutreWater[4].GetComponent<SpriteRenderer>().sprite;
                }
                else if (intMap[i, j] == 1)
                {
                    sprite = listTexutreWater[2].GetComponent<SpriteRenderer>().sprite;

                }
                else if (intMap[i, j] == 2)
                {
                    sprite = listTexutreWater[0].GetComponent<SpriteRenderer>().sprite;
                }
                //Grass Platform
                else if (intMap[i, j] == 3)
                {
                    //Grass1 would be greater chance of being select
                    int index = UnityEngine.Random.Range(-15, 2);
                    index = index < 0 ? 0 : index;
                    sprite = listTextureGrass[index].GetComponent<SpriteRenderer>().sprite;
                }
                else
                {   //Grass3 would be greater chance of being select
                    int index = UnityEngine.Random.Range(-20, 5);
                    index = index < 3 ? 3 : index;
                    sprite = listTextureGrass[index].GetComponent<SpriteRenderer>().sprite;
                }
                Tile tile = new Tile() { sprite = sprite };
                tilemap.SetTile(new Vector3Int(i, j, 0), tile);
            }
        }
        grid.transform.position = new Vector3(grid.transform.position.x, grid.transform.position.y, mapHeight + 5);
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
    bool IsInRegion(Vector2 desiredPos, Tilemap tilemap, List<GameObject> listRegion)
    {
        TileBase tileBase = tilemap.GetTile(tilemap.WorldToCell(desiredPos));
        if (tileBase != null)
        {
            Sprite sprite = (tileBase as Tile).sprite;
            if (sprite != null)
            {
                for (int i = 0; i < listRegion.Count; i++)
                {
                    if (sprite == listRegion[i].GetComponent<SpriteRenderer>().sprite)
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
        float height =UnityEngine.Random.Range(mapHeight/4, mapHeight-(mapHeight/4));
        playerSpawnPoint.transform.position = new Vector3(UnityEngine.Random.Range(mapWidth/4, mapWidth-(mapWidth/4)),height,height);
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;

}