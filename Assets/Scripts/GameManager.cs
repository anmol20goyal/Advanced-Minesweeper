using System;
using System.Collections;
using System.Collections.Generic;
using Packages.Rider.Editor.UnitTesting;
using TMPro;
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
    private List<int[]> mineMap = new List<int[]>();

    public GameObject block;
    public GameObject gridPanel;
    
    [Header("Sprites")] [Space(20)]
    public Sprite bomb;
    public Sprite square;
    public Sprite graySquare;
    public Sprite flag;

    [Header("UI")] [Space(20)] 
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TMP_Text flagsLeftTxt;
    
    #endregion

    #region EnumsAndVariables
    
    public enum DifficultyType
    {
        EASY,
        MEDIUM,
        HARD
    }
    
    [Header("EnumsVariables")] [Space(20)]
    public DifficultyType difficultyType;

    #endregion
    
    #region Variables

    [Header("Variables")] [Space(20)] 
    [SerializeField] private bool gameOver;
    private bool setFlag = false;
    private Vector2 mousePos;
    [SerializeField] private int totalFlagsLeft;

    #endregion

    #region Lists

    [Header("Lists")]
    [Space(20)]
    private List<GridSprites> tileList = new List<GridSprites>();

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

        GameExtreme(true);
    }

    private void GameExtreme(bool hide)
    {
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                if (hide)
                {
                    tileList[i].tile[j].Hide(true);    
                }
                else
                {
                    tileList[i].tile[j].RevealMine();
                }
            }
        }
    }
    
    private void PlaceBombs(DifficultyType type, int bombs)
    {
        // flag no assigned
        totalFlagsLeft = bombs;
        flagsLeftTxt.text = totalFlagsLeft.ToString();
        
        for (int i = 0; i < bombs; i++)
        {
            int x = Random.Range(0, grid.width);
            int y = Random.Range(0, grid.height);
            if (gridMap[x, y] != TileType.MINE)
            {
                mineMap.Add(new [] { x, y });
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
            SetBlock(x, y, TileType.NONE, false); // tileType doesn't matters here
        }
    }

    private void SetBlock(int x, int y, TileType tile, bool assign)
    {
        tileList[x].tile[y].SetBlock(tile);
        if (!assign) return;
        AssignSprite(x, y, tile);
    }

    private void AssignSprite(int x, int y, TileType tile)
    {
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
                tileList[x].tile[y].SetSprite(null, false); // doesn't matter the sprite...enable status is false
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
                        AssignFlag(i, j);
                        goto CHECK_END;
                    }

                    RevealBlock(i, j);
                    goto CHECK_END;
                }
            }
        }
        
        CHECK_END:
        CheckGameEnd();
    }

    private void RevealBlock(int x, int y)
    {
        if (x >= 0 && x < grid.width && y >= 0 && y < grid.height)
        {
            tileList[x].tile[y].Hide(false);
            tileList[x].tile[y].RevealBlock();

            var flagged = tileList[x].tile[y].CheckFlagged();
            var curr = gridMap[x, y];
            var clicked = tileList[x].tile[y].CheckCLicked();

            if (flagged) return;
            if (gridMap[x, y] == TileType.MINE)
            {
                GameEnded();
                gameOver = true;
                return;
            }

            tileList[x].tile[y].Clicked();
            
            if (curr != TileType.EMPTY || clicked) return;
            tileList[x].tile[y].Empty();
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

    private void GameEnded()
    {
        #region RevealAllMines

        gameOverUI.SetActive(true);
        gameOverUI.transform.GetChild(0).GetComponent<TMP_Text>().text = "You Lost!!";
        GameExtreme(false);

        #endregion
    }
    
    private void CheckGameEnd()
    {
        bool end = true;
        if (totalFlagsLeft == 0)
        {
            /*foreach (var mine in mineMap)
            {
                var x = mine[0];
                var y = mine[1];
                if (!tileList[x].tile[y].CheckFlagged())
                {
                    end = false;
                    break;
                }
            }*/

            for (int i = 0; i < grid.width; i++)
            {
                for (int j = 0; j < grid.height; j++)
                {
                    var tt = gridMap[i, j];
                    if (tt == TileType.MINE)
                    {
                        if (!tileList[i].tile[j].CheckFlagged())
                        {
                            end = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!tileList[i].tile[j].CheckCLicked())
                        {
                            end = false;
                            break;
                        }
                    }
                }
            }

            gameOverUI.SetActive(end);
            gameOver = end;
        }
    }
    
    private void AssignFlag(int x, int y)
    {
        tileList[x].tile[y].SetUnSetFlag();
        var flagged = tileList[x].tile[y].CheckFlagged();

        if (flagged)
        {
            tileList[x].tile[y].Hide(false);
            AssignSprite(x, y, TileType.FLAG);
            totalFlagsLeft--;
        }
        else
        {
            tileList[x].tile[y].Hide(true);
            AssignSprite(x, y, gridMap[x, y]);
            totalFlagsLeft++;
        }

        flagsLeftTxt.text = totalFlagsLeft.ToString();
    }
    
    private void Update()
    {
        #region Reveals The Block

        if (Input.GetMouseButtonDown(0) && !gameOver)
        {
            ButtonPress();
        }

        #endregion

        #region Puts Flag

        if (Input.GetMouseButtonDown(1) && !gameOver)
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
