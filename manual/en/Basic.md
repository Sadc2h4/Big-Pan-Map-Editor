# Basic controls

## Layout
![MD_Description_5](images/MD_Description_5.png)
- The left side shows references，load format，mode switching，and the unit or map list.
- The center shows the 2D map view and optional 3D view.
- The right inspector shows details for the selected Spawn，Waypoint，Waterbox，or field object.
- The lower console shows load results，save results，and errors.
- The mini controller switches the active edit target between Spawn，Route，and Waterbox.
- Field mode also shows the field generator console for elapsed-day and generator controls.
***

## Overlays
- Spawn overlay displays Spawn or field object points.
- Route overlay displays Waypoints and connection lines.
- Radius overlay displays range data for objects that have radius values.
- Water overlay displays Waterboxes.
- In cave mode，the left banner changes for Spawn，Route，and Waterbox edit targets.
***

## 3D view
![MD_Description_6](images/MD_Description_6.png)
- OBJ 3D view displays cave unit models.
- Field 3D view displays field map models.
- 3D view is mainly for placement inspection. Use 2D view and the inspector for precise editing.
- If the model is not displayed，check Hocotate_Toolkit.exe and cache generation status.

- 3D mode controls differ from 2D mode. Right-drag rotates the view，and middle mouse drag moves the camera.
![MD_Description_8](images/MD_Description_8.png)
***

## Select and move
![MD_Description_12](images/MD_Description_12.png)
- Left click selects Spawn，Waypoint，Waterbox，or field objects.
- Enable Move，then drag the selected point with the left mouse button.
- In 3D view，movement and angle editing can also target the selected object，but right-drag behavior depends on the active mode.
- Floating consoles can be moved by dragging the top grip.
- Use the minimize button to reduce a floating console when it covers the map.
- Hold Ctrl to lock Waterbox or Route movement vertically，or hold Shift to lock movement horizontally.
![MD_Description_16](images/MD_Description_16.png)

***

## Add and delete
![MD_Description_7](images/MD_Description_7.png)
- Pen enables click-add mode. Click the map after enabling it.
- Eraser enables click-delete mode. Click the target point after enabling it.
- The active add target is selected from the mini controller.
- In cave mode，select Spawn Type before adding Spawn points.
- In field mode，select the target generator and add template from the field generator console.
***

## Spawn editing
- Select Spawn in the mini controller to edit layout.txt Spawn data.
- Pen adds a Spawn. The Spawn Type selector controls the new type.
- Eraser deletes a Spawn.
- Move drags the selected Spawn.
- Angle edits direction.
- Radius edits radius.
- The right inspector edits Type，Min Count，Max Count，position，angle，radius，and related values.
***

## Route editing
![MD_Description_13](images/MD_Description_13.png)
- Select Route in the mini controller to edit route.txt Waypoints.
- Pen adds a Waypoint.
- Eraser deletes a Waypoint.
- Move drags the selected Waypoint.
- Radius edits Waypoint radius.
- Link add and link delete edit Waypoint connections.
- Room-connect moves the selected Waypoint to the nearest unit connection point.
- Route arrow color indicates height differences between route points.
![MD_Description_10](images/MD_Description_10.png)
***

## Waterbox editing
![MD_Description_14](images/MD_Description_14.png)
- Select Waterbox in the mini controller to edit waterbox data.
- Pen adds a Waterbox.
- Eraser deletes a Waterbox.
- Move changes the Waterbox center position.
- Waterboxes are displayed as rectangles so the XZ range can be inspected.
- 3D view can be used to inspect height placement.
***

## Angle and Radius editing
![MD_Description_9](images/MD_Description_9.png)
- Angle edits Spawn or field object direction.
- In 2D view，drag the triangular handle or right-drag from the selected point.
- In 3D view，right-drag may rotate the camera or edit selected Spawn direction depending on mode.
- Radius edits Spawn，Route，or field object radius / range.
- Enable Radius，then drag from the selected point to the desired radius.
- Field objects without matching raw fields may not be able to save GUI radius edits.
***

## View toggles
- Toggle Spawn，Route，and Waterbox overlays from the mini controller.
- Radius overlay displays range values for objects that have radius data.
- OBJ 3D view is used for cave unit model inspection.
- Field 3D view is used for field map model inspection.
