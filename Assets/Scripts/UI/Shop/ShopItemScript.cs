using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemScript : MonoBehaviour
{
    public TextMeshProUGUI itemPrice;
    public TextMeshProUGUI itemName;
    public Image itemIcon;
    public Slider itemMaxOutSlider;
    
    public ShopManagerScript shopManager;
    
    public Item Item;

    private Button _self;
    
    private void Start()
    {
        _self = GetComponent<Button>();
        if (Item.Price > 10_000)
            itemPrice.text = "??? kr";
        else
            itemPrice.text = Item.Price + " kr";
        itemName.text = Item.Name;
        itemMaxOutSlider.maxValue = Item.MaxAmount;
    }

    void Update()
    {
        itemIcon.fillAmount = Mathf.Min(shopManager.chickenCount.Coins / Item.Price, 1.0f);
        itemMaxOutSlider.value = Item.Amount;
        if ((int)itemMaxOutSlider.value == Item.MaxAmount)
        {
            _self.interactable = false;
        }
    }
}
