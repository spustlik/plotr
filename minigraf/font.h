// font.h

#ifndef _FONT_h
#define _FONT_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "Arduino.h"
#else
	#include "WProgram.h"
#endif

#include <WString.h>

void drawFontString(String s, int scale = 6);

#endif

