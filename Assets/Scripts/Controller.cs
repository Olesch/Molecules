using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.U2D.Animation;
using UnityEngine.Serialization;
using System;

public class Controller : MonoBehaviour
{
    // References
    public Grid grid;
    public Tilemap tilemap;
    public ClickPreview clickPreview;
    public Camera mainCamera;
    public Molecule molecule;

    //Caches

    // Game State
    private Molecule[,] moleculesInGrid;
    private int amountDirs=4;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("cellBounds:" + tilemap.cellBounds);
        Debug.Log("GetUsedTilesCount(): " + tilemap.GetUsedTilesCount());
        Debug.Log("localbounds: " + tilemap.localBounds);
        Debug.Log("size: " + tilemap.size);
        var t = Time.realtimeSinceStartup;

        moleculesInGrid = new Molecule[tilemap.size.x, tilemap.size.y];

        foreach (var tile in tilemap.cellBounds.allPositionsWithin)
        {
            moleculesInGrid[tile.x, tile.y] = Instantiate(molecule, grid.GetCellCenterWorld(tile), Quaternion.identity);
        }
        Debug.Log(Time.realtimeSinceStartup - t);

    }

    private Molecule.Directions getMouseDirection(Vector3 from, Vector3 to)
    {
        float angle = Vector3.SignedAngle(to - from, Vector3.right + Vector3.up, Vector3.forward);
        if (angle < 0)
        {
            if (angle < -90)
            {
                return Molecule.Directions.LEFT;
            }
            else
            {
                return Molecule.Directions.UP;
            }
        }
        else
        {
            if (angle > 90)
            {
                return Molecule.Directions.DOWN;
            }
            else
            {
                return Molecule.Directions.RIGHT;
            }
        }
    }


    // Mouse States
    private Vector3Int clickPos = new Vector3Int();
    private Vector3 placedPos = new Vector3();
    private Molecule.Directions clickDirection;
    private enum ClickMode
    {
        PLACING,
        DIRECTIONS
    }
    ClickMode clickMode = ClickMode.PLACING;

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        DrawCursor(mouseWorldPos);
        InterpretMouse(mouseWorldPos);
    }

    private void InterpretMouse(Vector3 mouseWorldPos)
    {
        Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);
        Vector3 drawCoords = grid.GetCellCenterWorld(tilePos);
        switch (clickMode)
        {
            case ClickMode.PLACING:
                if (Input.GetMouseButtonDown(0))
                {
                    clickPos = tilePos;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (clickPos == tilePos)
                    {
                        if (PlaceMolecule(clickPos))
                        {
                            placedPos = drawCoords;
                            clickMode = ClickMode.DIRECTIONS;
                        }
                    }
                }
                break;
            case (ClickMode.DIRECTIONS):
                if (Input.GetMouseButtonDown(0))
                {
                    clickDirection = getMouseDirection(placedPos, mouseWorldPos);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (clickDirection == getMouseDirection(placedPos, mouseWorldPos))
                    {
                        switch (PlaceDir())
                        {
                            case PlaceDirResult.INVALID:
                                break;
                            case PlaceDirResult.VALID_CONTINUE:
                                break;
                            case PlaceDirResult.VALID_STOP:
                                clickMode = ClickMode.PLACING;
                                break;
                        }
                    }
                }
                break;
        }
    }

    enum PlaceDirResult
    {
        INVALID,
        VALID_CONTINUE,
        VALID_STOP
    }

    private PlaceDirResult PlaceDir()
    {

        if (GetMolecule(clickPos).state.dirs[clickDirection] == false)
        {
            GetMolecule(clickPos).state.dirs[clickDirection] = true;
            GetMolecule(clickPos).UpdateAppearance();
            amountDirs--;
            if (amountDirs <= 0)
            {
                CheckForComplete(clickPos);

                float random = UnityEngine.Random.value;
                if (random < 0.2)
                {
                    amountDirs = 1;
                }
                else if (random < 0.6)
                {
                    amountDirs = 2;
                }
                else if (random < 0.85)
                {
                    amountDirs = 3;
                }
                else
                {
                    amountDirs = 4;
                }

                return PlaceDirResult.VALID_STOP;
            }
            else
            {
                return PlaceDirResult.VALID_CONTINUE;
            }
        }
        else
        {
            return PlaceDirResult.INVALID;
        }
    }

    private void CheckForComplete(Vector3Int clickPos)
    {
        foreach (Molecule molecule in moleculesInGrid)
        {
            molecule.state.visited = false;
        }

        if (Recurse(clickPos))
        {
            foreach (Molecule molecule in moleculesInGrid)
            {
                if (molecule.state.visited == true)
                {
                    molecule.state.enabled = false;
                    molecule.state.visited = false;
                    molecule.state.dirs[Molecule.Directions.UP] = false;
                    molecule.state.dirs[Molecule.Directions.DOWN] = false;
                    molecule.state.dirs[Molecule.Directions.LEFT] = false;
                    molecule.state.dirs[Molecule.Directions.RIGHT] = false;
                    molecule.UpdateAppearance();
                }
            }
        }

    }

    private bool Recurse(Vector3Int clickPos)
    {
        if (GetMolecule(clickPos).state.visited)
        {
            return true;
        }
        GetMolecule(clickPos).state.visited = true;

        foreach ( Molecule.Directions dir in Enum.GetValues(typeof(Molecule.Directions)))
        {
            Molecule neighbour;
            if (GetMolecule(clickPos).state.dirs[dir])
            {
                neighbour = GetMolecule(Vector3Int.RoundToInt(Molecule.addDirectionPostition(clickPos, dir)));
                if (neighbour && neighbour.state.enabled && neighbour.state.dirs[Molecule.getComplimentaryDirection(dir)])
                {
                    if (!Recurse(Vector3Int.RoundToInt(Molecule.addDirectionPostition(clickPos, dir))))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private bool PlaceMolecule(Vector3Int placePos)
    {
        if (GetMolecule(placePos).state.enabled == false)
        {
            GetMolecule(placePos).state.enabled = true;
            GetMolecule(placePos).UpdateAppearance();
            return true;
        }
        else
        {
            return false;
        }
    }

    private Molecule GetMolecule(Vector3Int placePos)
    {
        if (placePos.x > moleculesInGrid.GetUpperBound(0) || placePos.x < moleculesInGrid.GetLowerBound(0) ||
            placePos.y > moleculesInGrid.GetUpperBound(1) || placePos.y < moleculesInGrid.GetLowerBound(1))
        {
            return null;
        }
        return moleculesInGrid[placePos.x, placePos.y];
    }

    private void DrawCursor(Vector3 mouseWorldPos)
    {
        Vector3Int tilePos = grid.WorldToCell(mouseWorldPos);
        Vector3 drawCoords = grid.GetCellCenterWorld(tilePos);

        switch (clickMode)
        {
            case ClickMode.PLACING:
                //clickPreview.sprite clickerLibrary.GetSprite("Molecule", "Standard");
                if (tilemap.HasTile(tilePos))
                {
                    clickPreview.setConnections(amountDirs);
                    clickPreview.setPosition(true, drawCoords);
                }
                else
                {
                    clickPreview.setPosition(false);
                }
                break;
            case (ClickMode.DIRECTIONS):
                clickDirection = getMouseDirection(placedPos, mouseWorldPos);
                clickPreview.setDirection(clickDirection);
                clickPreview.setConnections(amountDirs);
                break;
        }
    }
}
