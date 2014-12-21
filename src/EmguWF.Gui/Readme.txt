add as post build event. this makes sure the opencv_xxx.dll are copied to the output directory.
Not having those dlls in place cause the cvInvoke.invoke to throw a type initializerr exception to be thrown. 

xcopy /Y "$(ProjectDir)OpenCV\*.dll" "$(TargetDir)x64\"

vs2012-xdesproc-hangs-when-xaml-file-is-opened
http://weblogs.asp.net/fmarguerie/life-changer-xaml-tip-for-visual-studio