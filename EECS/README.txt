Electrical Engineering & Computer Science - The Game

Description:

EECS is a single/multiplayer combat game inspired by core and advanced concepts in Mathematics, Computer Science, and Electrical Engineering. Players choose between mathematicians and engineers and battle in EECS-inspired environments with EECS-inspired weapons and move lists. From Integrals and Circuits to Vectors and AC and DC Currents, there's now no doubt that knowledge is the true power.




Characters:

Mathematician 
(Powers)-> Integral, Line Integral, Summation, Vectors (Normal Vector & Angled Vector)

Electrical Engineer 
(Powers)-> Low Pass Filter, High Pass Filter, (HAND WEAPON???????), Current (DC/AC)

Software Engineer
(Powers)-> ???


Player One Controller (See Controls in Starter Assets Input):

Backward/Forward, Forward/Backward 		- A/D
Weapon Equip Toggle				- I
Punch/Kick, (Weapon Equip) Swing/Jab		- P/K
Special Hand Weapon Attack (Weapon Unequip)	- H
Jump (+ Backward/Forward) (Weapon Unequip)	- Space (+ A/D)
Crouch						- C


How To Add A New Move

Edit:

Starter Assets (Input Assets) : When a new button needs to be mapped to a new control/button.

Starter Assets Input C# File : When a new button has been mapped to a control. Updating this script allows the C_Controller to access the input values via the InputValue (_input) class.

Character Animator Controller : When an animation needs to be mapped to a control. Variables should be created to control the State Machine flow and the values will be updated continuously in the C_Controller script. (Examples in the script)

C_Controller Script : When changes to the Player controls need to be made, including new moves, updated moves, weapons, movements and more. Awake() and Start() functions are called once and the Update() function is called every frame to update the scene.



TUTORIALS

How To Create A Prefab (Blender)

1. Import a reference image by switching to Object Mode (if not already there) and choose "Add", then "Image" and "Reference"

2. Add a Plane to the scene and delete all except for one vertex in Edit Mode -> Vertex

3. Right Click the vertex in Edit Mode and "Extrude Vertices". Trace the reference image by repeating this process. Extruded vertices are best done interatively where the last vertex added is the vertex that should be extruded from.

4. Move to the Edges tab in Edit Mode. Use Shift + Left Click to choose connecting edges and then push "F" to populate a face between them.

5. Move to the Faces tab in Edit Mode and select all of the faces before right clicking and choosing "Extrude Faces". From there, adjust the depth/width of the prefab/object.

6. (For Weapons) Create an empty object on the player and place/adjust the new asset there.

7. (For Weapon) Create a new Serialized GameObject variable in the script and drag the new asset there. Use Destroy() and Instantiate() functions for equip/unequip effect.



How to Create An Animated Scene Prefab

1. Open the Editor Type Tab in the top left and choose the Dope Sheet view.

2. From the Dope Sheet view, switch to the Action Editor and begin adding keyframes.

3. How to add Keyframes: https://youtu.be/CBJp82tlR3M?si=mCNKoK7KYcKQd0ra

4. Export the object as an FBX prefab for Unity