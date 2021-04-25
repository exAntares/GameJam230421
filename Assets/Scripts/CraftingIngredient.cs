using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CraftingIngredient : MonoBehaviour {
    public string CraftingId;
    private CraftingSystem _craftingSystem;

    private void OnValidate() {
#if UNITY_EDITOR
        var assetPath = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(assetPath) && transform.root == transform) {
            CraftingId = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
        }
#endif
    }

    private void Awake() {
        _craftingSystem = FindObjectOfType<CraftingSystem>();
        _craftingSystem.OnCraftingItemSpawned(this);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        var craftingIngredient = other.GetComponent<CraftingIngredient>();
        if (craftingIngredient != null) {
            _craftingSystem.OnCraftingTriggerEnter2D(this, craftingIngredient);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        var craftingIngredient = other.GetComponent<CraftingIngredient>();
        if (craftingIngredient != null) {
            _craftingSystem.OnCraftingTriggerExit2D(this, craftingIngredient);
        }
    }
}