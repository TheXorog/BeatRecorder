# OBSControl

This application is for people who record their BeatSaber gameplay using OBS. It connects to [HttpStatus](https://github.com/opl-/beatsaber-http-status/) to detect the current game-state and [obs-websocket](https://obsproject.com/forum/resources/obs-websocket-remote-control-obs-studio-from-websockets.466/) to automatically start and stop the recording.

Files are saved where-ever you set your output folder to in OBS.

**If you have encountered any bugs or issues with this application, feel free to create an issue with your problem.**

**If you want to get it fixed faster or it's something urgent, you can join [my Discord](https://discord.gg/gzEjZE9Mre) and hit me (`Fab#5555`) with a ping.**

## Requirements

* [beatsaber-http-status](https://github.com/opl-/beatsaber-http-status/) (And any other dependencies that HttpStatus has.)
* [obs-websocket](https://obsproject.com/forum/resources/obs-websocket-remote-control-obs-studio-from-websockets.466/)

## How to set up

1. Install all the previously mentioned dependencies.
2. Download and unzip this application.
3. Run it once, it'll ask you to input your Settings. Do so.
4. And after configuring what you want, save the file and re-open the application.
5. Profit

## OBSControl not connecting?

**AVG Antivirus is known to cause all kinds of issues** with connection-related things. (Credit to `Arro#6969` to help me discover this issue! <3)
If you have AVG Antivirus installed, please uninstall it and find a better antivirus solution.

If you ruled out your antivirus (through uninstalling it or deactivating it's protection), just create an issue or join my Discord like mentioned before.

## Filename Placeholders

* `<song-name>` - The song name
* `<song-author>` - The song author
* `<song-sub-name>` - The subtitle of the song (e.g. what kind of remix the song is)
* `<mapper>` - The person who made the map
* `<levelid>` - The LevelID of the song (not the beatsaver id)
* `<bpm>` - The beats per minute the song uses

* `<rank>` - The Rank you got (B, A, S, SS, etc.)
* `<accuracy>` - The accuracy you achieved (e.g. `91.31`, `FAILED`, `QUIT`)
* `<max-combo>` - The best combo you achieved in that play
* `<score>` - Your Score, with mod multipliers enabled
* `<raw-score>` - Your Score, without mod multipliers enabled

(If you potentially need more placeholders, create an issue and i'll see what i can do.)

## Contributing or modifying the code

It should be as easy as cloning the repository and running `dotnet restore`.
