using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResizeGridBasedOnResolution : MonoBehaviour
{
    private GridLayoutGroup gridLayout;
    [SerializeField] private GridType gridType;
    [SerializeField] private float WIDTH_SCALE = 0.14583f;
    [SerializeField] private float HEIGHT_SCALE = 0.10546875f;

    void Start()
    {
        gridLayout = this.gameObject.GetComponent<GridLayoutGroup>();

        if (gridType == GridType.Free) {
            ResizeGrid();
        } else if (gridType == GridType.Square) {
            ResizeGridSquare();
        }
        
    }

    private void ResizeGrid()
    {
        // 현재 화면 크기에 따라 그리드 셀 크기 조절
        float cellWidth = Screen.width * WIDTH_SCALE; // 원하는 비율에 맞게 조절
        float cellHeight = Screen.height * HEIGHT_SCALE;
        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void ResizeGridSquare()
    {
        // 현재 화면 크기에 따라 그리드 셀 크기 조절
        float cellSize = Screen.width * WIDTH_SCALE; // 원하는 비율에 맞게 조절
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }
}

enum GridType {
    Free,
    Square
}