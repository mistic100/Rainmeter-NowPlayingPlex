NowPlayingPlex.dll
================

A Rainmeter plugin to read the currently playing track on Plex.


# Usage

The plugin works similarly to the [NowPlaying measure](https://docs.rainmeter.net/manual/measures/nowplaying) : there is a main measure doing the query and child measures connected to the main.


# Options

### `PlexToken` (required on main measure)

_Main measure only._ Your personnal access token ([how to get it](https://www.plexopedia.com/plex-media-server/general/plex-token/).

### `PlexServer` (default: `http://localhost:32400`)

_Main measure only._ The url of your Plex server.

### `PlexUsername`

_Main measure only._ Your Plex username, this is used to filter the sessions if other users are using your server. If not defined, the first session will be used.

### `PlayerName` (required on child measures)

_Child measures only._ Name of the main measure.

### `PlayerType` (required)

Type of the measure value. Valid values are: 

- `Artist`
- `Album`
- `Title`
- `Number`
- `Year`
- `Cover`
- `File`
- `Duration`
- `State`

**Notes:** With measures of type `Duration`, the string value is in the form `MM:SS` and the number value is the actual number of seconds.

### `DisableLeadingZero` (default: `0`)

_Main measure only._  If set to `1`, the format of `Duration` is `M:SS` instead of `MM:SS`.


# Example

```ini
[Rainmeter]
Update=1000

[Variables]
PlexToken=XXXXXX
PlexUsername=JohnDoe

[MeasureTitlePlex]
Measure=Plugin
Plugin=NowPlayingPlex
PlexToken=#PlexToken#
PlexUsername=#PlexUsername#
PlayerType=Title

[MeasureArtistPlex]
Measure=Plugin
Plugin=NowPlayingPlex
PlayerName=MeasureTitlePlex
PlayerType=Artist

[MeasureAlbumPlex]
Measure=Plugin
Plugin=NowPlayingPlex
PlayerName=MeasureTitlePlex
PlayerType=Album

[MeasureDurationPlex]
Measure=Plugin
Plugin=NowPlayingPlex
PlayerName=MeasureTitlePlex
PlayerType=Duration
Substitute="00:00":""
```


# License

MIT
