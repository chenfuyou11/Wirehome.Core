#include <Wire.h>
#include "SerialEx.h"
#include "Infrared.h"
#include "Dht22Controller.h"
#include "LPD433MhzController.h"
#include "CurrentController.h"

#define IS_HIGH(pin) (PIND & (1<<pin))
#define IS_LOW(pin) ((PIND & (1<<pin))==0)
#define SET_HIGH(pin) (PORTD) |= (1<<(pin))
#define SET_LOW(pin) (PORTD) &= (~(1<<(pin)))

#define I2C_SLAVE_ADDRESS 50
#define LED 13

#define I2C_ACTION_DHT22 1
#define I2C_ACTION_433MHz 2
#define I2C_ACTION_Infrared 3
#define I2C_ACTION_433MHz_Read 4
#define I2C_ACTION_Current 5

uint8_t _lastAction = 0;

void handleI2CRead()
{
	SerialEx::WriteText("I2C READ for action" + String(_lastAction));
	digitalWrite(LED, HIGH);

	uint8_t response[32];
	size_t responseLength = 0;

	switch (_lastAction)
	{
		case I2C_ACTION_DHT22:
		{
			responseLength = DHT22Controller_handleI2CRead(response);
			break;
		}
		case I2C_ACTION_433MHz_Read:
		{
			responseLength = LPD433MhzController_handleI2CRead(response);
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

	digitalWrite(LED, LOW);
}

void handleI2CWrite(int dataLength)
{
	if (dataLength == 0) return;

	if (dataLength > 32) 
	{ 
		SerialEx::WriteError(F("Received too large package"));
		return;
	}

	digitalWrite(LED, HIGH);

	_lastAction = Wire.read();

	uint8_t package[32];
	size_t packageLength = dataLength - 1;

	Wire.readBytes(package, packageLength);

	SerialEx::WriteText("I2C WRITE for action " + String(_lastAction));

	switch (_lastAction)
	{
		case I2C_ACTION_DHT22:
		{
			DHT22Controller_handleI2CWrite(package, packageLength);
			break;
		}
		case I2C_ACTION_433MHz:
		{
			LPD433MhzController_handleI2CWrite(package, packageLength);
			break;
		}
		case I2C_ACTION_Infrared:
		{
			Infrared::Send(package, packageLength);
			break;
		}
		case I2C_ACTION_433MHz_Read:
		{
			LPD433MhzController_SetReadMode(package, packageLength);
			break;
		}
		case I2C_ACTION_Current:
		{
			CurrentController_handleI2CWrite(package, packageLength);
			break;
		}

	}

	digitalWrite(LED, LOW);
}

void setup() 
{
	pinMode(LED, OUTPUT);
	digitalWrite(LED, HIGH);

    Serial.begin(9600);

	//SerialEx::Init();
	//Infrared::Init();

	 Wire.begin(I2C_SLAVE_ADDRESS);
	 Wire.onReceive(handleI2CWrite);
	 Wire.onRequest(handleI2CRead);

	digitalWrite(LED, LOW);
}

void loop() 
{ 
	//Infrared::ProcessLoop();
    //uint8_t value = 15;
    //uint8_t value2 = 30;
	int value = 15;
    digitalWrite(LED, HIGH);
	Serial.write(value);
	//Serial.write(value2);
    
	delay(1000);

	digitalWrite(LED, LOW);

	// DHT22Controller_loop();

	// LPD433MhzController_loop();

	// CurrentController_loop();
}




