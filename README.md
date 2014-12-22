EmguWF
======

EmguWF is an image processing pipeline designer.

EmguWF uses the EMGU CV image processing library.  

EmguWF provides you with a designer surface, that allows you to, very easily, create custom image processing workflows. In EmguWF, a workflow is created, by simply dragging and dropping pipeline activitities unto the desinger surface. 
With EmguWF, you can create any sequence of activitities, out of a set of predefined elements. If the predefined activities do not fit your needs, you can create your own.

Also, EmguWF allows the user to load or save existing workflows from your local filesystem. 

After the user has setup his custom routine, he can run it against an image, by pressing the 'Run Workflow' button. 

Following screenshot, show the EmguWF designer.  

![emguwf1](https://cloud.githubusercontent.com/assets/2285199/5525326/8aadd656-89e4-11e4-9a7a-f5b65479291f.JPG)

How to install
--------------
+ Download and install a version of Emgu CV from sourceforge. I use version 2.9.0.1922-beta. [emgucv-windows-universal-cuda 2.9.0.1922](http://sourceforge.net/projects/emgucv/).
+ Go to the bin folder. In my case this is xxx/emgucv-windows-universal-cuda 2.9.0.1922/bin. The bin folder contains all EMGU.CV.xxx.dlls you will need for EmguWF to compile.     
Also inside the bin folder, you will find two subfolders: x86 or x64. Both folders, contain the opencv dlls that EMGU CV uses. If you are targetting a 64 bit environment, you must rely on the x64 folder, and forget about the x86. The inverse is true, if you are running on a 32 bit platform. 
+ Get of clone of EmguWF and open it in Visual Studio 2012.   
Add all opencv_dlls to the OpenCV folder.   
Add all EMGU.CV.xxx.dlls as references to both EmguWF.Gui & EmguWF.Actvitities projects.
+ Make sure in the project properties, under 'tab build events', that you have the xcopy statement : 'xcopy /Y "$(ProjectDir)OpenCV\*.dll" "$(TargetDir)x64\"' as post build event. this copies all the opencv dlls to the output/x64 directory. Change it to x86 if yoyu are running against an 32 bits platform.
+ If this is all setup, you should now be able to compile and run emguWF. :-)

![emguwf2](https://cloud.githubusercontent.com/assets/2285199/5526368/e0a655de-89f2-11e4-84c7-a0e46425373f.JPG) |
![emguwf3](https://cloud.githubusercontent.com/assets/2285199/5526371/e88669a6-89f2-11e4-8e1a-45d0b4e45a0f.JPG) | ![emguwf4](https://cloud.githubusercontent.com/assets/2285199/5526369/e4fc1754-89f2-11e4-8f83-bbd1ee3ccb1f.JPG)









