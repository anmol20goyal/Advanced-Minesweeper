using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid
{
    public int width, height;

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}
