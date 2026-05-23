# View / Save

## Overlays

- Spawn overlay displays Spawn or field object points.
- Route overlay displays Waypoints and connection lines.
- Radius overlay displays range data for objects that have radius values.
- Water overlay displays Waterboxes.
- In cave mode，the left banner changes for Spawn，Route，and Waterbox edit targets.

## 3D view

- OBJ 3D view displays cave unit models.
- Field 3D view displays field map models.
- 3D view is mainly for placement inspection. Use 2D view and the inspector for precise editing.
- If the model is not displayed，check Hocotate_Toolkit.exe and cache generation status.

## Save operations

- Save writes the current edit target.
- Save All writes multiple edit targets together.
- Cave mode targets layout，route，waterbox，and archive repack.
- Field mode targets route.txt and generator txt files.
- Save results are shown in the console.
- Back up target folders before saving.

## Distribution and embedded resources

- Button icons，spawn icons，home assets，pretty images，manual md，and manual images are embedded during build.
- Release publish is intended to output a single BigPanMapEditor.exe.
- Settings json and cache folders are created in the runtime environment.

## Manual images

- manual_editor.html can add images into manual/images.
- Inserted images use `![description](images/file.png)` in md.
- Images under manual/images are embedded as manual images during build.
