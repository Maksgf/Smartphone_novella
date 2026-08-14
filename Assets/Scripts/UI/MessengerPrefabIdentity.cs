// Arcane Afterglow — semantic marker for editable UI prefab modules.
using UnityEngine;
namespace SmartphoneNovella.UI
{
    public enum MessengerPrefabKind { HandsetShell, DiscoverCard, ChatRow, ReplyOption, EventToast }
    public sealed class MessengerPrefabIdentity : MonoBehaviour
    {
        public MessengerPrefabKind kind;
        [TextArea] public string editorPurpose;
    }
}
