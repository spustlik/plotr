plotr
=====

this project has two parts:
- 1. c# HPGL processor for some manipulations with simple HPGL files (Hewlett-Packard graphics language) used for plotters
- 2. arduino project for receiving simple HPGL over serial port and using it to drive step-motors

Primary this was used to raise-from-groove of my old plotter MINIGRAF ARITMA A507.

Plotr-HPGL processor
--------------
simple utility to process HPGL file with 3 types of output
- image (bitmap)
  it can convert HPGL file into image (png by default)
- file 
  just for do some operations with your HPGL file, see more)
- serial port
  for sending your file to your home-made potter. It can be paused for changing your pen etc.

Utility supports only some commands of HPGL language:
PU,PD (Pen Up/Pen Down)
PA,PR (Pen Absolute, Pen Relative)
LB (Label)

It can do some operations for you 
- converting text to vectors (using embedded font)
- transform drawing (autoscale, rotate, move, zoom)
- optimize pen-up moves
- absolutize (remove PR's)

see commandline help for more


minigraf-Arduino sources
---------------
receives HPGL over serial port (at 57600 bauds) and drives step motors.
It is assumed, that step-mottors already has TTL counter driver, so there are 3 X-pins (D7-D5), 3 Y-pins (D4-D2) and PEN pin (D12).
There is also hard-coded timing for my plotter, so you can change it.

implemented HGPL commands:
- PU[x,y];
- PD[x,y]*;
- PAx,y[,x,y]*;
- PRx,y[,x,y]*;
- LBtext<0xD>

Enjoy

Installation, compilation
-------------------------
plotr - simple c# .NET 4.5 application. Open & compile with Visual Studio 2012. You can use SDK or express.
minigraf - use arduino addin to Visual studio 2012

