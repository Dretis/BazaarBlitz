using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlighter : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject highlighter;
    public SpriteRenderer highlighterRenderer;


    // Mouse Shit
    Vector2 mousePos => Input.mousePosition;
    public Vector2 mouseWorldPos => Camera.main.ScreenToWorldPoint(mousePos);

    public Vector3Int hoveredTilePos;
    // Start is called before the first frame update
    void Start()
    {
        hoveredTilePos = tilemap.WorldToCell(mouseWorldPos);
        highlighterRenderer = highlighter.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        hoveredTilePos = tilemap.WorldToCell(mouseWorldPos);
        var hoveredTile = tilemap.GetTile(hoveredTilePos);

        if (hoveredTile != null)
        {
            highlighter.transform.position = tilemap.CellToWorld(hoveredTilePos);

            if (Input.GetKeyDown(KeyCode.Mouse0)) // test code
            {
                Debug.Log("Hovered Tile Pos: " + hoveredTilePos);
            }
        }

    }
}
