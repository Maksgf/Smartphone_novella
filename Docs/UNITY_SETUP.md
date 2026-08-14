# Unity Adaptation Notes

## Migration decision

The supplied repository is a clean **Unity 6.3 URP/2D** project. The web prototype has therefore been migrated as a Unity-native `uGUI` runtime rather than copied as React files inside Unity. The primary interaction and content vocabulary has been retained: discovery, signals, mutual matches, an asynchronous-feeling reply, affection, XP, level progression, skills, profile, and reset.

| Original demo concept | Unity equivalent |
| --- | --- |
| Desktop browser frame | Responsive `CanvasScaler` layout with a centered phone frame on landscape displays |
| Babylon city backdrop | Runtime-built Veylan night silhouette with moon and lantern accents |
| React state hook | `FantasyMessengerBootstrap` local runtime state |
| Component screens | View enum plus runtime screen rendering methods |
| Original web state data | `MessengerContentLibrary` ScriptableObject and `MessengerSession` state machine |
| Broken copied image paths | Project-local ember mark, Veylan backdrop, and four SVG portrait assets, with no external asset dependency |

## Working with the project

Open `Assets/Scenes/FantasyMessenger.unity` and press Play. The scene owns one bootstrap component. It constructs the canvas, event system, desktop stage, handset, and every interaction at runtime. The build settings already point to this scene.

The project uses Unity’s Input System UI module so the generated buttons can receive pointer and touch input. Keep `com.unity.inputsystem` installed when updating packages.

## Editable UI modules

The `Assets/Prefabs/UI` folder contains the **Handset Shell**, **Discover Card**, **Chat Row**, **Reply Option**, and **Event Toast** modules. Each carries a `MessengerPrefabIdentity` marker and named child slots so a Unity designer can replace the runtime-built presentation module-by-module. `Assets/Data/FantasyMessengerPrefabCatalog.asset` is the central editor reference asset for this family.

## Recommended production split

| Next technical step | Reason |
| --- | --- |
| Move character data and reply paths into ScriptableObjects | Enables scalable dialogue branches and content authoring without editing code |
| Convert handset, cards, rows, and modal into prefabs | Makes art direction and responsive tuning faster in the Unity editor |
| Replace glyph portraits with commissioned or generated sprites imported as Addressables | Restores distinctive character identity without bundling source-project storage URLs |
| Save the player state through a versioned persistence layer | Lets affection, XP, skills, and message history survive a restart |

## References

[1] [Unity UI (uGUI) manual](https://docs.unity3d.com/Manual/UIToolkits.html)
[2] [Unity Input System package manual](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)
