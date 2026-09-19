using System;
using System.Collections.Generic;
using Globs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Item
{
    public int ItemID;
    public int Price;
    public string Name;
    public Action OnBuy;
    public int MaxAmount;
    public int Amount;
    
    public string Category;
}

public class Category
{
    public string Name;
}

public class ShopManagerScript : MonoBehaviour
{
    public List<Item> ShopItems;
    public ChickensCountScript chickenCount;
    public GameObject itemPrefab;
    public GameObject categoryPrefab;
    public GameObject windowPanel;
    
    [Space]
    public GameObject itemHost;
    public GameObject categoryHost;

    [Header("Prefabs for buyable objects")]
    public GameObject miniBotPrefab;

    private bool _enabled;

    private void Awake()
    {
        Globals.ShopManagerScript = this;
        Globals.ShopCamera = GetComponent<Canvas>().worldCamera;
    }

    private void OnEnable()
    {
        if (_enabled) return;
        _enabled = true;
        ShopItems = new List<Item>();
        windowPanel.SetActive(true);
        
        // Core Power (cheap, frequent upgrades)
        AddCategory("Core Power");
        AddItem("Damage +5", 20, 40, "Core Power", () =>
        {
            Globals.Player.GetComponent<PlayerInteractScript>().damage += 5;
        });          // ~2 chickens
        AddItem("Attack Speed", 5, 80, "Core Power", () =>
        {
            Globals.Player.GetComponent<PlayerInteractScript>().attackSpeed -= 0.1f;
        });       // ~4 chickens
        AddItem("Bigger Hitbox", 5, 60, "Core Power", () =>
        {
            Globals.Player.GetComponent<SphereCollider>().radius += 0.5f;
        });      // ~3 chickens
        AddItem("Movement Speed", 5, 100, "Core Power", () =>
        {
            Globals.Player.GetComponent<PlayerMovementScript>().moveSpeed += 0.6f;
        });    // ~5 chickens
        AddItem("Multi Kill", 4, 120, "Core Power", () =>
        {
            Globals.Player.GetComponent<PlayerInteractScript>().killAmount += 1f;
        });

        // Economy (medium grind)
        AddCategory("Economy");
        AddItem("Money Multiplier x1.2", 6, 200, "Economy", () =>
        {
            Globals.ChickensCountScript.chickenToKrRatio *= 1.2f;
            Globals.ChickensCountScript.chickenToKrRatio = Mathf.Round(Globals.ChickensCountScript.chickenToKrRatio);
        });  // ~10 chickens
        AddItem("Lucky Chicken Chance +1%", 25, 240, "Economy", () =>
        {
            Globals.ChickenSpawnerScript.luckyChickenChance += 1;
        });   // ~12 chickens

        // Chaos (expensive = fun unlocks)
        AddCategory("Chaos");
        AddItem("Diamond Chicken Spawn", 1, 400, "Chaos");   // ~20 chickens
        AddItem("Exploding Chickens Chance +5%", 5, 500, "Chaos");     // ~25 chickens

        // Utility (quality of life)
        AddCategory("Utility");
        AddItem("MiniBot", 1, 2000, "Utility", () =>
        {
            GameObject miniBot = Instantiate(miniBotPrefab);
            Globals.ChickenSpawnerScript.miniBot = miniBot.transform;
        });               // ~100 chickens
        
        // End Game
        AddCategory("???");
        AddItem("???", 1, 500_000, "???");             // ~25 000 chickens
        
        windowPanel.SetActive(false);
    }

    private void AddItem(string itemName, int max, int price, string category, Action onBuy = null)
    {
        int id;
        if (ShopItems.Count > 0)
            id = ShopItems[^1].ItemID + 1; // increment id
        else
            id = 0;

        if (!ShopCategoryScript.AllCategories.Find(x => x.Category.Name == category))
        {
            throw new IndexOutOfRangeException("Category not found");
        }
        
        Item item = new Item
        {
            ItemID = id,
            Name = itemName,
            Price = price,
            OnBuy = onBuy,
            MaxAmount = max,
            Category = category
        };
        ShopItems.Add(item);
        
        AddItemObject(item);
    }

    private void AddItemObject(Item item)
    {
        GameObject newItem = Instantiate(itemPrefab, itemHost.transform);
        newItem.name = "Item \"" + item.Name + '"';
        newItem.GetComponent<Button>().onClick.AddListener(() =>
        {
            Purchase(newItem);
        });
        
        ShopItemScript info = newItem.GetComponent<ShopItemScript>();
        info.Item = item;
        info.shopManager = this;
        
        ShopCategoryScript.AllCategories.Find(x => x.Category.Name == item.Category).items.Add(newItem);
    }

    private void AddCategory(string categoryName)
    {
        Category category = new Category
        {
            Name = categoryName
        };
        
        AddCategoryObject(category);
    }

    private void AddCategoryObject(Category category)
    {
        GameObject newCategory = Instantiate(categoryPrefab, categoryHost.transform);
        newCategory.name = category.Name;
        
        ShopCategoryScript info = newCategory.GetComponent<ShopCategoryScript>();
        info.Category = category;
        
    }

    private void Purchase(GameObject target)
    {
        ShopItemScript buttonRef = target.GetComponent<ShopItemScript>();
        
        Item item = buttonRef.Item;
        float price = item.Price;
        
        if (chickenCount.Coins >= price)
        {
            chickenCount.Coins -= price;
            item.Amount += 1;
            item.OnBuy?.Invoke();
        }
        chickenCount.UpdateText();
    }
}
