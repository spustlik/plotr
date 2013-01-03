// debugging.h

#ifndef _DEBUGGING_h
#define _DEBUGGING_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include "WProgram.h"
#endif

//#define DEBUG
#ifdef DEBUG
#include "point.h"
#define LOG(x) _log(x);
void _log(String s);

#define PT_TO_STR(pt) (String("[") + (pt).x + "," + (pt).y + "]")

#else
#define LOG(x) ;

#endif


#endif

