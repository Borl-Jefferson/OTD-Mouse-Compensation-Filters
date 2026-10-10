#SingleInstance Force

*ScrollLock::
IfWinActive, osu
{
return    
 }
else{
    SetScrollLockState, On
    KeyWait, ScrollLock 
return
}

*ScrollLock up::
    SetScrollLockState, Off
return