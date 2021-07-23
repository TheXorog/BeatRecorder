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

## Config Help

* `README` - This really just redirects you here for help.
<br></br>
* `ConfigVersion` - Please don't touch <3
* `ConsoleLogLevel` - The log level displayed in the console. I recommend leaving that alone.
<br></br>
* `BeatSaberUrl` - The IP/URL you use to connect to the HTTPStatus-WebSocket. Leave at default if you don't know any better.
* `BeatSaberPort` - The Port you use to connect to the HTTPStatus-WebSocket. Leave at default if you don't know any better.
<br></br>
* `OBSUrl` - The IP/URL you use to connect to the OBS-WebSocket. Leave at default if you don't know any better.
* `OBSPort` - The IP/URL you use to connect to the OBS-WebSocket. Leave at default if you don't know any better.
* `OBSPassword` - The password you use to connect to the OBS-Websocket. OBSControl will ask for the password if you haven't put one in.
* `AskToSaveOBSPassword` - This specifies if you want OBSControl to ask for a password. 
<br></br>
* `MininumWaitUntilRecordingCanStart` - The amount of milliseconds a new recording is gonna be delayed when forcing to restart a recording. Increase this if you find that your recordings don't restart after restarting a song.
<br></br>
* `PauseRecordingOnIngamePause` - Should your recording be paused when you pause your current song?
* `FileFormat` - The format your files will be renamed to. Check Filename Placeholders below for more info.
* `StopRecordingDelay` - How many seconds OBSControl should wait after you finish a song/leave the song to end the recording.
<br></br>
* `DeleteIfShorterThan` - Delete the file if the recording is shorter than the specified amount of seconds.
* `DeleteQuit` - Delete the file if you quit the song.
* `DeleteIfQuitAfterSoftFailed` - Delete the file if you quit the song after losing all your health with no fail.
* `DeleteFailed` - Delete the file if you fail the song.
* `DeleteSoftFailed` - Delete the file if you soft-fail¹ the song.
<br></br>
1: _Soft-Failing means losing all your health with no fail on._


## Filename Placeholders

The current default is: `[<rank>][<accuracy>][<max-combo>x] <song-name> - <song-author> [<mapper>]`
<!-- * `<difficulty>` - The difficulty (e.g. `Expert`, `Expert+`) (`1.1.1+`) -->
<!-- * `<short-difficulty>` - The difficulty but shorter (e.g. `EX`, `EX+`) (`1.1.1+`) -->
* `<song-name>` - The song name
* `<song-author>` - The song author
* `<song-sub-name>` - The subtitle of the song (e.g. what kind of remix the song is)
* `<mapper>` - The person who made the map
* `<levelid>` - The LevelID of the song (not the beatsaver id)
* `<bpm>` - The beats per minute the song uses
<br></br>
* `<rank>` - The Rank you got (`B`, `A`, `S`, `SS`, etc.)
* `<accuracy>` - The accuracy you achieved (e.g. `91.31`, `FAILED`, `QUIT`)
* `<max-combo>` - The best combo you achieved in that play
* `<score>` - Your Score, with mod multipliers enabled
* `<raw-score>` - Your Score, without mod multipliers enabled

(If you potentially need more placeholders, create an issue and i'll see what i can do.)

## Contributing or modifying the code

It should be as easy as cloning the repository and running `dotnet restore`.
