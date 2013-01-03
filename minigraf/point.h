// point.h

#ifndef _POINT_h
#define _POINT_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include "WProgram.h"
#endif

typedef struct _Point {
	int x;
	int y;
} Point;

#endif
