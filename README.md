# S&Box - Destiny 2 Map Importer
- Imports ripped Destiny 2 maps from **[CharmBox](https://github.com/DeltaDesigns/CharmBox)** (My fork of Charm) into Hammer
 
## Requirements
1. Have a S&Box Project setup, if you don't have one, the Minimal template will suffice.
2. Have a map ripped using CharmBox (Hopefully you know how to do this if you're using this tool)
3. Have all your materials, shaders and models in the maps addon folder ([Guide](https://github.com/DeltaDesigns/Charm/wiki/Source-2-Importing))
 
## How to install

1. In your Project, put the "D2MapImporter.cs" into a folder called "Editor", S&Box should compile and succeed.
2. In Hammer, make a new map
3. Click "D2 Map Importer" in the top tool bar, adjust settings if needed, then "Select Files"
4. Navigate to where the maps "info.cfg" files are located and select everything you want to import
5. Wait, this may take some time!
 
![image](https://github.com/DeltaDesigns/SBox-Destiny-2-Map-Importer/assets/50308149/e155850d-03bd-4f78-b5b3-1edad04ec728)
![image](https://github.com/DeltaDesigns/SBox-Destiny-2-Map-Importer/assets/50308149/65add1d4-4466-4c86-a6ea-ded1c9b0c6a6)
![image](https://github.com/DeltaDesigns/SBox-Destiny-2-Map-Importer/assets/50308149/4111951d-ae27-4378-aeaa-9c24955d1080)

**This is a WIP**
Things will not be perfect, you might/will have to adjust things to your liking such as lights

## To Do:
- ~~Add importing progress using NoticeWidgets.~~
- ~~Add a basic menu for importer settings.~~
- ~~Add model instancing, currently not very do-able due to instances not being able to be individually scaled~~
- Decals and decoration
- Optimizations where possible

