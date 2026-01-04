using UnityEngine;
using UnityEngine.UI;

public class MenuBGScroller : MonoBehaviour
{
    [SerializeField] private RawImage menuBG;
    [SerializeField] private float _x, _y;

    private void Update()
    {
        menuBG.uvRect = new Rect(menuBG.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, menuBG.uvRect.size);
    }
}
