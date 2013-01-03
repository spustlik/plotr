// hpglparser.h

#ifndef _HPGLPARSER_h
#define _HPGLPARSER_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include "WProgram.h"
#endif

#include "point.h"

String readHpglCmd();
bool tryReadPoint(Point *pt);
bool readSeparator();
String readStringUntil(char ch);
void addToBuffer(char ch);
void purgeBuffer();
bool isBufferEmpty();
bool isBufferFull();
#endif

