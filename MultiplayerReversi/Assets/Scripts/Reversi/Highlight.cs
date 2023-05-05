using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField]
    private Color normalColor;

    [SerializeField]
    private Color mouseOverColor;

    private Material material;

    public ReversiChess chess;

    // Start is called before the first frame update
    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        material.color = normalColor;
    }

    private void OnMouseEnter()
    {
        material.color = mouseOverColor;
    }

    private void OnMouseExit()
    {
        material.color = normalColor;
    }

    private void OnDestroy()
    {
        Destroy(material);
    }

    public delegate void Callback(string index);
    public Callback onClicked;

    private void OnMouseDown()
    {
        if (onClicked != null) {
            onClicked(chess.BoardIndex);
        }
    }
}
