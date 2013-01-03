// 
// 
// 

#include "hpglparser.h"
#include "debugging.h"

String buffer = "";
int bufferPosition = 0;

void addToBuffer(char ch)
{
	buffer += ch;
}

void purgeBuffer()
{
	buffer = buffer.substring(bufferPosition);
	bufferPosition = 0;
}
bool isBufferEmpty()
{
	return bufferPosition<buffer.length();
}

bool isBufferFull()
{
	return (buffer.length()>40  && !buffer.indexOf("LB")>=0);
}

bool isNumber(char ch)
{
	return ch=='-' || isDigit(ch);
}

void skipWhitespaces()
{
	//preskoc non-alpha
	while(bufferPosition<buffer.length() && isWhitespace(buffer[bufferPosition]))
	{
		bufferPosition++;
	}
}

bool tryReadInt(int *i)
{
	skipWhitespaces();
	char ch = buffer[bufferPosition];
	if (isNumber(ch))
	{
		int p = bufferPosition;
		while(bufferPosition<buffer.length() && isNumber(buffer[bufferPosition]))
		{
			bufferPosition++;
		}
		String s = buffer.substring(p,bufferPosition);
		//LOG("tryReadInt "+s);
		*i = s.toInt();
		return true;
	}
	return false;
}

bool isSeparator(char ch)
{
	return isWhitespace(ch) || ch==',';
}

bool readSeparator()
{
	bool b = false;
	while(bufferPosition<buffer.length() && isSeparator(buffer[bufferPosition]))
	{
		b = true;
		bufferPosition++;
	}
	return b;
}

bool tryReadPoint(Point *pt)
{
	int i;
	if (!tryReadInt(&i))
		return false;
	(*pt).x = i;
	if (!readSeparator())
		return false;
	if (!tryReadInt(&i))
		return false;
	(*pt).y = i;
	return true;
}

String readHpglCmd()
{
	//preskoc non-alpha
	while(bufferPosition<buffer.length() && !isAlpha(buffer[bufferPosition]))
	{
		bufferPosition++;
	}
	String cmd = buffer.substring(bufferPosition, bufferPosition + 2);
	bufferPosition+=2;
#ifdef DEBUG
	LOG(">DUMP cmd=" + cmd + "|" + buffer + "|" + bufferPosition);
#endif
	return cmd;
}

String readStringUntil(char ch)
{
	int pos=bufferPosition;
	while(bufferPosition<buffer.length() && buffer[bufferPosition]!=ch)
	{
		bufferPosition++;
	}
	String result = buffer.substring(pos, bufferPosition);
	bufferPosition++;
	return result;
}