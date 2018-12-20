﻿# Galaxy Explorer

Galaxy Explorer is an open-source HoloLens application that was developed
in 6-weeks as part of the Share Your Idea program where the community
submitted and voted on ideas.

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any
additional questions or comments.

The following sections serve as guide posts to help navigate the code by
explaining some of the larger systems, both how they work and how they
interact.

# HoloLens Tools

Download all of the developer tools from the
[Microsoft Developer Website](http://lens.ms/Sa37sr)

*note*, the currently supported version of Unity is 2017.4.10f1 which can be
found on the [Unity Beta Program site](https://unity3d.com/get-unity/download/archive).

# Running in Unity

Note that by default when you point Unity at this repo, it will open a new
scene. Navigate to /Scenes and double-click MainScene to setup the editor
properly. After that, hitting Play will start the experience.

# Building Galaxy Explorer

From Unity, choose File->Build Settings to bring up the Build Settings
window. All of the scenes in the Scenes to Build section should be checked.
Choose Universal Windows Platform as the Platform. On the right side, choose
"any device" as the Target device, XAML as the UWP Build Type, 10.0.16299.0
as the SDK, check "Unity C# Projects" and then click Build. Select the folder
called 'UWP' and choose this folder.

After the build completes successfully, an explorer window will pop up.
Navigate into the UWP folder and double-click `Galaxy Explorer.sln` to launch
Visual Studio. From Visual Studio, set the Configuration to **Release**
for faster builds (doesn't use .NET Native) or **Master** to build the
type of package the Store needs (uses .NET Native).

## Building for HoloLens

Make sure to change ARM or x64 to **x86**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

## Building for Windows Phone

Make sure to change x64 or x86 to **ARM**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

## Building for Windows Desktop

Make sure to change ARM to **x64** or **x86**.
Now you can deploy to the Emulator, a Remote Device, or create a Store
package to deploy at a later time.

# CoreSystems

CoreSystems is a scene that we load that has most of our global game objects.
Things like our audio rig (which has our background music and VOManager) and
things like our Input stack. CoreSystems is loaded into any scene via
Layers so that developers and artists can run any scene (e.g.
SunView.unity) independent from running the MainScene. 

# ViewLoader

The ViewLoader manages the loading of scenes used throughout the app. 
The Viewloader lives inside CoreSystems scene.
It is responsible to load and unload scenes.
Scenes are loaded asynchronously
TransitionManager calls Viewloader in order to load and unload scenes.
All the rest of the script just hook up on ViewLoader's callbacks in order
to know when a new scene is about to be loaded and when that has been completed.

ViewLoader keeps the trail of scenes in a stack in order to know in which scene to go back.
Scenes during Introduction flow should not go in this stack as user never goes back to introduction flow.


# IntroductionFlow

Flow of introduction is managed by FlowManager and IntroFlow.cs
IntroFlow.cs lives in MainScene. In that way, in order for introduction to play,
user need to run the main scene and it doesnt run when running any other scene.

IntroFlow.cs activates FlowManager in CoreSystems scene.
FlowManager has all the dinstict stages of the introduction flow, from Logo appearance until 
the first scene that user can interact with, the galaxy view.

* Logo
* Sfx
* VO
* Earth pin
* Solar System view
* Galaxy view 

The IntroFlow.cs just helps out FlowManager in functionality that cant be just hooked up in editor in FloManager
		
When the GalaxyView scene is loaded which is the first scene that user can interact with, then thats the end of introduction.

# Galaxy

The galaxy rendering process was mostly described in Tech Process - Creating a
Galaxy (https://docs.microsoft.com/en-us/windows/mixed-reality/case-study-creating-a-galaxy-in-mixed-reality)

The code itself lives in Assets\Galaxy and is comprised of a set of
SpiralGalaxy which make up the stars and clouds (see MilkyWay.prefab).

The galaxy is the result of 3 layers:
* Stars, rendered last but first in the hierarchy
* Clouds shadows, that make up the dark spots that can be seen when looking at the Galaxy from the side
* Clouds, that make up the fluffy blue clouds that surround the stars

The galaxy itself being rendered through Unity's DrawProcedural via the
OnPostRender method. OnPostRender being called only on scripts attached to
Cameras, we use a RenderProxy script to trigger the Galaxy rendering.

# Tools

The Tools are contained in a ToolPanel, which manages the visibility of the
Back, Grab, Reset, About, and Controls UI elements. 
The ToolPanel.cs has the functionality to move the tools and implements is as the tag-along functionality.

In the ToolPanel, there are Buttons (Back, Grab, Reset, About, and Controls). 
The buttons perform an action on selection, and tools enter a toggle state. 
The ToolPanel can be raised or lowered through the manager. 

The ToolManager handles the Button settings that can be called from anywhere in script. 

All UI Buttons are in UI physics layer, in order to be able to have sfx from Audui

The tools appear in HoloLens and MR platfroms.
In Desktop, there are unity buttons on the right corner of the screen.
The buttons are Reset and About.

# PointOfInterest

PointOfInterests (POIs) are markers in scene.
They are represented in the application by a line and an indicator on the top.

Parts of a PointOfInterest:
* BillboardLine - the line that connects the interest point to interact with to an indicator at the top of the line. The line is always vertical and scales with distance as a UI element. It does not rescale with content and will always start at a target point.
* Indicator - the card that is shown above the BillboardLine.
* Card Description - a text card that appears when user focuses on the poi.

POIs have different size in different platforms.
PoiResizer.cs is the script that updates the scale of the poi elements depending on platform.

## OrbitScalePointOfInterest
Is a toggle that converts between Realistic and Simplified orbit and size views
in the solar system.

## CardPOI
Is used in the galaxy to inspect images through a magic window. Parallax for
the window and settings are incorporated in the POI_Porthole shader, and the
window depth is hidden with the POI_Occlusion shader. 
The magic window lives under the POI in the hierarchy. The CardPOI.cs is responsible 
to place the magic window at the desired position and rotation.

The font used in the PointOfInterest cards is called "Orbitron" and it can be found
[here](https://www.theleagueofmoveabletype.com/orbitron). As this font is released
under the [SIL Open Font License](http://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL_web),
developers who are interested in creating or modifying PointOfInterest cards should
treat it like any other third-party dependency and install the font on their own
development system rather than committing it directly into the git repository.

##PlanetPOI
Is used in solar system and galactic center.
Its the poi that when selected, it results in entering in a new scene.
The scene that it loads is set in PlanetPOI in editor.
The planet object lives along the poi under the same parent.
There is a reference to the planet from the poi.
The planets have a collider wich is around the mesh which is needed to be exactly as the mesh
as these colliders are used during transitions to new scenes.
Planets have an extra wider collider which is arond the planet and its much larger than the planet.
Its purpose is to be able for the planet to be selected, as the collider that its exactly around its mesh, 
might be small, a larger collider, gives the ability to select the planet easier.


## GE_POIMaker Tool

The GE_POIMaker tool can be used to help create new point of interest billboard
image files quickly and easily. This tool is located as a seperate project
within the solution. It can create one-off POI billboard .png files or batch
create all POI files when necessary. The current POI files included within 
GalaxyExplorer were generated with this tool's default settings.

The GE_POIMaker can also be used to experiment with colors, font sizes and other
effects by clicking the "Show advanced controls" checkbox. Please consult the
tooltips on each control for help on that specific control.

The GE_POIMaker tool references the Orbitron font, but will use a default font
if Orbitron is not installed on the development system.

# TransitionManager

Each view (galaxy, solar system, each planet, and the sun) is a scene in Unity.
The ViewLoader handles loading these scenes and the TransitionManager manages
how flow moves from an old scene to a new scene through callbacks from the
ViewLoader. This system handles the animations that are run between scenes to
easily flow between scenes.

First, components that arent needed are disabled, like the OrbitUpdater, POIRotation animation,
PointOfInterests, Touchscript. All these, are components that move the gameobjects in the scene and during
transition to a new scene, nothing should move the objects except the transition code.

Scenes have focus colliders. For example, solar system scene has as focus collider the sun planet collider.
A single planet scene, will have that collider as focus collider.
The idea is, that the previous and new scene's focus colliders are being transitioned from one into the other.

For example, going from solar system into earth view. The new scene, earth view, will initially be scaled, positioned and rotated
in a way so its focus collider will match exactly the transform of the earth focus collider in solar system view.
The transition code will update both scenes in that way so both focus colliders have matching transforms at any point until
the end of the transition. At the end of the transition, the new scene will have the transform values that it had when it was spawned.
So, the old and new scenes, are being modified, to transition from the previous scene's focus collider transform into the 
next scene's focus collider transform.

The transition code is in ZoomInOut.cs


# Fader

Faders control the transition alpha for any materials that use shaders
supporting _TransitionAlpha. Each Fader is responsible for changing the blend
settings of material shaders for alpha blending and returning them to their
original states when fading completes.

Use the TransitionManager.Instance.FadeContent() coroutine to fade in/out
content over time. All faders on the object passed to the function will fade
in/out. You can disable specific faders of the parent object passed to the
function by calling EnableFade() on those faders before the coroutine is
started; the function assumes that faders that have already been enabled
are handled by other logic.

Parts of the Fader:
* Faders chained in a hierarchy will, by default, only contain materials used by renderers that do not have a closer Fader parent in its hierarchical tree.
* Call EnableFade() to allow a shader to receive and use _TransitionAlpha properly and DisableFade() to restore its shader settings to their original settings for performance outside of transitions.
* When enabled, a Fader can have its alpha changed through SetAlpha.

## PointOfInterest.POIFader
CardPointOfInterest (see PointOfInterest) is a fader of this type. For shared
meshes of this fader, the _TransitionAlpha is used to individually set
transparency without breaking batch rendering for performance.

## BillboardLine.LineFader
Forwards _TransitionAlpha to the BillboardLine (see PointOfInterest) to set POI
line opacity without breaking batch rendering for performant rendering.

## ToolPanel.ToolsFader
Forwards _TransitionAlpha to the ToolPanel, Buttons, and Tools (see Tools) to
handle opacity changes.

## MaterialsFader
Has all of its materials defined in the UnityEditor instead of trying to figure
out which materials to fade through renderers. You can use this for batch
rendering or to fade a group of objects together without needing to collect a
list of faders for better performance.

## SharedMaterialFader
Identifies the first material that the fader contains and forces all of the
renderers to share the first material found. This is used to fade several 
objects in a hierarchy at the same time and ensures that all of the children 
in the fader are using the same shader for better performance.

## SunLensFlareSetter
Specifies a single material in the UnityEditor to integrate _TransitionAlpha
settings with other shader-dependent values for lens flare.

# GazeSelection

GazeSelection collects all targets that are selected through gaze by using a
ray from the HoloLens position along the device's forward vector. The cursor
indicates the position and direction of the ray in the demo, and the Cursor
script defines how targets are found with physics.

The logic supports raycast and spherical cone collisions and processes the
physics layers to test against in-order. This allows us to prioritize tool
selection first and fallback to spherical cone collisions for gaze selection
assistance when near interactive elements. If any collisions are found at any
step in physics, all targets are cached for that frame. If the spherical cone
collision finds multiple targets, those targets are prioritized and ordered
from closest to the gaze ray to farthest from the gaze ray.

The GazeSelectionManager filters the gaze selection targets down to a single
target and manages when the target changes for the entirety of the app. When
the target would change, there is a small delay introduced before the target
is actually deselected to account for an unsteady gaze. There is additional
logic to prevent gaze selection from quickly switching between objects by
keeping an object selected if switching to an object one frame would reselect
the old object in the next. This allows targets of gaze selection to turn
on/off colliders and not flicker if the colliders refer to the same target
selection object.

Only a GazeSelectionTarget can be selected. This gives the app occluder
support, allowing other colliders to block interactions. If there is a new
component that should support gaze selection, it must inherit from the
GazeSelectionTarget component and implement the IGazeSelectionTarget interface.
GazeSelectionTarget and IGazeSelecdtionTarget have function calls to respond to
gaze selection changes (selection and deselection), hand and clicker input, and
voice commands.

The following component types are GazeSelectionTargets:

* Hyperlink - used in the About Galaxy Explorer slate to explore more about Galaxy Explorer outside of the application.
* Button - the Back, Grab, Reset, About, and Controls buttons in the ToolPanel.
* Tool - the Zoom and Tilt tools in the ToolPanel.
* PointOfInterest and PointOfInterestReference - define the interactions with specific objects in the galaxy and solar system.
* PlacementControl - an invisible barrier that is enabled when the Grab tool is selected. Selecting this disables Grab and places the content in its current location.

# VOManager

VOManager is used to control how voice over clips are played and stopped. The
voice over content is broken up based on where we are in the flow of the
experience and requires a central control to ensure that it flows as desired. 

Playing a voice over clip will enter that clip into a queue to be played in a
first in, first out order. Individual clips can have their own delay from: when
they're queued up to play, to when they actually play. By default, clips will
only be played once, even if Play is called with that specific clip again.
However, each clip has the option to play more than once which is exposed
publicly.

Stopping the playing audio will fade out what's currently playing over a user
tweakable fadeout time. Stopping also has the option to clear the queue of all
clips if the user wants to start a new sequence of voice audio.

Lastly, voice over audio can be disabled and enabled globally by setting the VO
state to false, fading out any audio currently playing and clearing the queue
of clips waiting to be played.

VOManager works best when it exists in a persistent system as it inherits the
Singleton behavior pattern. Its only requirement is that an AudioSource is
placed on the same object.

# WorldAnchorHelper

WorldAnchors are Unity components that create Windows Volumetric Spatial
Anchors at a defined real world transform in space. The WorldAnchorHelper
class wraps up all of the calls to create and maintain the WorldAnchor that
defines the position where the galaxy is placed as part of the introduction
process.

One important function of WorldAnchorHelper is to listen for changes in the
locatability of the created WorldAnchor. A WorldAnchor will only be located
when the device is able to find the playspace where the WorldAnchor was
created. While the WorldAnchor is not located, the main content will be
hidden. If the content is hidden for more than five seconds then the content
is placed in the 'grabbed' state and shown so it can be placed again.

# Shaders

## Galaxy

The galaxy is using a geometry shader to expend a particle system into screen
aligned quads.

## Magic Window - POI_Porthole

Because the Galaxy renders in several passes, we didn't want to have other
passes for the background and have to manually clip them. Instead, we have a
texture for the background and we tweak the UV depending on the direction to
the camera to create a parallax effect. Essentially, we do an intersection test
between the ray to the camera to the plane where we want the virtual image to
be at, and shift the UV coordinates based on that.

## Solar System Orbits - OrbitalTrail

The orbits lines are screen space lines expanded with a geometry shader. Each
vertex have 2 positions: one for the real scale view and one for the schematic
view. The vertex shader then interpolates between those 2 positions to compute
the final position according the a reality scale that moves between 0 and 1 and
then pass it to a geometry shader that generates correctly triangulated lines
in screen space. This makes the orbits have a fixed width on screen no matter
what scale the solar system is being viewed at.

## Earth - PlanetShaderEarth

Like all the planets, most parameters are evaluated in the vertex shader as we
have a high poly version of each planet. The light is computed with a N.L
contribution that we gamma correct in order to have a realistic looking light
transition from the dark side to the light side. We also have in the alpha
channel of the Albedo texture a map of the night lights from NASA photographs
that we use to illuminate the dark side of the planet. You might notice that
there are lights in the middle of Australia … which are actually wildfires
that can be seen from space.

## Saturn - PlanetShaderSaturn

In the experience we don't have dynamic shadows enabled - as they are mostly
irrelevant for our scene - except for Saturn. The rings shadow pattern always
plays a big part of the aesthetic look of the planet, so we spent some time
making analytic shadows for it. The logic behind is to project a sphere on a
plane perpendicular to the direction to the light (the sun is approximated as
a directional light) and checking if the resulting pixel is inside of the
shadow or not. For the shadow of the planet on the rings, the world space
position of the pixel on the ring is compared to the radius of the planet
when projected on the plane that contains the pixel. For the shadow of the
rings of the planet, we project the world space position of the pixel on the
planet into the rings plane, and we compare its distance to the center of the
planet to the distance to the inner ring radius and outer ring radius. The
result gives a value in [0-1] which is used to sample a shadow texture.

## Performance Investigation

During the development process, we used various tools to investigate possible
performance optimization in our rendering tasks.
* Unity Profiler - Integrated with Unity, it gives a good overview where time is spent on the CPU and how many elements are being drawn on the screen.
* Unity's shader "compile and show code" - It shows the shader assembly and gives an idea on how expensive the shaders will be once being executed on device. A rule of thumb is that lower instructions count especially in the pixel/fragment shader is better.
* Visual Studio Graphics Debugger - Very powerful tool that gives timing on both the CPU and GPU side, can analyze shader performance and reveal hot code path on the CPU
* GPU View (Integrated with Visual Studio Graphics Debugger) - Gives precise timing on the GPU and CPU workload on device. Best used to determine if the experience is GPU bound or CPU bound.
