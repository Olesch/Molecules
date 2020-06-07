using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molecule : MonoBehaviour
{
    public enum Directions
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    public GameObject upObject;
    public GameObject downObject;
    public GameObject leftObject;
    public GameObject rightObject;

    public GridInfo<Molecule.Directions> state = new GridInfo<Molecule.Directions>();
    private Dictionary<Directions, GameObject> dirObjects = new Dictionary<Directions, GameObject>();


    static public Directions getComplimentaryDirection(Directions dir)
    {
        switch(dir)
        {
            case Directions.UP:
                return Directions.DOWN;
            case Directions.DOWN:
                return Directions.UP;
            case Directions.LEFT:
                return Directions.RIGHT;
            case Directions.RIGHT:
                return Directions.LEFT;
            default:
                Debug.Log("Invalid Directions Complimentary!");
                return 0;
        }
    }

    static public Vector3 addDirectionPostition(Vector3 vec, Directions dir)
    {
        return vec + getDirectionPostition(dir);
    }

    static public Vector3 getDirectionPostition(Directions dir)
    {
        switch (dir)
        {
            case Directions.UP:
                return Vector3.up;
            case Directions.DOWN:
                return Vector3.down;
            case Directions.LEFT:
                return Vector3.left;
            case Directions.RIGHT:
                return Vector3.right;
            default:
                Debug.Log("Invalid Directions Vector!");
                return Vector3.zero;
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        // The Connectors to graphically show them
        dirObjects.Add( Directions.DOWN,  downObject);
        dirObjects.Add(Directions.UP, upObject);
        dirObjects.Add(Directions.LEFT, leftObject);
        dirObjects.Add(Directions.RIGHT, rightObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        state.enabled = false;
        state.dirs[Directions.UP] = false;
        state.dirs[Directions.DOWN] = false;
        state.dirs[Directions.LEFT] = false;
        state.dirs[Directions.RIGHT] = false;

        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        gameObject.SetActive(state.enabled);
        foreach(KeyValuePair<Directions, bool> it in state.dirs)
        {
            dirObjects[it.Key].SetActive(it.Value);
        }
    }
}
