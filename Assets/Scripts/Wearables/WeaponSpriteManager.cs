using System.Collections.Generic;
using UnityEngine;

public class WeaponSpriteManager : MonoBehaviour
{
    private static WeaponSpriteManager _instance;
    public static WeaponSpriteManager Instance => _instance;

    [SerializeField] private string[] folderPaths = { 
        "Wearables/WeaponSprites/Cleave", 
        "Wearables/WeaponSprites/Pierce", 
        "Wearables/WeaponSprites/Smash", 
        "Wearables/WeaponSprites/Ballistic", 
        "Wearables/WeaponSprites/Magic", 
        "Wearables/WeaponSprites/Splash", 
        "Wearables/WeaponSprites/Consume", 
        "Wearables/WeaponSprites/Aura", 
        "Wearables/WeaponSprites/Throw", 
        "Wearables/WeaponSprites/Shield", 
    };

    private Dictionary<Wearable.NameEnum, WearableSprite> wearableSprites;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadAllWearableSprites();
        }
    }

    private void LoadAllWearableSprites()
    {
        wearableSprites = new Dictionary<Wearable.NameEnum, WearableSprite>();

        foreach (var folderPath in folderPaths)
        {
            Debug.Log("Loading " + folderPath);
            var sprites = Resources.LoadAll<WearableSprite>(folderPath);
            foreach (var sprite in sprites)
            {
                if (!wearableSprites.ContainsKey(sprite.WearableNameEnum))
                {
                    wearableSprites.Add(sprite.WearableNameEnum, sprite);
                }
            }
        }
    }

    public Sprite GetSprite(Wearable.NameEnum wearableNameEnum, PlayerGotchi.Facing facingDirection)
    {
        if (wearableSprites.TryGetValue(wearableNameEnum, out var wearableSprite))
        {
            switch (facingDirection)
            {
                case PlayerGotchi.Facing.Front:
                    return wearableSprite.FrontView;
                case PlayerGotchi.Facing.Right:
                    return wearableSprite.RightView;
                case PlayerGotchi.Facing.Left:
                    return wearableSprite.LeftView;
                case PlayerGotchi.Facing.Back:
                    return wearableSprite.BackView;
            }
        }
        return null;
    }

    public int GetSpriteOrder(Wearable.NameEnum wearableNameEnum, PlayerGotchi.Facing facingDirection)
    {
        if (wearableSprites.TryGetValue(wearableNameEnum, out var wearableSprite))
        {
            switch (facingDirection)
            {
                case PlayerGotchi.Facing.Front:
                    return wearableSprite.FrontViewOrder;
                case PlayerGotchi.Facing.Right:
                    return wearableSprite.RightViewOrder;
                case PlayerGotchi.Facing.Left:
                    return wearableSprite.LeftViewOrder;
                case PlayerGotchi.Facing.Back:
                    return wearableSprite.BackViewOrder;
            }
        }
        return -1; // Return a default value if not found
    }
}
