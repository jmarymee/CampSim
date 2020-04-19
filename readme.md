# CampSimGithub


## Prerequisites
- At least 20 GB of free space, 8 GB of memory, and as much CPU/GPU as possible, although GPU is not stricly necessary. 
- The **binassets** folder (containing non-checked in assets) from me personally. It is around 2 GB.
- Visual Studio 2017 with Unity support installed, it can work with VS Code too, but it hasn't been tried on this yet.

## Installation instructions (probably obsolete)

1.	Make sure you have a recent version of Git (**git –version**, I am using 2.21.0.windows.1)
2.	Enable **git-lfs** for your user with **git lfs install**. If you don’t want to do this, you have to do step 7a and 7b (which is no big deal)
3.	Install Unity Hub ( I currently am using 2.0.0)
4.	Install a recent version of Unity (I am using 2019.1.2f1)
5.	Make sure Unity works (open it up add a sphere to a scene, move it around)
6.	Find a place where you will keep the project. It should have like 30+ GB free, preferably more so you can make safety clones quickly with no worries. 
7.	Clone the **onefloortestforparking** project into a directory. Will take a few minutes. 
    - a.	If you didn’t enable **git-lfs** globally, then after it has cloned, enter the directory and enable it locally (**git lfs install --local**) – see <https://github.com/git-lfs/git-lfs/wiki/Installation> for more inforamation
    - b.	Now do a **git pull** which should pull any missing lfs managed files that were left behind.
    - c.	The github repo is 1.86 GB (which is too big, waiting for a complaint email)
	 - d.	After cloning to disk it is like 5.86 GB
	 - e.	After compilation and using it for a while it will need around 12-13 GB.
8.	Download the Arcore unity package from <https://developers.google.com/ar/develop/downloads>
    - a.	It is currently using **1.8.0** but looks like they are up to 1.9.0 now. However I tried 1.9.0 and it is broken (doesn't compile), but can be easily fixed, see below.
9.	From Hub, open the project. It should take a while to compile all the scripts, bake the texture maps, etc. the first time you open it (like 30 minutes)
	 - a.	The status bar seemingly doesn’t update if you don’t move the mouse. I assume it is still working in the background… very disconcerting.
	 - b.	This would probably be a lot faster if we removed some things that are no longer being used, like the floor plan bitmaps
10.	Copy the folders in the binary assets (**binassets**) to the Assets subdirectory, except for the **willow** subdirectory to **Assets/Resources**. It will compile a bunch of assets, taking up to 20 minutes.
11.	Go to **Assets/Resources/TreesAndShrubs** and see if the icons look like trees and shrubs. If not left click on each of them and do a **Reimport** individually to refresh the prefab instance.


## Testing
- Go to the **Project Window**, then the **Assets/_Scenes** Folder. 
- Double click on the **CampusScene1** scene to load it into the **Scene View**
- Configure a window with resolution **2048x1546** in the **Game View** and select it
- Now hit the run button. After a few seconds - maybe up to 15  the first time - The scene should load
- If you can't see the whole UI, note the scale slider on the top of the **Game View**, make sure it is set to the far left.

## Problems and Solutions
- Compiler Error: Arcore **1.9.0 **does not compile - missing variable in **InstantPreviewWarnings.cs**
     -   Arcore **1.9.0** has a problem whereby they removed a variable definition in one of the files. A quick fix is to either add it back or delete the reference
     - 	File where lines were deleted was: **D:\Unity\onefloortestforparking\Assets\GoogleARCore\SDK\InstantPreview\Scripts\InstantPreviewWarnings.cs**
     - 	Deleted lines (as defined in 1.8.0) were at line number 61 : 
**public const string InstantPreviewWarningPrefabPath =            "Assets/GoogleARCore/SDK/InstantPreview/Prefabs/Instant Preview Touch Warning.prefab";**
     -	File where (missing) line was referenced was: **D:\Unity\onefloortestforparking\Assets\GoogleARCore\SDK\InstantPreview\Scripts\InstantPreviewManager.cs**
     -	Reference was at line  54
     -	Saw this on both Windows and Ubuntu


- Console Error:  NullReferenceException: Object reference not set to an instance of an object (Filename: Assets/GoogleARCore/SDK/Scripts/ARCoreBackgroundRenderer.cs Line: 231)
     -	Arcore threw a lot of exceptions on Ubuntu because a texture was not initialized. I fixed this with a code modification to the **ArcoreBackgroundRenderer** update event handler (commented all the code out), but it probably was due to not have git-lfs enabled which caused some texture maps to be left behind

- Console Error: "A Tree asset could not be loaded because the prefab is missing"
     -	Probably missed step 11 (i.e. the one about **TreesAndShrubs**) above. Otherwise you might need to re-import the **Treespackage** 

- Runs, but all the cars are missing
     -   This happened to me when I imported without gif-lfs enabled, it wasn't importing the car textures so you didn't see them

- Runs, but some of the cars are missing, including the Teslas
     -   This happens if you haven't copyied over the binassets 