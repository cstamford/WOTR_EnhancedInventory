using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace EnhancedInventory.Util
{
    public static class Extensions
    {
        public static string GetPath(this Transform current)
        {
            if (current.parent == null)
            {
                return "/" + current.name;
            }
                
            return current.parent.GetPath() + "/" + current.name;
        }
        public static UnitEntityData GetCurrentCharacter() {
            var firstSelectedUnit = Game.Instance.SelectionCharacter.FirstSelectedUnit;
            return (object)firstSelectedUnit != null ? firstSelectedUnit : (UnitEntityData)Game.Instance.Player.MainCharacter;
        }
    }
}
