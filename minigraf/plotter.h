// plotter.h

#ifndef _PLOTTER_h
#define _PLOTTER_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include "WProgram.h"
#endif

#include "point.h"

void initPlotter();
void DEMO();
void OK();
void BASICPOSITION();
void PD();
void PU();
void MOVETO(Point *pt);
void MOVETO(int nx,int ny);

Point getCurrentPlotterPos();

#endif

