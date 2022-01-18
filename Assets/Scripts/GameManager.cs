using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class GridSprites
{
    public List<TileScript> tile = new List<TileScript>();
}

public enum TileType { EMPTY, BLOCK_1, BLOCK_2, BLOCK_3, BLOCK_4, BLOCK_5, BLOCK_6, BLOCK_7, BLOCK_8, MINE, FLAG, NONE }

public class GameManager : MonoBehaviour
{
    #region GameObjects

    private Grid grid;
    private TileType[,] gridMap;

    public GameObject block;
    public GameObject gridPanel;
    
    [Header("Sprites")]
    [Space(20)]
    public Sprite bomb;
    public Sprite square;
    public Sprite graySquare;
    public Sprite flag;

    #endregion

    #region EnumsAndVariables
    
    public enum DifficultyType
    {
        EASY,
        MEDIUM,
        HARD
    }
    
    [Header("EnumsVariables")]
    [Space(20)]
    public DifficultyType difficultyType;
    public TileType tileType;

    #endregion
    
    #region Variables
    [Header("Variables")]
    [Space(20)]
    [SerializeField] private bool setFlag = false;
    [SerializeField] private Vector2 mousePos;

    #endregion

    #region Lists

    [Header("Lists")]
    [Space(20)]
    [SerializeField] private List<GridSprites> tileList = new List<GridSprites>();

    #endregion

    private void Start()
    {
        grid = new Grid(8, 15);
        gridPanel.GetComponent<GridLayoutGroup>().constraintCount = grid.width;

        gridMap = new TileType[grid.width, grid.height];
        
        for (int i = 0; i < grid.width; i++)
        {
            tileList.Add(new GridSprites());
            for (int j = 0; j < grid.height; j++)
            {
                gridMap[i, j] = 0;
                tileList[i].tile.Add(Instantiate(block, gridPanel.transform).GetComponent<TileScript>());
                SetBlock(i, j, TileType.EMPTY, true);
            }
        }

        PlaceBombs(DifficultyType.EASY, 15);

        HideAll();
    }

    private void HideAll()
    {
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                AssignSprite(i, j, TileType.EMPTY, true); // the TileType doesn't matters, as hide is true.
            }
        }
    }
    
    private void PlaceBombs(DifficultyType type, int bombs)
    {
        for (int i = 0; i < bombs; i++)
        {
            int x = Random.Range(0, grid.width);
            int y = Random.Range(0, grid.height);
            if (gridMap[x, y] != TileType.MINE)
            {
                gridMap[x, y] = TileType.MINE;
                SetBlock(x, y, TileType.MINE, true);

                SetNeighbourNumbers(x - 1, y - 1);
                SetNeighbourNumbers(x, y - 1);
                SetNeighbourNumbers(x + 1, y - 1);
                SetNeighbourNumbers(x - 1, y);
                SetNeighbourNumbers(x + 1, y);
                SetNeighbourNumbers(x - 1, y + 1);
                SetNeighbourNumbers(x, y + 1);
                SetNeighbourNumbers(x + 1, y + 1);
            }
            else
            {
                i--;
            }
        }
    }

    private void SetNeighbourNumbers(int x, int y)
    {
        if (x >= 0 && x < grid.width && y >= 0 && y < grid.height && gridMap[x, y] != TileType.MINE)
        {
            gridMap[x, y] += 1;
            SetBlock(x, y, TileType.NONE, true); // tileType doesn't matters here
        }
    }

    private void SetBlock(int x, int y, TileType tile, bool assign)
    {
        tileList[x].tile[y].SetBlock(tile);
        if (!assign) return;
        AssignSprite(x, y, tile, false);
    }

    private void AssignSprite(int x, int y, TileType tile, bool hide)
    {
        if (hide)
        {
            tileList[x].tile[y].Hide(true);
            return;
        }

        switch (tile)
        {
            case TileType.MINE:
                tileList[x].tile[y].SetSprite(bomb, true);
                tileList[x].tile[y].SetText(false);
                break;
            case TileType.FLAG:
                tileList[x].tile[y].SetSprite(flag, true);
                tileList[x].tile[y].SetText(false);
                break;
            case TileType.EMPTY:
                tileList[x].tile[y].SetSprite(graySquare, true);
                tileList[x].tile[y].SetText(false);
                break;
            default:
                tileList[x].tile[y].SetSprite(square, false); // doesn't matter the sprite...enable status is false
                tileList[x].tile[y].SetText(true);
                break;
        }
    }

    private void ButtonPress()
    {
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                var rectTransform = tileList[i].tile[j].GetComponent<RectTransform>();
                var clicked = tileList[i].tile[j].CheckCLicked();
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos) && !clicked)
                {
                    if (setFlag)
                    {
                        PutFlag(i, j);
                        break;
                    }

                    RevealBlock(i, j);
                    break;
                }
            }
        }
    }

    private void RevealBlock(int x, int y)
    {
        if (x >= 0 && x < grid.width && y >= 0 && y < grid.height)
        {
            Debug.Log($"x: {x}, y: {y}");
            tileList[x].tile[y].Hide(false);
            tileList[x].tile[y].RevealBlock();
            
            var curr = tileList[x].tile[y]._current;
            var clicked = tileList[x].tile[y].CheckCLicked();
            if (curr != TileType.EMPTY || clicked) return;
            tileList[x].tile[y].Clicked();
            RevealBlock(x - 1, y - 1);
            RevealBlock(x, y - 1);
            RevealBlock(x + 1, y - 1);
            RevealBlock(x - 1, y);
            RevealBlock(x + 1, y);
            RevealBlock(x - 1, y + 1);
            RevealBlock(x, y + 1);
            RevealBlock(x + 1, y + 1);

        }
    }

    private void PutFlag(int x, int y)
    {
        var flagged = tileList[x].tile[y].CheckFlag();
        tileList[x].tile[y].Hide(!flagged);
        AssignSprite(x, y, TileType.FLAG, !flagged);
    }
    
    private void Update()
    {
        #region Reveals The Block

        if (Input.GetMouseButtonDown(0))
        {
            ButtonPress();
        }

        #endregion

        #region Puts Flag

        if (Input.GetMouseButtonDown(1))
        {
            setFlag = true;
            ButtonPress();
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            setFlag = false;
        }

        #endregion
        
        mousePos = Input.mousePosition;
    }


    public void Reload()
    {
        SceneManager.LoadScene(0);
    }
}
