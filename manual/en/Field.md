# Field mode

## Map list

- Field mode searches user/Abe/map and user/Kando/map，then lists field maps.
- Selecting a map loads route.txt and generator files.
- If display cache exists，Field 3D view can inspect the map model.
- Field objects differ from cave Spawn points because generator files and elapsed-day conditions control visibility.
***

## Elapsed-day conditions
![MD_Description_15](images/MD_Description_15.png)
- Changing elapsed day in the field generator console shows only active generator objects for that day.
- The same map can show different objects depending on generator file conditions.
- Objects outside the current day condition are excluded from editing.
- Editing with the day condition visible helps match in-game spawn states.
***

## Add field objects

- Select the target generator in the field generator console.
- Select an add template from Teki，Item，Pikmin，or Cave Entrance.
- Enable Pen，then click the map to add an object to the selected generator.
- The added object appears according to the current elapsed-day condition.
- Onions，rocket，bridges，gates，and seesaw blocks use distinct icons or rectangular footprints.
***

## Edit field objects

- Left click selects an existing object.
- Move edits object coordinates.
- Angle edits direction.
- Radius edits radius.
- Bridges，gates，and similar range-based objects are displayed as rectangles.
- Raw shows the selected generator object text.
- After editing raw text，press Apply raw to update the selected object.
- Some GUI edits cannot be saved if the raw object does not contain matching fields.
***

## Route and save

- Field mode can also display and edit route.txt Waypoints.
- Save writes route.txt and generator txt files back to the map folder.
- Field generator data is more conditional than cave layout data，so back up the target map folder before large edits.
