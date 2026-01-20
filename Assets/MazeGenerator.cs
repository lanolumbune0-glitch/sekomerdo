using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Labirent Ayarları")]
    public int width = 10; // Labirent genişliği (hücre sayısı)
    public int height = 10; // Labirent yüksekliği (hücre sayısı)
    public float cellSize = 2f; // Her hücrenin boyutu (duvar aralığı)

    [Header("Prefablar")]
    public GameObject wallPrefab; // Duvar olarak kullanılacak nesne
    // İstersen zemin prefabı da ekleyebilirsin
    // public GameObject floorPrefab; 

    private Cell[,] grid;
    private Stack<Vector2Int> stack = new Stack<Vector2Int>();

    void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        // 1. Izgarayı (Grid) oluştur
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell();
            }
        }

        // 2. Başlangıç hücresini seç (0,0)
        Vector2Int currentCell = new Vector2Int(0, 0);
        grid[currentCell.x, currentCell.y].visited = true;
        stack.Push(currentCell);

        // 3. Algoritma döngüsü (Recursive Backtracker)
        while (stack.Count > 0)
        {
            currentCell = stack.Pop();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(currentCell);

            if (neighbors.Count > 0)
            {
                stack.Push(currentCell);
                Vector2Int randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(currentCell, randomNeighbor);
                grid[randomNeighbor.x, randomNeighbor.y].visited = true;
                stack.Push(randomNeighbor);
            }
        }

        // 4. Labirenti sahneye çiz
        DrawMaze();
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Yukarı
        if (cell.y + 1 < height && !grid[cell.x, cell.y + 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y + 1));
        // Aşağı
        if (cell.y - 1 >= 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
        // Sağ
        if (cell.x + 1 < width && !grid[cell.x + 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
        // Sol
        if (cell.x - 1 >= 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x - 1, cell.y));

        return neighbors;
    }

    void RemoveWall(Vector2Int current, Vector2Int neighbor)
    {
        // Komşu yukarıdaysa
        if (neighbor.y > current.y)
        {
            grid[current.x, current.y].topWall = false;
            grid[neighbor.x, neighbor.y].bottomWall = false;
        }
        // Komşu aşağıdaysa
        else if (neighbor.y < current.y)
        {
            grid[current.x, current.y].bottomWall = false;
            grid[neighbor.x, neighbor.y].topWall = false;
        }
        // Komşu sağdaysa
        else if (neighbor.x > current.x)
        {
            grid[current.x, current.y].rightWall = false;
            grid[neighbor.x, neighbor.y].leftWall = false;
        }
        // Komşu soldaysa
        else if (neighbor.x < current.x)
        {
            grid[current.x, current.y].leftWall = false;
            grid[neighbor.x, neighbor.y].rightWall = false;
        }
    }

    void DrawMaze()
    {
        // Labirenti tutacak boş bir ebeveyn nesne oluşturalım ki hiyerarşi dağılmasın
        GameObject mazeParent = new GameObject("GeneratedMaze");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);

                // Üst Duvar (Sadece en üst sıra veya üstünde duvar olması gerekenler)
                if (cell.topWall)
                {
                   GameObject wall = Instantiate(wallPrefab, cellPosition + new Vector3(0, 0, cellSize / 2f), Quaternion.identity, mazeParent.transform);
                   wall.transform.localScale = new Vector3(cellSize, wall.transform.localScale.y, 0.1f); // Duvarı incelt ve hücre boyutuna uyarla
                }
                
                // Alt Duvar (Sadece en alt sıra)
                if (cell.bottomWall && y == 0)
                {
                    GameObject wall = Instantiate(wallPrefab, cellPosition + new Vector3(0, 0, -cellSize / 2f), Quaternion.identity, mazeParent.transform);
                    wall.transform.localScale = new Vector3(cellSize, wall.transform.localScale.y, 0.1f);
                }

                // Sağ Duvar
                if (cell.rightWall)
                {
                   GameObject wall = Instantiate(wallPrefab, cellPosition + new Vector3(cellSize / 2f, 0, 0), Quaternion.Euler(0, 90, 0), mazeParent.transform);
                   wall.transform.localScale = new Vector3(cellSize, wall.transform.localScale.y, 0.1f);
                }

                 // Sol Duvar (Sadece en sol sıra)
                if (cell.leftWall && x == 0)
                {
                    GameObject wall = Instantiate(wallPrefab, cellPosition + new Vector3(-cellSize / 2f, 0, 0), Quaternion.Euler(0, 90, 0), mazeParent.transform);
                    wall.transform.localScale = new Vector3(cellSize, wall.transform.localScale.y, 0.1f);
                }
                
                // İstersen burada zemin prefabını da oluşturabilirsin.
            }
        }
    }

    // Her bir hücrenin verisini tutan yardımcı sınıf
    private class Cell
    {
        public bool visited = false;
        public bool topWall = true;
        public bool bottomWall = true;
        public bool leftWall = true;
        public bool rightWall = true;
    }
}