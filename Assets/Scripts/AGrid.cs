using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    //public LayerMask feederMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public int obstacleProximityPenalty = 100;
    
    LayerMask walkableMask;
    Dictionary<int,int> walkableRegionsMap = new Dictionary<int,int>();
    Node[ , ] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue, penaltyMax = int.MinValue;

    public GameObject comedor, cintas;
    public Transform obstaculos;
    int comedorLimit;
    int cinProb;
    public static List<Node> feeders = new List<Node>(); //Esta lista luego tendra que estar en el script de los npcs y aqui una referencia a ese script

    

    public Transform player;


    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
        cinProb = Random.Range(0, 50);
        foreach(TerrainType region in walkableRegions)
        {
            walkableMask.value  |= region.terrainMask.value;
            walkableRegionsMap.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
        CreateGrid();
    }

    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    void CreateGrid()
    {

        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        int numFilasC1 = Random.Range(0, 3);
        int numFilasC2 = Random.Range(0, 3);
        int numFilasC3 = Random.Range(0, 3);
        int numFilasC4 = Random.Range(0, 3);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask)) ;
                int movementPenalty = 0;
                //raycast


                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableRegionsMap.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                if ((x == 0  || x == gridSizeX - 1) && (y > 1 && y < gridSizeY - 3)) // ultima y primera columnas del grid
                {
                    if (cinProb >= 25   && y % 3 == 0)
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "cinta",movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }
                }
                else if (x == (gridSizeX/2) -1 && (y > 4 &&  y < (gridSizeY / 2) - 6 || y > (gridSizeY / 2) + 4 && y < gridSizeY-4)) //columna del medio del grid
                {
                    if (cinProb < 25 && y % 2 == 0)
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "cinta", movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }
                }
                else if (x > 4 && x < (gridSizeX / 2) - 5 && y > 4 && y < (gridSizeY / 2) - 4)//cuadrante izquierdo bajo
                { 

                    if (x % 5 == 0 && y % 8 == 0 && y < (gridSizeY / 2) - 2 - (6*numFilasC1))
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "comedor", movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }

                }
                else if (x > 4 && x < (gridSizeX / 2) - 5 && y > (gridSizeY / 2) + 4 && y < gridSizeY - 4) //cuadrante izquierdo alto
                {

                    if (x % 5 == 0 && y % 8 == 0 && y < (gridSizeY - 4) - (6 * numFilasC2))
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "comedor", movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }

                }
                else if (x > (gridSizeX / 2) + 4 && x < gridSizeX - 4 && y > 4 && y < (gridSizeY / 2) - 4) //cuadrante derecho bajo
                {
                    if (x % 6 == 0 && y % 8 == 0 && y < ((gridSizeY / 2) - 4) - (6 * numFilasC3))
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "comedor", movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }

                }
                else if (x > (gridSizeX / 2) + 4 && x < gridSizeX - 4 && y > (gridSizeY / 2) + 4 && y < gridSizeY - 4) //cuadrante derecho alto
                {
                    if (x % 6 == 0 && y % 8 == 0 && y < (gridSizeY - 4) - (6 * numFilasC4) )
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "comedor", movementPenalty);
                    }
                    else
                    {
                        grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                    }

                }
                else
                {
                    grid[x, y] = new Node(walkable, worldPoint, x, y, "camino", movementPenalty);
                }

            }
        }
        SpawnDistribution();
        BlurPenaltyMap(1);
 
    }

    void SpawnDistribution()
    {

        foreach (Node node in grid)
        {
            if (node.objeto == "comedor")
            {
                node.fd = Instantiate(comedor, node.worldPosition, comedor.transform.rotation, obstaculos);
                node.movementPenalty += obstacleProximityPenalty;
                grid[node.gridX-1, node.gridY].walkable = false;
                grid[node.gridX - 1, node.gridY].movementPenalty += obstacleProximityPenalty;
                grid[node.gridX+1, node.gridY].walkable = false;
                grid[node.gridX + 1, node.gridY].movementPenalty += obstacleProximityPenalty;
                feeders.Add(node); //Lista de Nodos donde hay comederos
            }
            else if (node.objeto == "cinta")
            {
                if(node.gridY >= gridSizeY / 2)
                {
                    Instantiate(cintas, node.worldPosition, comedor.transform.rotation, obstaculos);
                }
                else
                {
                    Instantiate(cintas, node.worldPosition, Quaternion.Euler(0, 180, 0), obstaculos);
                }
                node.walkable = false;
                node.movementPenalty += obstacleProximityPenalty;
                grid[node.gridX,node.gridY + 1].walkable = node.walkable;
                grid[node.gridX, node.gridY + 1].movementPenalty = node.movementPenalty;
                grid[node.gridX, node.gridY - 1].walkable = node.walkable;
                grid[node.gridX, node.gridY - 1].movementPenalty = node.movementPenalty;
            }


        }

    }

    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtens = (kernelSize-1) / 2;
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; y++)
        {
            for(int x = -kernelExtens; x <= kernelExtens; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtens);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;

            }
            for(int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtens - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtens,0,gridSizeX-1);

                penaltiesHorizontalPass[x,y] = penaltiesHorizontalPass[x-1, y] - grid[removeIndex,y].movementPenalty + grid[addIndex,y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtens; y <= kernelExtens; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtens);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];

            }
            
            int blurrePenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurrePenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtens - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtens, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x , y - 1] - penaltiesHorizontalPass[x,removeIndex] + penaltiesHorizontalPass[x,addIndex];
                blurrePenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y]/(kernelSize*kernelSize));
                grid[x,y].movementPenalty = blurrePenalty;

                if(blurrePenalty > penaltyMax)
                {
                    penaltyMax = blurrePenalty;
                }
                if(blurrePenalty < penaltyMin)
                {
                    penaltyMin = blurrePenalty;
                }
            }
        }

    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX &&  checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }

        return neighbours;
    } 


    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));


        if (grid != null && displayGridGizmos)
        {
            Node playerNode = GetNodeFromWorldPoint(player.position);
            foreach (Node n in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;

                if (playerNode == n)
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));

            }

        }
        
        
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

    
}