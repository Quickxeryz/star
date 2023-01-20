# Format
## Header
### Must have arguments
- #TITLE: string
- #ARTIST: string
- #MP3: relativePath -> no functionality
- #BPM: miliseconds
- #GAP: miliseconds

### Optional arguments 
- #START: seconds -> no functionality
- #VIDEO: relativePath
- #VIDEOGAP: miliseconds -> no functionality
- #GENRE: string -> no functionality
- #LANGUAGE: string -> no functionality
- #YEAR: integer -> no functionality
- #EDITION: string -> no functionality
- #COVER: relativePath -> no functionality
- #BACKGROUND: relativePath -> no functionality
- #RELATIVE: [yes | no] -> no functionality
- #RESOLUTION: integer -> no functionality
- #AUTHOR: string -> no functionality

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
- #MP3: relative path to the audio file of the song or to the same mp4 as video (#VIDEOGAP also skips audio in this case)
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

# Formulas
## startTime of first beat in file
absolute: startTime = firstBeat / BPM / 4 * 60 sec + GAP\
relative: startTime = GAP

-----------------------------------------------------------
First col
: Regular note
* Golden note
F Freestyle syllable
– Line break (separates lyrics into suitable lines).
Line breaks are different to other types of row, in that they consist of a hyphen ( – ) and either one or two numbers. If it contains one number, it determines the beat at which the previous line will disappear. For example, in the first line of the song above, the ‘Teenage dreams’ line disappears as soon as it’s been sung, on beat 12. If the line break contains 2 numbers, the first number determines when the first line disappears, and the second determines when the next line will appear. There is no example of this type of line above, as it’s a fast moving song with no proper breaks from singing – line breaks containing two numbers are generally for songs with a large instrumental break in them. Two numbers aren’t at all necessary, however, as the game automatically puts the next line up when it is approaching – it’s only if you want to control when it happens that you need to worry about the ‘second’ number.
Second col
appearing of syllable
Third col
length of syllable
Fourth col
pitch (0 = c1 (negative possible))
Fifth col
text 
Calculatiuon of first beat: starttime = first col / BPM / 4 * 60 Sekunden + GAP.