#include <Wire.h>
#include "SerialEx.h"
#include "Infrared.h"
#include "LPD433.h"
#include "Dht22Controller.h"
#include "CurrentController.h"
#include "common.h"

#define IS_HIGH(pin) (PIND & (1<<pin))
#define IS_LOW(pin) ((PIND & (1<<pin))==0)
#define SET_HIGH(pin) (PORTD) |= (1<<(pin))
#define SET_LOW(pin) (PORTD) &= (~(1<<(pin)))

uint8_t _lastAction = 0;

void handleI2CRead()
{
	SerialEx::WriteLine("I2C READ for action" + String(_lastAction));
	digitalWrite(PIN_LED, HIGH);

	uint8_t response[32];
	size_t responseLength = 0;

	switch (_lastAction)
	{
		case I2C_ACTION_DHT22:
		{
			responseLength = DHT22Controller_handleI2CRead(response);
			break;
		}
		case I2C_ACTION_Current:
		{
			responseLength = CurrentController_handleI2CRead(response);
			break;
		}
	}

	Wire.write(response, responseLength);
	Wire.flush();

	digitalWrite(PIN_LED, LOW);
}

void handleI2CWrite(int dataLength)
{
	if (dataLength == 0) return;

	if (dataLength > 32) 
	{ 
		SerialEx::WriteError(F("Received too large package"));
		return;
	}

	digitalWrite(PIN_LED, HIGH);

	_lastAction = Wire.read();

	uint8_t package[32];
	size_t packageLength = dataLength - 1;

	Wire.readBytes(package, packageLength);

	SerialEx::WriteLine("I2C WRITE for action " + String(_lastAction));

	switch (_lastAction)
	{
		case I2C_ACTION_DHT22:
		{
			DHT22Controller_handleI2CWrite(package, packageLength);
			break;
		}
		case I2C_ACTION_433MHz:
		{
			LPD433::Send(package, packageLength);
			break;
		}
		case I2C_ACTION_Infrared:
		{
			Infrared::Send(package, packageLength);
			break;
		}
		case I2C_ACTION_Current:
		{
			CurrentController_handleI2CWrite(package, packageLength);
			break;
		}
	}

	digitalWrite(PIN_LED, LOW);
}

void setup() 
{
	pinMode(PIN_LED, OUTPUT);
	digitalWrite(PIN_LED, HIGH);

	SerialEx::Init();
	Infrared::Init();
	LPD433::Init();

	Wire.begin(I2C_SLAVE_ADDRESS);
	Wire.onReceive(handleI2CWrite);
	Wire.onRequest(handleI2CRead);

	digitalWrite(PIN_LED, LOW);
}

void loop() 
{ 
	Infrared::ProcessLoop();

    LPD433::ProcessLoop();

	DHT22Controller_loop();

	//CurrentController_loop();
}




