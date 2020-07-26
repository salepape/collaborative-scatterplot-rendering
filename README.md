# Collaborative Scatterplot Rendering

## Introduction

This independant project has been conducted during my end-of-studies internship, and was the first step of a process to improve a bigger Immersive Analytics project. Some comments within the code describe more precisely how it works (algorithmic, decisions taken and so on).

## Setup

Just download the .zip project folder or clone the repo to your computer. Then open this folder under Unity. Be sure :
* you have a HTC Vive headset (classic, PRO or PRO EYE) linked to the computer on which you run Unity, according to Vive cables and linking box. 
* SteamVR application recognizes headset, its corresponding motion controllers as well as base stations / lighthouses (appears green when it's the case). 
Before clicking Play button, you need to :
* fill in appId required, provided when you create a Photon account (PUN Setup window pops up when project is opening, or go to Window > Photon Unity Networking > PUN Wizard > Setup Project);
* provide your dataset through a .csv file, with its 3 first columns indicating its coordinates along x, y and z axes. Click on "Scatterplot" GameObject in the Hierarchical window, and provide your .csv file name in "inputfile" field;
* choose the features you want to test in Unity right panel (cf Features section). Click on "Scatterplot" GameObject in the Hierarchical window, then double click on "Point Cloud Material" field, then check the boxes you want. You can also modify point representation characteristics.
To finish, click Play button to launch the application. Then you enter the Networking Scene :
* Some panels will successively be displayed to the screen ("Connect to Photon...loading...", "Connected", or "Lost connection to Photon services..." if failure). If it succeed, a connecting window will appear where you need to indicate your player name and the name of the server you want to create or join. 
* Then, by clicking Create or Join button, you enter the Game Scene.

## Features

When a player enters the Game scene, he / she can :
* see what other players do in real time if several HTC Vive toolkits have been initialized and players are in the same server (some latency is to be expected according to the computer used, the netork connection quality...). Some rudimentary player modeling will be seen for each player (colored cube for headset, red hands for motion controllers).
* grab Game0bjects which are entertainment objects (cubes) or geometric analysis tools (planes or boxes) and move them to dataset points in order to visualize the modification desired (cutting or highlighting feature, choose the light effect applied and so on);
* display text at top of each data point intersecting the analysis tool currently grabbed;

## Controls

Simply pull the trigger when motion controllers intersects a clipping plane / box (the clipping object will turn yellow to blue) to grab it.

## Technologies

The project is developed under Unity 2019 within Windows 10 environment. Unity Hub has been used to push modifications. Technologies used are :
* Unity 2019 1.11f1 game engine : C# code + HLSL shaders;
* SteamVR application;
* OpenVR API;
* Photon PUN 1 API;
* TextMesh Pro package.

## Resources

* Scientific data has been retrieved from AREMI (https://nationalmap.gov.au/) and NEAR project (https://near.csiro.au/);
* Initial bigger project to improve called ImAxes made by Maxime Corbeil : https://github.com/MaximeCordeil/ImAxes, and Mircrosoft project used for improvements : https://github.com/microsoft/MapsSDK-Unity;
* Some ideas and parts of code project have been inspired from VRTK, Youtube, website tutorials and research papers (see Bibliography section of the report for more information).
