using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategoryScript : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI categoryName;

    [Space]

    public Category Category;
    public List<GameObject> items = new();

    private bool _selected;
    private Button _button;

    // keeps track of all categories so only one can be selected
    public static List<ShopCategoryScript> AllCategories = new();

    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value) return;

            _selected = value;
            OnSelect();
        }
    }

    private void Awake()
    {
        _button = GetComponent<Button>();

        AllCategories.Add(this);
    }

    private void Start()
    {
        categoryName.text = Category.Name;

        _button.onClick.AddListener(Select);
        
        OnSelect(); // update at start
    }

    private void Select()
    {
        // deselect all others
        foreach (var cat in AllCategories)
        {
            cat._selected = false;
            cat.OnSelect();
        }

        Selected = true;
    }

    private void OnSelect()
    {
        // prevent clicking selected category again
        _button.interactable = !Selected;

        foreach (var item in items)
        {
            item.SetActive(Selected);
        }
    }

    private void OnDestroy()
    {
        AllCategories.Remove(this);
    }
}
