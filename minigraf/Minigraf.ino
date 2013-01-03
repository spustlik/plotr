#include "debugging.h"
#include "point.h"
#include "hpglparser.h"
#include "plotter.h"
#include "font.h"

void setup()
{
	initPlotter();
	Serial.begin(57600); 
	// wait for serial port to connect. Needed for Leonardo only
	while (!Serial) {}
}

void loop()
{	
	while(Serial.available())
	{
		char ch = (char)Serial.read();
		addToBuffer(ch);
		if (ch==';')
		{
			while(processCmd()) {};
			purgeBuffer();
		}
	}
}

bool processCmd()
{
	String cmd = readHpglCmd();
	if (cmd=="DE")
	{
		DEMO();
		OK();
	} 
	else if (cmd=="BP")
	{
		BASICPOSITION();
		OK();
	}
	else if (cmd=="PU")
	{
		PU();
		Point pt;
		while(tryReadPoint(&pt))
		{
			LOG(">PU "+PT_TO_STR(pt));
			MOVETO(&pt);
			if (!readSeparator())
				break;
		}
		OK();
	}
	else if (cmd=="PD")
	{
		PD();
		Point pt;
		while(tryReadPoint(&pt))
		{
			LOG(">PD "+PT_TO_STR(pt));
			MOVETO(&pt);
			if (!readSeparator())
				break;
		}
		OK();
	}
	else if (cmd=="PA")
	{
		Point pt;
		while(tryReadPoint(&pt))
		{
			LOG(">PA "+PT_TO_STR(pt));
			MOVETO(&pt);
			if (!readSeparator())
				break;
		}
		OK();		
	}	
	else if (cmd=="PR")
	{
		Point pt;
		while(tryReadPoint(&pt))
		{
			LOG("PR "+PT_TO_STR(pt));
			Point currentPos = getCurrentPlotterPos();
			MOVETO(currentPos.x + pt.x, currentPos.y + pt.y);			
			if (!readSeparator())
				break;
		}
		OK();		
	}
	else if (cmd=="LB")
	{
		//LOG("label detected");
		String str = readStringUntil('x');
		LOG("label:"+str);
		//TODO: scaling
		drawFontString(str, 6);
		OK();
	}
	else if (cmd=="")
	{
		//skip empty command
	}
	else 
	{
		Serial.print("ERR ");
		Serial.print(cmd);
		Serial.println(";");
	}
	return isBufferEmpty();
}

