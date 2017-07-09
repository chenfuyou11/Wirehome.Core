#include "SerialEx.h"

void SerialEx::Init()
{
	Serial.begin(9600);
}

void SerialEx::WriteText(const String &message)
{
	#if DEBUG
 	 	Serial.print(message);
		Serial.flush();
	#endif
}

void SerialEx::WriteError(const String &message)
{
	#if DEBUG
 	 	Serial.print(message);
		Serial.flush();
	#endif
}




