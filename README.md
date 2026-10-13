# OTD-Mouse-Compensation-Filters
A combination of filters to help make a pen have the same functionalities a mouse does for daily use. These all rely on the tablet being in relative mode and is not compatable with absolute positioning.

Similar filters used as templates  
[https://github.com/rinormaloku/DragThreshold](url)  
[https://github.com/Kuuuube/Kuuube-s-CHATTER-EXTERMINATOR](url)  
[https://github.com/X9VoiD/VoiDPlugins](url)

**Directional Binds:**  
The main filter that allows the pen to have the same capabilities as a mouse (with the exception of middle clicking). When scrolllock is held down, it locks the cursor in place and ignores the normal pen clicking inputs. Instead, it sends inputs depending on which direction you swipe on on the tablet with down sending a left click, up sending right click, left scrolling up, and right scrolling down. If you tap and don’t drag in any direction it releases the cursor and disables the mode until you hold the key down again. I used scrolllock because I don't use it for anything and rebinded it to left alt to make it more convenient to press, and use the ScrollLockToggle.ahk script to make the mode only active when holding the key instead of acting as a toggle. The script is disabled (disabling the whole plugin by blocking scrolllock mode) if you're playing Osu so your cursor doesn't freeze when you’re near the edge when used with the Gesture Swipe plugin. AI was used to send Windows inputs.

**Gesture Swipe:**  
This allows you to use the previous filter one handed using just the pen, without needing to hold down a key with the other hand. When the pen goes within an adjustable distance from either the top or bottom of the tablet, it activates scrolllock mode. To release it, you can either tap the pen without any directional input, or lift the pen past the detection range of the tablet. The detection based release doesn’t interfere when you’re holding down the key yourself. AI was used for tracking whether the pen was in range or not. 

**Velocity Controls:**  
I use a button on my mouse to pause and resume whatever media is playing and this adds the same feature to the pen. When you swipe up or down (while hovering or dragging) at an adjustable speed it sends a menu key input which the psp.ahk script remaps to pausing and starting the most recently played media. It works even when the cursor is frozen with Directional Binds. Also disabled through the ahk script when playing Osu so media isn’t spammed on and off during jumps.

**Relative Drag Threshold:**  
Identical to the normal Drag Threshold filter but works in relative mode instead. This is the first filter I modified and was used as a template for Velocity Controls and Gesture Swipe. Lost the project folder though

**Keyboard Binded Precision Control:**  
Identical to the original Precision Control but now can be activated with keyboard binds. Also lost the project folder
