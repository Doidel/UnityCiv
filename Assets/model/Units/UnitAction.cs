using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct UnitAction {
    public string Name;
    public string Description;
    // Tile1Improvement, IGameBuilding, etc.
    public object Action
    {
        get
        {
            // load it in case it's not loaded yet
            if (_action is string)
                _action = Resources.Load<GameObject>((string)_action);
            return _action;
        }
        set
        {
            _action = value;
        }
    }
    private object _action;
    private Sprite imageSprite;
    public Sprite Image
    {
        get
        {
            if (imageSprite == null && !string.IsNullOrEmpty(ImageAssetPath))
                imageSprite = Resources.Load<Sprite>(ImageAssetPath);
            return imageSprite;
        }
    }
    public string ImageAssetPath;
}
