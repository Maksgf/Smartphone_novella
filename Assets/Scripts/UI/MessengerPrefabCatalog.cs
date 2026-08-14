// Arcane Afterglow — central reference asset for the Unity UI prefab family.
using UnityEngine;
namespace SmartphoneNovella.UI
{
    [CreateAssetMenu(fileName = "FantasyMessengerPrefabCatalog", menuName = "Fantasy Messenger/Prefab Catalog")]
    public sealed class MessengerPrefabCatalog : ScriptableObject
    {
        public GameObject handsetShell;
        public GameObject discoverCard;
        public GameObject chatRow;
        public GameObject replyOption;
        public GameObject eventToast;
    }
}
