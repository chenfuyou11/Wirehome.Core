#include <Arduino.h>
#include "SerialEx.h"

#define ACTION_REGISTER_SENSOR 0
#define POLL_INTERVAL 2000UL
#define RESPONSE_SIZE 5UL

uint8_t _CurrentPins[16];
uint8_t _CurrentPinsIndex = 0;
uint8_t _CurrentCache[16][RESPONSE_SIZE];
int _CurrentMin[16];
int _CurrentMax[16];
uint8_t _CurrentPinForRead = 0;
int _CurrentSampleTime = 500;

unsigned long _CurrentLastMillies = millis();

union CurrentFloatToBytes
{
	float value;
	struct
	{
		uint8_t b0;
		uint8_t b1;
		uint8_t b2;
		uint8_t b3;
	} bytes;
};

int getCurrentPinIndex(uint8_t pin)
{
	for (int i = 0; i < _CurrentPinsIndex; i++)
	{
		if (_CurrentPins[i] == pin)
		{
			return i;
		}
	}

	return -1;
}

void CurrentController_handleI2CWrite(uint8_t package[], uint8_t packageLength)
{
	// Example request bytes:
	// 0 = ACTION
	// 1 = PIN
	if (packageLength != 2) return;

	uint8_t action = package[0];

	switch (action)
	{
		case ACTION_REGISTER_SENSOR:
		{
			uint8_t pin = package[1];

			if (getCurrentPinIndex(pin) == -1)
			{
				_CurrentPins[_CurrentPinsIndex] = pin;
				_CurrentCache[_CurrentPinsIndex][0] = 0; // Set default to "no success".

				_CurrentPinsIndex++;
			}
		
			_CurrentPinForRead = pin;

			break;
		}
	}

	SerialEx::WriteLine("Set sensor PIN to " + String(_CurrentPinForRead));
}

uint8_t CurrentController_handleI2CRead(uint8_t response[])
{
	int pinIndex = getCurrentPinIndex(_CurrentPinForRead);
	if (pinIndex == -1)
	{
		SerialEx::WriteLine("Cache value for " + String(_CurrentPinForRead) + " not found");
		return 0;
	}

	SerialEx::WriteLine("Fetching cache value from index " + String(pinIndex));

	for (int i = 0; i < RESPONSE_SIZE; i++)
	{
		response[i] = _CurrentCache[pinIndex][i];
	}

	return RESPONSE_SIZE;
}

void getVPP()
{
	int readValue;  //value read from the sensor

	// prepare min/max value
	for (int i = 0; i < _CurrentPinsIndex; i++)
	{
		_CurrentMin[i] = 1024;
		_CurrentMax[i] = 0;
	}

	// find min and max values for sample_time
	uint32_t start_time = millis();
	while ((millis() - start_time) < _CurrentSampleTime) //sample for x Sec
	{
		for (int i = 0; i < _CurrentPinsIndex; i++)
		{
			readValue = analogRead(_CurrentPins[i]);
			// get min and max values
			if (readValue > _CurrentMax[i])
			{
				_CurrentMax[i] = readValue;
			}
			if (readValue < _CurrentMin[i])
			{
				_CurrentMin[i] = readValue;
			}
		}
	}

	// prepare results
	for (int i = 0; i < _CurrentPinsIndex; i++)
	{
		_CurrentCache[i][0] = 1;
		int current = _CurrentMax[i] - _CurrentMin[i];

		float voltage = (current * 5.0) / 1024.0;
		union CurrentFloatToBytes converter;

		converter.value = voltage;
		_CurrentCache[i][1] = converter.bytes.b0;
		_CurrentCache[i][2] = converter.bytes.b1;
		_CurrentCache[i][3] = converter.bytes.b2;
		_CurrentCache[i][4] = converter.bytes.b3;

		SerialEx::WriteLine(",Current:");
		SerialEx::WriteText(String(voltage));
	}
}

void CurrentController_loop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - _CurrentLastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		_CurrentLastMillies = currentMillies;
		getVPP();
	}
}



