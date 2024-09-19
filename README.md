# Unity-Project-Initializer-Package

### Overview
Custom Editor Window to quickly generate folders, batch import local and remote packages.
- Supports presets
- Creating folder structures
- Batch import local packages with unity asset store cache or custom path
- Batch import remote package through the package manager api
- Can also act as a general overview of common packages to store all paths etc.
- Will continue to import remote packages even after restart.

### How To:
To use open Window/LeonDrace/Project Initializer.

## Steps:
- Create your folder structure
- Add and enable your local packages, which have to be downloaded first and present at the defined location.
- Add and enable your remote packages. The Packagemanager will give info whether the package is valid or not. (It requires a package manifest)
- Use either the button in each section to do its import or in the header. Make sure to give it a few seconds after each action due to asset database refreshes, compiling etc.
- This might be improved in a future release to make it a one button load preset action which sequentially goes through all operations.

## Good To Know:
- There might be some packages that can cause problems when they auto import or create files on their own after importing. In most cases it should work fine though.
- In case there is a major import problem with remote packages where Unity crashes, and generally after restart it will just continue to work through the import list, you can go into the project folder and under Assets there is a TempImportData file which you can delete to stop the process.

![Project_Initializer_Window_Sample](https://github.com/user-attachments/assets/0d9e063d-aa4e-491f-b41f-7b1b0c5817b9)
