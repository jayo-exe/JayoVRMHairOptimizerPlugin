# Jayo's VRM Hair Optimizer Plugin for VNyan

A VNyan Plugin that automatically combines hair meshes on standard VRM-structured models to greatly improve performance

# Table of contents
1. [Compatability Notice](#compatability-notice)
2. [Installation](#installation)
3. [Merging Hair Meshes](#controlling-liltoon-properties)
4. [Usage](#usage)
    1. [Inbound Triggers](#inbound-triggers)
        1. [Activate Plugin](#set-float-property)
        2. [Deactivate Plugin](#set-int-property)
5. [Development](#development)

## Compatability Notice
In order for this plugin to function, it needs to be used on a model with the **standard VRM object structure** and hair meshes that were **not already merged when the moel was exported**.
It won't be able to work on models where the hair objects are located in a differently-named part of the model hierarchy, and (obviously) can't merge hair meshes that are already merged.

## Installation
I've made the source of this plugin available here on Github for anyone to build, run, or modify for thier own purposes! 

Built and ready-to-use copies are available for purched on my itch.io and Ko-Fi stores! You'll get ready-to-use plugin files, access to plugin updates forever, and my gratutude for supporting my continued work!

Once you've got your plugin files (either from a ZIP fiule from the store, or from your builds from source), installation is simple:

1. In VNyan, make sure you've enabled "Allow 3rd party plugins" from the Settings menu.
2. Extract the contents of the ZIP file _directly_ into your VNyan installation folder_.  This will add the plugin files to yor VNyan `Items\Assemblies` folders. If you built your own files you'll need to put them into the Assemblies folder directly.
3. Launch VNyan, confirm that a button for the plugin now exists in your Plugins window!

## Merging Hair Meshes
Many Vroid-provided haitstyles have **more than one hundred** individual hair sections! If a VRM model is exported without merging the hair into one mesh, this could **triple** the number of draw calls required to render your model every frame.  
This can greatly reduce your framerate, especially if you're also streaming a CPU-intensive game.

When the plugin is active and a model is loaded in VNyan, it will look inside the model's strucutre for a GameObject called `Hairs` that contains a series of other GameObjects.  
If it finds this, the individual meshes will be merged together into a sigle mesh, with one submesh for each different material found.
The result is that the model's hair will look and function exactly the same, but with a greatly reduced number of draw calls and therefore improved performance.

This can be useful if you'd like to keep your model's hair separated so that you can customize different sections of it with different materials in Unity, while still minimizg the performance impact at show time!

## Usage

When the plugin is activated, it works automatically on the currently-loaded model, and will also automatically apply to any other model that is loaded!
THere's also an option to automatically actvate the plugin when VNyan starts

### Inbound Triggers

While entirely optional, this plugin listens to a couple triggers in case you need to activate or deativate in from the Node Graph for some reason. 
These simple triggers don't use any value sockets and are simply called by name.

#### Activate Plugin
Trigger Name: `_xjho_enable`

Activates the Hair Optimizer plugin if it isn't already activated.  Functionally the same as clicking the "Activate" button in the plugin window.

#### Deactivate Plugin
Trigger Name: `_xjho_disable`

Deactivates the Hair Optimizer plugin if it is currently active.  Functionally the same as clicking the "Deactivate" button in the plugin window.

## Development
(Almost) Everything you'll need to develop a fork of this plugin (or some other plugin based on this one)!  The main VS project contains all of the code for the plugin DLL, and the `dist` folder contains a `unitypackage` that can be dragged into a project to build and modify the UI and export the modified Custom Object.

Per VNyan's requirements, this plugin is built under **Unity 2020.3.40f1** , so you'll need to develop on this version to maintain compatability with VNyan.
You'll also need the [VNyan SDK](https://suvidriel.itch.io/vnyan) imported into your project for it to function properly.
Your Visual C# project will need to mave the paths to all dependencies updated to match their locations on your machine (i.e. you VNyan installation directory under VNyan_Data/Managed).  Most should point to Unity Engine libraries for the correct Engine version **2020.3.40f1**.