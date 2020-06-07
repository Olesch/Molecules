using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.U2D.Animation;

public class ClickPreview : MonoBehaviour
{

    public Text connectionNumber;
    public SpriteLibraryAsset clickerLibrary;

    // own components
    private SpriteRenderer componentSpriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        componentSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void setPosition(bool active, Vector3 pos = new Vector3())
    {
        transform.position = pos;
        gameObject.SetActive(active);
        componentSpriteRenderer.sprite = clickerLibrary.GetSprite("Molecule", "BALL");
    }


    public void setDirection(Directions dir)
    {
        componentSpriteRenderer.sprite = clickerLibrary.GetSprite("Directions", dir.ToString());
    }

    public void setConnections(int remain)
    {
        if(remain==0)
        {
            connectionNumber.text = "";
        }
        else
        {
            connectionNumber.text = remain.ToString();
        }
    }


}
