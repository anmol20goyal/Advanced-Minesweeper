using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    #region GameObjectsAndEnums

    [SerializeField] private Image tileImage;
    [SerializeField] private TileType _previous;
    [SerializeField] public TileType _current;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _image;
    [SerializeField] private Image _hideImage;

    #endregion

    #region Variables

    [SerializeField] private bool clicked = false;
    [SerializeField] private bool flagged = false;

    #endregion
    
    public void Clicked()
    {
        clicked = true;
    }

    public bool CheckCLicked()
    {
        return clicked;
    }

    public void SetSprite(Sprite _sprite, bool enabledStatus)
    {
        _image.enabled = enabledStatus;
        if (!enabledStatus) return;
        _image.sprite = _sprite;
    }

    public void Empty()
    {
        GetComponent<Image>().enabled = false;
        _image.enabled = false;
    }

    public void SetText(bool enableText)
    {
        _text.enabled = enableText;
        if (!enableText) return;
        _text.text = _current.ToString().Substring(_current.ToString().Length - 1);
    }

    public void Hide(bool hideStatus)
    {
        _hideImage.enabled = hideStatus;
    }

    public void SetUnSetFlag()
    {
        flagged = !flagged;
        _current = flagged ? TileType.FLAG : _previous;
        
    }

    public bool CheckFlagged()
    {
        return flagged;
    }

    public void RevealBlock()
    {
        if (_current != TileType.MINE && _current != TileType.EMPTY && _current == _previous)
        {
            SetSprite(null, false);
            SetText(true);
        }
    }

    public void RevealMine()
    {
        if (_previous == TileType.MINE)
        {
            Hide(false);
            if (CheckFlagged())
            {
                tileImage.color = Color.blue;
            }
        }
        else
        {
            if (CheckFlagged())
            {
                tileImage.color = Color.black;
            }
        }
    }
    
    public void SetBlock(TileType type)
    {
        if (type != TileType.NONE)
        {
            _current = type;
        }
        else
        {
            _current += 1;
        }

        _previous = _current;
    }
}
