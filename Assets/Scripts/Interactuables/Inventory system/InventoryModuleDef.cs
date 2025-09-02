using UnityEngine;

[CreateAssetMenu(fileName = "ModuleDef", menuName = "Inventory/Module Def")]
public class InventoryModuleDef : ScriptableObject
{
    public string displayName = "Pocket";
    public int rows = 6;
    public int cols = 6;

    public int Capacity => Mathf.Max(1, rows * cols);
}
