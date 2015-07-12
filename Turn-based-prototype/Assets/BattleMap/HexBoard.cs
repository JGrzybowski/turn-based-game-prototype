using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class HexBoard : MonoBehaviour, IEnumerable<Hex>{

    private Hex[,] cells;
    [SerializeField]
    private int columns = 5;
    [SerializeField]
    private int cellsPerColumn = 4;
    [SerializeField]
    private GameObject hexPrefab;
    [SerializeField]
    private float hexWidth;

    private float offsetX;
    private float offsetY;
    private float scale;

    private void Awake()
    {
        cells = new Hex[columns,cellsPerColumn];
        var gridw = GetComponent<RectTransform>().rect.width;
        var hexw = hexPrefab.GetComponent<RectTransform>().rect.width;
        var gridh = GetComponent<RectTransform>().rect.height;
        var hexh = hexPrefab.GetComponent<RectTransform>().rect.height;
        var widthScale = (gridw / (0.75f * (columns+1))) / hexw;
        var heightScale = (gridh / (cellsPerColumn)) / hexh;

        scale = Mathf.Min(widthScale, heightScale);
        offsetX = +hexw * scale / 2;
        offsetY = -hexh * scale / 2;

        //print("h" + heightScale + " w" + widthScale);

        Vector3 startingPosition = gameObject.transform.position;
        for(int i=0; i < columns; i++)
            for(int j = 0; j < cellsPerColumn; j++)
            {
                var cell = (GameObject)Instantiate(hexPrefab, calculateHexTransform(i, j, hexWidth*scale/2 ,startingPosition), Quaternion.Euler(0,0,0));
                cells[i, j] = cell.GetComponent<Hex>();
                cells[i, j].Position = ArrayToAxial(i, j);
                cell.transform.SetParent(this.gameObject.transform);
                cell.GetComponent<RectTransform>().localScale = new Vector3(scale,scale,scale);
            }
        
    }

    

    public Hex this[int q, int r]
    {
        get { return cells[q, r + q / 2]; }
        set { cells[q, r + q / 2] = value; }
    }
    public Hex this[Vector2 position]
    {
        get { return this[(int)position.x, (int)position.y]; }
        set { this[(int)position.x, (int)position.y] = value; }
    }

    private Vector2 AxialToArray(int q, int r) { return new Vector2(q, r + (q / 2)); }
    private Vector2 ArrayToAxial(int col, int row) { return new Vector2(col, row - (col / 2)); }

    private Vector2 calculateHexTransform(int col, int row, float hexSize, Vector3 startingPosition)
    {
        Vector2 offset = new Vector2(offsetX, offsetY);
        Vector2 result = new Vector3(col * 1.5f * hexSize, - row * Mathf.Sqrt(3f) * hexSize);
        result += offset*1.5f;
        if (col % 2 == 1)
            result.y -= (Mathf.Sqrt(3.0f) / 2) * hexSize;
        return result;
    }

    public IEnumerator<Hex> GetEnumerator()
    {
        foreach (Hex h in cells)
            yield return h;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return cells.GetEnumerator();
    }

}
