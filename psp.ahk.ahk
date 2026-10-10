SetTitleMatchMode, 2
SetTitleMatchMode, Fast

$AppsKey::

IfWinActive, osu
{
return    
}

else
{
SendInput, {Media_Play_Pause}
}