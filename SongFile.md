This file contains informations about the song file format
# Format
All song files should be encoded in utf-8 to work properly

## Header
### Must have arguments
- #TITLE: string
- #ARTIST: string
- #MP3: relativePath
- #BPM: miliseconds
- #GAP: miliseconds

### Optional arguments 
- #START: seconds -> no functionality
- #VIDEO: relativePath
- #VIDEOGAP: miliseconds
- #GENRE: string -> no functionality
- #LANGUAGE: string -> no functionality
- #YEAR: integer -> no functionality
- #EDITION: string -> no functionality
- #COVER: relativePath -> no functionality
- #BACKGROUND: relativePath -> no functionality
- #RELATIVE: [yes | no] -> no functionality
- #RESOLUTION: integer -> no functionality
- #AUTHOR: string

### Example header
#TITLE: My favorite song\
#ARTIST: DJ Example\
#MP3: myFavoriteSongMusic.mp3\
#BPM: 200\
#GAP: 1000\
#START: 100\
#VIDEO: myFavoriteSongVideo.mp4\
#VIDEOGAP: 1000\
#GENRE: pop\
#LANGUAGE: English\
#YEAR: 2010\
#EDITION: Pop\
#COVER: myFavoriteSongCover.jpg\
#BACKGROUND: myFavoriteSongBackground.jpg\
#RELATIVE: no\
#RESOLUTION: 100
#AUTHOR: Quickxeryz

### Argument definition
- #TITLE: name of the song
- #ARTIST: artist of the song
- #MP3: relative path to the mp3-file of the song or to the same mp4 as video (#VIDEOGAP also skips audio and text in this case)
- #BPM: amount of quarter notes in a minute
- #GAP: amount of milliseconds the first beat has delay 
- #START: delay of lyrics, sound and video in seconds (for song testing)
- #VIDEO: path to the background video
- #VIDEOGAP: amount of milliseconds to be skiped in video file
- #GENRE: genre of the song
- #LANGUAGE: language of the song
- #YEAR: year the song was released
- #EDITION: singstar edition of the song
- #COVER: path to the cover image of the song
- #BACKGROUND: path to the background image of the song. It is only used when no video is given or the video path isn't working
- #RELATIVE: the beat of every song line starts with 0 
- #RESOLUTION:
- #AUTHOR: name from the writer of this file

## Song Lines
Table like structure of the song all times are given in beats per minute
- First column
- - :: Regular syllable which gives normal points
- - \*: Golden syllable wich gives doublem points
- - F: Freestyle syllable which gives 0 points
- - \-: Line break which tells when the old line should disappear and the new line appear
- Second column 
- - syllable: appearing time of syllable
- - line break: beat in which the line disappears
- Third column
- - syllable: length of the syllable
- - line break: optional, the beat the next line appears, if not set the dissapear time equals the appear time
- Fourth col
- - syllable: pitch (0 = c1 (negative possible))
- - line break: nothing
- Fifth col
- - syllable: the syllable
- - line break: nothing

### Example song lines
F 13 6 0 This\
F 19 13 1  is\
F 32 8 5  Free\
F 40 26 3 style\
\- 68\
: 70 5 3 Nor\
: 77 3 8 ~\
: 82 5 12 mal\
: 90 8 3  Node\
\- 100\
\* 102 2 4 Dou\
\* 106 6 6 ble\
\* 115 7 12 Points\

# Formulas
## startTime of first beat in file
absolute: startTime = firstBeat / BPM / 4 * 60 sec + GAP\
relative: startTime = GAP