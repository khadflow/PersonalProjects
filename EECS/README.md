Electrical Engineering & Computer Science - The Game

Description:

EECS is a single/multiplayer combat game inspired by core and advanced concepts in Mathematics, Computer Science, and Electrical Engineering.
Players will choose between mathematicians, software and electrical engineers and take them to battle in EECS-inspired environments with EECS-inspired weapons and move lists.
From Integrals and Circuits to Vectors and DC Current, there's now no doubt that knowledge is the true power.


Characters:

Mathematician 
(Powers)-> Integral, Summation, Vectors (Normal Vector & Angled Vector)

Electrical Engineer 
(Powers)-> Low Pass Filter, Summation, Clk (DC Current)

Software Engineer
(Powers)-> TBD


Keyboard Controller (See Controls in Starter Assets Input):

Backward/Forward, Forward/Backward 		- A/D
Weapon Equip Toggle				- I
Punch/Kick/Jab/Hit (Weapon Equip/Uneuquip)	- P/K/J/L
Hand Weapon Attack (Weapon Equip/Uneuquip)	- H
Jump (+ Backward/Forward) (Weapon Equip/Uneuquip)- Space (+ A/D)
Crouch	(+ Backward/Forward)			- C (+ A/D)
Special Move (Weapon Equip/Uneuquip)		- U
Block						- B

Gamepad Controller

Backward/Forward, Forward/Backward 		- Analog Stick
Weapon Equip Toggle				- LB
Punch/Kick/Jab/Hit (Weapon Equip/Uneuquip)	- B/X/Y/A
Hand Weapon Attack (Weapon Equip/Uneuquip)	- RB
Jump (+ Backward/Forward) (Weapon Equip/Uneuquip)- Upward Analog Stick
Crouch	(+ Backward/Forward)			- Downward Analog Stick (+ Left/Right Analog)
Special Move (Weapon Equip/Uneuquip)		- LT
Block						- RT



##### TUTORIALS #####

How To Add A New Move


Edit Starter Assets Input System (Input Assets) : When a new button needs to be mapped to a new control/button, create it in the Input Assets.

Edit Starter Assets Input C# File : Update this script to allow the C_Controller to access the input values via the InputValue (_input) class. This will allow the script to know when the button was pushed and to act appropriately.

Character Animator Controller : When an animation needs to be mapped to a new control. Variables should be created within the animator to control the State Machine flow and the values will be updated continuously in the C_Controller script. An associated animator ID needs to be created to manage the value created in the animator (See examples in the player script)

Edit C_Controller Script : Awake() and Start() functions are called once and the Update() function is called every frame to update the scene. Checks for controller updates like push and release need to be monitored in Update().


How To Create A Prefab (Blender)

1. Import a reference image by switching to Object Mode (if not already there) and choose "Add", then "Image" and "Reference"

2. Add a Plane to the scene and delete all except for one vertex in Edit Mode -> Vertex

3. Right Click the vertex in Edit Mode and "Extrude Vertices". Trace the reference image by repeating this process. Extruded vertices are best done interatively where the last vertex added is the vertex that should be extruded from.

4. Move to the Edges tab in Edit Mode. Use Shift + Left Click to choose connecting edges and then push "F" to populate a face between them.

5. Move to the Faces tab in Edit Mode and select all of the faces before right clicking and choosing "Extrude Faces". From there, adjust the depth/width of the prefab/object.

6. (For Weapons) Create an empty object on the player and place/adjust the new asset there.

7. (For Weapon) Create a new Serialized GameObject variable in the script and drag the new asset there. Use Instantiate() and Destroy() functions for equip/unequip effect.



How to Create An Animated Scene Prefab

1. Open the Editor Type Tab in the top left and choose the Dope Sheet view.

2. From the Dope Sheet view, switch to the Action Editor and begin adding keyframes.

3. How to add Keyframes: https://youtu.be/CBJp82tlR3M?si=mCNKoK7KYcKQd0ra

4. Export the object as an FBX prefab for Unity



References:

Unity Starter Assets (including a 3rd Person Character Controller)
https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526?_ga=2.50460394.2113761589.1623841518-1686674300.1578303282&_gl=1*64zyzb*_ga*MTY4NjY3NDMwMC4xNTc4MzAzMjgy*_ga_1S78EFL1W5*MTYyMzkxOTE1MC44LjAuMTYyMzkxOTE1MC42MA..&utm_source=YouTube&utm_medium=social&utm_campaign=evangelism_global_generalpromo_2021-06-17_starter-assets-tp-assetstore
