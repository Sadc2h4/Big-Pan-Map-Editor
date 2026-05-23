# Setup

## Initial setup
- Hocotate Tool Kit.exe is required for this app to work correctly. Prepare it before using the editor.
![MD_Description_2](images/MD_Description_2.png)
***

## Home screen
- Big Pan Map Editor starts on the home screen.
- The ISO / GCR / root field accepts ISO files，GCR files，extracted disc folders，
- or the cave mapunits arc folder. The Hocotate_Toolkit field should point to Hocotate_Toolkit.exe.

- Cave Gen Editor opens the cave unit editor.
- Field Gen Editor opens the field map editor.
- About Hocotate Tool Kit opens the Hocotate Tool Kit repository page.
- Editor Manual opens this manual.
- The language selector affects the home screen and the main editor UI.
***

## Input targets
- When ISO / GCR is selected，the app tries disc extraction with Hocotate_Toolkit.exe.
![MD_Description_4](images/MD_Description_3.png)

- An extracted disc folder that contains sys/files can be loaded without extraction.
- Cave mode can load user/Mukki/mapunits/arc directly. Each unit folder under arc appears in the list.
- A single unit folder that directly contains arc.szs and texts.szs can also be opened.
- Field mode searches user/Abe/map and user/Kando/map，then lists available field maps.
![MD_Description_1](images/MD_Description_1.png)
***

## Cache
- 3D models and pretty previews use generated cache data.
![MD_Description_4](images/MD_Description_4.png)

- Cave mode supports per-unit cache generation and all-unit cache generation.
- Field mode uses map-level display cache.
- If Hocotate_Toolkit.exe is not configured，cache steps that require SZS extraction are skipped.
- Cache folder buttons open the generated unit cache and image cache locations.
