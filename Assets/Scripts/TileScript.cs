using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileScript : MonoBehaviour
{
    public TileType _previous;
    public TileType _current;
    public TMP_Text _text;
    public Image _image;
    public Image _hideImage;

    public bool clicked = false;
    public bool flagged = false;
    // public bool mine = false;

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

    public bool CheckFlag()
    {
        flagged = !flagged;
        _current = flagged ? TileType.FLAG : _previous;
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
