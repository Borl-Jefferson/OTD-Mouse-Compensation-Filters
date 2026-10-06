# OTD-Mouse-Compensation-Filters
A combination of filters to help make a pen have the same functionalities a mouse does for daily use. These all rely on the tablet being in relative mode and would likely break with absolute positioning.

Similar filters used as templates
[https://github.com/rinormaloku/DragThreshold](url)
[https://github.com/Kuuuube/Kuuube-s-CHATTER-EXTERMINATOR](url)
[https://github.com/X9VoiD/VoiDPlugins](url)

**Directional Binds:**
Based off of Kuube’s antichatter filters as a template. The main filter that allows the pen to have most of the same capabilities as a mouse (minus middle clicking). When used with the ScrollLockToggle ahk script, it locks the cursor in place and stops the pen from clicking normally when holding down the scrolllock button. In that mode, when you swipe down it does a left click, up does a right click, left starts scrolling up, and right scrolls down. They all hold their input until you stop dragging. If you tap and don’t drag in any direction it releases the cursor and disables the mode until you hold the key down again. I used scrolllock because I never use it for its intended purpose, but I rebinded it to left alt to make it more convenient to press. If you change it to a different key you don’t need the ahk script, all it does is change scrolllock to only be on when the key is held instead of acting as a toggle. The ahk script is disabled (disabling the whole filter) if you happen to be playing Osu so your cursor doesn't freeze when you’re near the edge when used with Gesture Swipe. AI was used to add the part that sends inputs through Windows.

**Gesture Swipe:**
This allows you to use the previous filter one handed without needing to hold down a key with the other hand. When the pen goes within a certain distance from either the top or bottom of the tablet, it activates the scrolllock mode. To release it, you can either tap the tablet without any directional input, or lift the pen past the detection range of the tablet. The distance based release doesn’t interfere when you’re holding down the key yourself. To keep the cursor where you want it, move the pen past that activation range and swipe towards the center to keep it in place. AI was used for tracking whether the pen was in range or not. 

**Velocity Controls:**
I have a button on my mouse that pauses and resumes whatever media is playing and this adds that to the pen. When you swipe up or down at a certain speed it sends a menu key input which the “psp” ahk script remaps to pausing and starting the most recently played media. It works even when the cursor is frozen with Directional Binds. Also disabled when playing Osu so media isn’t spammed on and off during jumps.

**Relative Drag Threshold:**
Identical to the normal Drag Threshold filter but works in relative mode instead. This is the first filter I modified and was used as a template for Velocity Controls and Gesture Swipe. 

**Keyboard Binded Precision Control:**
Identical to the original Precision Control but now can be activated with keyboard binds.
