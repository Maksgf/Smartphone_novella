# Fantasy Messenger — Unity Edition

**Fantasy Messenger** is a Unity 6 mobile-first relationship RPG prototype. On a PC, the game is presented as a tactile handset centred in a nocturnal fantasy city; on a phone-shaped display, the interface expands to the full screen.

| Area | Included Unity implementation |
| --- | --- |
| Presentation | Responsive uGUI canvas, desktop phone frame, city silhouette, ember-gold Arcane Afterglow material palette |
| Core loop | Profile ritual, discovery cards, consequential signals, a mutual-match moment, messages, player replies, relationship value, XP, levels, skills, settings reset |
| Interaction | Unity Input System UI event module and runtime-generated buttons; no external service, database, or network connection is required |

## Open and run

Open the repository with **Unity 6000.3.22f1**. The active build scene is `Assets/Scenes/FantasyMessenger.unity`. Press Play to run the demo; use the game-window aspect ratio to switch between the desktop handset presentation and phone-sized full-screen experience.

> The first implementation is deliberately code-driven: `FantasyMessengerBootstrap.cs` constructs the interface and game state at runtime. This keeps the migrated demo self-contained, eliminates missing copied web assets, and provides a clear starting point for splitting the UI into prefabs and ScriptableObject content later.

## Structure

| Path | Purpose |
| --- | --- |
| `Assets/Scenes/FantasyMessenger.unity` | The enabled playable entry scene |
| `Assets/Scripts/Runtime/FantasyMessengerBootstrap.cs` | UI creation, responsive handset geometry, demo state, and all current interactions |
| `Assets/Scripts/Data/MessengerContentLibrary.cs` and `Assets/Scripts/Runtime/MessengerSession.cs` | Editable character, dialogue, skills, progression, settings, and asynchronous-chat model migrated into Unity-native code |
| `Assets/Prefabs/UI/` | Editable Handset Shell, Discover Card, Chat Row, Reply Option, and Event Toast prefab modules |
| `Assets/Art/` | Project-local ember mark, Veylan backdrop, and four original stained-glass character portrait assets |
| `Docs/UNITY_SETUP.md` | Hand-off notes and a practical next implementation sequence |

## References

[1] [Unity UI (uGUI) manual](https://docs.unity3d.com/Manual/UIToolkits.html)
