#include "Infrared.h"

void Infrared::Init()
{
	Infrared::myReceiver.enableIRIn();
}

void Infrared::ProcessLoop()
{
	if (Infrared::myReceiver.getResults()) 
	{
    	myDecoder.decode();
    	myReceiver.enableIRIn();
  	}
}

void Infrared::Send(uint8_t package[], uint8_t packageLength)
{
	
}



