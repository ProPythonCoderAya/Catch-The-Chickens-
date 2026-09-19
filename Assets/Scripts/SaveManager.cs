using System;
using System.Collections.Generic;
using UnityEngine;
using Globs;
using UnityEngine.Serialization;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SaveKey = "SaveData";
    
    [Serializable]
    public class SavedUpgrade
    {
        public int ItemID;
        public int Amount;
    }
    
    [Serializable]
    public class SavedTransform
    {
        public float posX, posY, posZ;

        public float rotX, rotY, rotZ, rotW;

        public float scaleX, scaleY, scaleZ;
    }
    
    public SavedTransform Capture(Transform t)
    {
        return new SavedTransform
        {
            posX = t.position.x,
            posY = t.position.y,
            posZ = t.position.z,

            rotX = t.rotation.x,
            rotY = t.rotation.y,
            rotZ = t.rotation.z,
            rotW = t.rotation.w,

            scaleX = t.localScale.x,
            scaleY = t.localScale.y,
            scaleZ = t.localScale.z
        };
    }
    
    public void Apply(Transform t, SavedTransform data)
    {
        t.position = new Vector3(data.posX, data.posY, data.posZ);

        t.rotation = new Quaternion(
            data.rotX,
            data.rotY,
            data.rotZ,
            data.rotW
        );

        t.localScale = new Vector3(
            data.scaleX,
            data.scaleY,
            data.scaleZ
        );
    }

    [Serializable]
    public class SaveData
    {
        public float coins;

        public SavedTransform playerTransform;

        public List<SavedUpgrade> upgrades = new();
    }

    public SaveData data = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LateStart.AddLate(_LateStart, 1f);
    }

    private void _LateStart()
    {
        LoadGame();
        ApplyToGame();
    }

    void Update()
    {
        // auto-save every ~5 seconds
        if (Time.frameCount % 3600 == 0)
        {
            SyncFromGame();
            SaveGame();
        }
    }

    // ---------------- SAVE ----------------

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            data = new SaveData();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        data = JsonUtility.FromJson<SaveData>(json);
    }

    // ---------------- SYNC FROM GAME ----------------

    public void SyncFromGame()
    {
        if (Globals.ChickensCountScript)
        {
            data.coins = Globals.ChickensCountScript.Coins;
        }

        if (Globals.Player)
        {
            data.playerTransform = Capture(Globals.Player.transform);
        }
        
        SyncUpgradesFromGame();
    }
    
    public void SyncUpgradesFromGame()
    {
        var shop = Globals.ShopManagerScript;
        if (!shop) return;

        data.upgrades.Clear();

        foreach (var item in shop.ShopItems)
        {
            data.upgrades.Add(new SavedUpgrade
            {
                ItemID = item.ItemID,
                Amount = item.Amount
            });
        }
    }

    // ---------------- APPLY TO GAME ----------------

    public void ApplyToGame()
    {
        ApplyUpgradesToGame();

        if (Globals.ChickensCountScript != null)
        {
            Globals.ChickensCountScript.Coins = data.coins;
            Globals.ChickensCountScript.UpdateText();
        }

        if (Globals.Player != null && data.playerTransform != null)
        {
            Apply(Globals.Player.transform, data.playerTransform);
        }
    }
    
    public void ApplyUpgradesToGame()
    {
        var shop = Globals.ShopManagerScript;
        if (!shop) return;

        foreach (var saved in data.upgrades)
        {
            Item item = shop.ShopItems.Find(x => x.ItemID == saved.ItemID);

            if (item == null)
                continue;

            item.Amount = saved.Amount;

            for (int i = 0; i < saved.Amount; i++)
            {
                item.OnBuy?.Invoke();
            }
        }
    }

    // ---------------- MANUAL CONTROL ----------------

    public void ForceSave()
    {
        SyncFromGame();
        SaveGame();
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        data = new SaveData();
        SaveGame();
        ApplyToGame();
    }

    private void OnDestroy()
    {
        ForceSave();
    }
}
