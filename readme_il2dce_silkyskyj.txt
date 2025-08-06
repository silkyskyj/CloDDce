=============================================================================================================================
A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Release (Version 0.7.5)
=============================================================================================================================
                                                                                                                   08/13/2025
This is the silkysky version of IL2DCE.

IL2DCE was created and released by the original author, Stefan Rothdach, in 2011 and has been customized since.
I have forked the source code and customized it under the AGPL3.0 license terms, and am releasing it this time.

It is important to note that this add-in does not replace or attempt to compete with existing apps and add-ins such as Quick Mission, Single Mission, Campaign, and other apps and add-ins created by various developers, official development teams, and groups.
I hope you will choose it as one of the several ways to play the game.

In this release, I have focused on testing whether the program works properly, and have done little testing in abnormal cases.
(For example, most of the abnormality tests are for things like what happens if there is an unexpected format in the configuration file, what happens if the pilot name is 100 characters, etc.)
Please be aware of this.

This is a program that is available for free under the terms of AGPL3.0.
I do not guarantee operation, damages caused by use, the presence or absence of defects, bug fixes, feature additions, replies to inquiries, or explanations of features.
I'm very sorry, but please use this product only if you understand these points.

I can't speak English, so I translate or paraphrase everything I read and write. Please understand that there may be some minor mistakes in the expressions.

Main functions
1. Dynamic Campaign
2. Quick Misson

1. In Dynamic Campaign, you can select and set the following items.
 Career
 - Army (Red/Blue)
 - Air Force(*)
 - Rank
 - PilotName
 - Campaign (mission file for IL2DCE)
 - Campaign Mode (Repeat/Progress/Random/...)
 - Air Group
 - Campaign Period
 - Filter display function for existing career list (Filter Army/Air Force/Campaign/StrictMode/Playabe/Aircraft & Sort Logic) 
 - Strict Mode & Dynamic Frontmarker
 - Spawn Parked
 - Initial AirGroup Skill (system default or custom skills) 19 in total + user-defined (can be set in il2dce's conf.ini file)
 - Progress (AnyTime/Daily/AnyDay/AnyDayAnyTime)
 - Battle start time range

2. The following items can be selected and set in Quick Mission.
 - Campaign (mission file for IL2DCE)
 - Army (Red/Blue)
 - Air Force(*)
 - Rank
 - Air Group
 - Skill (system default or custom skills) 19 in total + user-defined (can be set in il2dce's conf.ini file)
 - Mission Type
 - Flight
 - Formation
 - Spawn type [idle/Parked/AirStart(Altitude)]
 - Spawn Speed
 - Fuel % value (5% increments)
 - Date
 - Time
 - Weather
 - Cloud Altitude

 (*)Air Force
     Red: Royal Air Force/Armee de l'air/United States Army Air Forces/Soviet Air Force/Air Force of Polish government-in-exile/Air Force of Czechoslovak government-in-exile/Royal Netherlands Air Force/Belgian Air Component
     Blue: Lufutwaffe/Regia Aeronautica/Royal Hungarian Air Force/Royal Romanian Air Force/Finissh Air Force/Slovak Air Force/Bulgarian Air Force/Air Force of the Independent State of Croatia
     *Mission files and configuration files must be compatible with your own extensions. Please refer to the included sample missions.

Items that can be selected and set commonly in 1. 2.
 - Ability to convert and import existing mission files & campaign
 - Max Additional Air Operations [1-12]
 - Max Additional Ground Operations [10-300]
 - Automatically add Air Groups (this adds to Random if there are few AirGroups)
 - Automatically Ground Groups / Stationary Units
 - Keep nums of Groups/Units (Dynamic Spawn)
 - Random Air Group Spawn Location [Player/Friendly/Enemy]
 - Random Air Group Spawn Altitude [Friendly/Enemy]
 - Time(Spawn delay) 15-1800[sec] [Friendly/Enemy]
 - Track Recording
 - AI Air Group Skill
 - AI Ship Skill
-  Add Random Ground units
 - Ground Unit Setting
 - Unit Nums
 - Convert Generic type
 - Save/Load general settings

Sample mission (5 mission files)
 - Adlerangriff - Royal Air Force/Lufutwaffe
 - Cross v Round - All Air Forces
 - Kanalkampf - Royal Air Force/Lufutwaffe
 - Supply Wars - Royal Air Force/Lufutwaffe/Regia Aeronautica
 - Transfer - Royal Air Force/Lufutwaffe (Sample of Multiple mission file & Map Change)

I have not been able to confirm whether this will work in any environment, but I will describe the environment in which it actually worked.
OS: Windows 10 Home x64 (Development) latest official MS patch / Core i7-3770 & GTX 1050 Ti
    Windows 11 Home (Test & Play) latest official MS patch
    Windows 7 Home Premium x64 latest official MS patch
    Windows 7 Home Premium x86 latest official MS patch
Software: IL-2 Sturmovik: Cliffs of Dover + TFS latest patch - English Langauage 
Display Resolution: 1920x1080

A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover

Official website: https://github.com/silkyskyj/il2dce
*The latest source code and change history are all publicly available.

Original: https://github.com/il2dce/il2dce ( https://code.google.com/archive/p/il2dce/ )
Install: https://code.google.com/archive/p/il2dce/wikis/Install_Instructions.wiki (+ Blitz edition: https://forum.il2sturmovik.com/topic/90336-as-a-qmb-is-currently-wip-lets-discuss-what-it-needs-to-be-successful/ )

Campaign Creation Instructions: https://code.google.com/archive/p/il2dce/wikis/Campaign_Creation_Instructions.wiki

===========================================================================================================================
History 
===========================================================================================================================

  Ver      Date       Comment

  0.7.5    08/13/2025 Fixed: Some minor issues. 
                      Added: Original Cliffs of Dover x86 Version (require TFS latest patch)
  0.7.4    07/14/2025 Cumulative Update(CU202507)
                      Fixed: The score calculation is wrong when changing sides. A friendly kill may be counted if the player crashes into the ground or dies on their own. HUD messages related to automatic resupply and ammo refilling are displayed on units that include AI other than the player. etc
  0.7.3    05/24/2025 Added: Ability to continue campaigns even if the player's AirGroup is not present in the ongoing mission file of the campaign.
                      Changed: Save and load the period set when creating a new campaign to a carrier file.
                      Fixed: "Collection has been modified; enumeration operation may not execute" error when generating a mission. Other minor bug fixes.
  0.7.2    05/18/2025 Added: Campaign Progress Mode(and Random/Repeat). Convert & Improt CloD default type Campaign. Maps change depending on the battle date (the English_Channel_1940/Autumn/Winter map sean changes in the attached sample and convert/import mission). select battle start time range in Dynamic campaign page. select date in Quick Mission page. 
                      Changed: Mission Import & Conversion Logic(Weapons info is null case/Duplicate Squadron)
                      Fixed: Some minor issues.
  0.7.1    05/10/2025 Added: Bulgarian Air Force and Air Force of the Independent State of Croatia in Select Air Force. User interface (UI) for specifying folders and files for the mission conversion and import function.
                      Changed: Some of the names of Air Force and Air Group that have been expanded and added independently.
  0.7.0    05/04/2025 Added: Dynamic generation of operations during mission battles, and the option to spawn groups and stationary units.
                      Fixed: AirGroup group name generation and other minor fixes.
  0.6.4    04/29/2025 Fixed: Strict and other modes are not properly separated(Campaign). When the number of flights in a squadron changes, it is treated as a separate unit, and continuation of the state, reinfoce, etc. are not performed properly (Campaign StrictMode). An error occurs if you proceed to the next step without fighting, and in StrictMode the campaign ends. 
                      Added: some sort and filter functions to the career selection in the campaign.
  0.6.3    04/27/2025 Fixed: Battle results statistics show nothing. In Campaign Strict Mode, some groups and units do not inherit their status or reinforce properly.
  0.6.2    04/25/2025 Fixed: not work AdditionalStationaries in Campaign, not carried over GroundGroup disable & position[Campaign StrictMode], if a group's units are reduced to 0, they will disappear forever[Campaign StrictMode]
  0.6.1    04/24/2025 Fixed: Temporary solution to CloD specification where AI aircraft will not take off if AirGroup is set idle.
  0.6.0    04/20/2025 Added: Strict mode/AI AirGroup Skill/SpawnParked/Progress type selection in Dynamic Campaign new Career page. Add Random Ground units/AI Ship Skill/Ground Unit Setting/Unit Nums/Convert Generic type option in each page. Save/Load general settings in each pages. Ability to increase or decrease skill values ​​based on Campaign mission results/Display current skill values.
                      Fixed: Improved mission loading speed. Other minor bug fixes.
                      Changed: Changes to mission import logic and mission file structure. Fixes to attached mission files.
  0.5.10   04/05/2025 Added: AI Air Group Skill selection. Stats view in Quick Mission Page. Royal Netherlands Air Force and Belgian Air Component in Select AirForce. 
                      Fixed: if ther aircraft moves while stopped at the airport, the time until re-fuel(re-arm) is shorter. If an exception occurs while displaying Result page, game cannot be continued. Changed: Stats display Format & statType Option(conf.ini).
  0.5.9    03/30/2025 Fixed: After the change in v0.5.6, the speed becomes 0 when starting with AirStart. Reviewed the logic of Ground Operation and fixed related items in mission import.
  0.5.8    03/30/2025 Fixed: The name of Blue amry Armor replaced by default was incorrect. when use PreLoad button, the page to become unstable. Stats are not displayed with statType option 1.
  0.5.7    03/29/2025 Fixed: Fixed: Depending on the position of the FrontMarker, the random spawn localtion may continue to process infinitely.
  0.5.6    03/29/2025 Added: Select Auto re-Arm(bullets only), Auto re-Fuel, Track Recording option in Quick Mission page, Career & Campaign Intro page. display the spawn airport and the distance [km] from the front line option in Select Air Group list.
                      Fixed: FrontMarker is not displayed. Armor/Vehicle are not placed.
  0.5.5    03/24/2025 Added: Select Formation in Quick Mission Page. Fixed: abend error after import mission. where some of Air Group ways were overwritten and not saved property(including a bug where the speed was not set.)
  0.5.4    03/23/2025 Added: Select flight count & size. Fixed: Bug Where the fuel and speed values set in Quick mission were not reflected.
  0.5.3    03/23/2025 Changed: Changed the logic for generating the mission types Hunting and Follow. Changed some misspellings in the key names in the Config.ini file. Sorce* -> Source*
  0.5.2    03/23/2025 Fixed a bug when the EnableMissionMultiAssign option introduced in v0.5.1 was enabled (Air Groups could be duplicated in missions such as Hunting).
  0.5.1    03/22/2025 Fixed: mission type Cover was not assaigned 
  0.5.0    03/22/2025 New release 

---
IL2DCE [Forked by silkysky]
Copyright © Stefan Rothdach 2011- & silkysky 2025-
silkysky https://github.com/silkyskyj/il2dce
Stefan Rothdach https://github.com/il2dce/il2dce (https://code.google.com/archive/p/il2dce/)