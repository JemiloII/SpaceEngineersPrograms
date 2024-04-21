# Small Projector Build
| Not Building | Actively Building |
|--------------|-------------------|
| <img src="https://github.com/JemiloII/SpaceEngineersPrograms/blob/main/small-projector-build/off.png?raw=true" width="512" alt="Not Building"> | <img src="https://github.com/JemiloII/SpaceEngineersPrograms/blob/main/small-projector-build/on.png?raw=true" width="512" alt="Actively Building"> |


This is designed to have multiple of these built and running.

Only variable you need to change is the prefix:

```c#
string prefix = "Build 01"
```

Everything else in the build requires blocks named the following with the prefix:

1. Build 01 Projector
2. Build 01 Welder¹
3. Build 01 Connector
4. Build 01 Button Panel
5. \*Build 01 Caution LCDs\*²

<sup>*1 Just rename any tiered tech blocks*</sup>
<br>
<sup>*2 Group of Holo LCDs*</sup>

## LCD Setup
The LCD Images were created with SEImage2LCD. I've included references to the images used. 
I used the images marked with `-r` as my holo lcd panels are on the ground. Rotating the
Images caused the back side to look upside down. So I just fliped my image file instead.
You can also choose not to use images at all and put in some default red background color.
The script simply turns the LCDs on or off.

## Setup
1. Set lcd to display text, monospace font, 0.100 text size
2. Copy the text from caution.txt or warning.txt to your LCDs text.
3. Optional: Set the text color to be red with the RGB sliders. 

### Custom LCDs
1. Download convertor http://1530.ru/software/SEImage2LCD.zip
2. Uncheck "RLE Encode"
3. Convert image
4. Copy to lcd "Public text"
5. Set lcd to display text, monospace font, 0.100 text size

Credit: https://steamcommunity.com/sharedfiles/filedetails/?id=730877708

| caution.txt | warning.txt |
|-------------|-------------|
| <img src="https://github.com/JemiloII/SpaceEngineersPrograms/blob/main/small-projector-build/caution.png?raw=true" width="256" alt="Caution Image"> | <img src="https://github.com/JemiloII/SpaceEngineersPrograms/blob/main/small-projector-build/warning.png?raw=true" width="256" alt="Warning Image"> |
