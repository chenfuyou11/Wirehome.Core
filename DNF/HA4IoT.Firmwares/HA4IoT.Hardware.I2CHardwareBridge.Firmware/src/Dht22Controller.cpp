#include "Dht22Reader.h"
#include <Arduino.h>
#include "SerialEx.h"

#define ACTION_REGISTER_SENSOR 0
#define POLL_INTERVAL 2500UL
#define RESPONSE_SIZE 9UL

uint8_t _pins[16];
uint8_t _pinsIndex = 0;
uint8_t _cache[16][RESPONSE_SIZE];
uint8_t _pinForRead = 0;

unsigned long _lastMillies = millis();

union floatToBytes
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

int getPinIndex(uint8_t pin)
{
	for (int i = 0; i < _pinsIndex; i++)
	{
		if (_pins[i] == pin)
		{
			return i;
		}
	}

	return -1;
}

void DHT22Controller_handleI2CWrite(uint8_t package[], uint8_t packageLength)
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

			if (getPinIndex(pin) == -1)
			{
				_pins[_pinsIndex] = pin;
				_cache[_pinsIndex][0] = 0; // Set default to "no success".

				_pinsIndex++;		
			}
		
			_pinForRead = pin;

			break;
		}
	}
    
	SerialEx::SendMessage("Set sensor PIN to " + String(_pinForRead));
}

uint8_t DHT22Controller_handleI2CRead(uint8_t response[])
{
	int pinIndex = getPinIndex(_pinForRead);
	if (pinIndex == -1)
	{
		SerialEx::SendMessage("Cache value for " + String(_pinForRead) + " not found");
		return 0;
	}

	SerialEx::SendMessage("Fetching cache value from index " + String(pinIndex));

	for (int i = 0; i < RESPONSE_SIZE; i++)
	{
		response[i] = _cache[pinIndex][i];
	}

	return RESPONSE_SIZE;
}

void pollSensors()
{
	// Example cache bytes:
	// 0 = SUCCESS
	// 1 = TEMP_0
	// 2 = TEMP_1
	// 3 = TEMP_2
	// 4 = TEMP_3
	// 5 = HUM_0
	// 6 = HUM_1
	// 7 = HUM_2
	// 8 = HUM_3
	for (int i = 0; i < _pinsIndex; i++)
	{
		Dht22Reader dht22Reader = Dht22Reader(_pins[i]);
		dht22Reader.setup();
		boolean success = dht22Reader.read();

		float temperature = 0.0F;
		float humidity = 0.0F;

		if (success)
		{
			_cache[i][0] = 1;
			temperature = dht22Reader.getTemperature();
			humidity = dht22Reader.getHumidity();
		}
		else
		{
			_cache[i][0] = 0;
		}

		union floatToBytes converter;
		converter.value = temperature;
		_cache[i][1] = converter.bytes.b0;
		_cache[i][2] = converter.bytes.b1;
		_cache[i][3] = converter.bytes.b2;
		_cache[i][4] = converter.bytes.b3;
		converter.value = humidity;
		_cache[i][5] = converter.bytes.b0;
		_cache[i][6] = converter.bytes.b1;
		_cache[i][7] = converter.bytes.b2;
		_cache[i][8] = converter.bytes.b3;

		SerialEx::SendMessage("Read DHT22 sensor SUCCESS:");
		SerialEx::SendMessage(String(success));
		SerialEx::SendMessage(",TEMP:");
		SerialEx::SendMessage(String(temperature));
		SerialEx::SendMessage(",HUM:");
		SerialEx::SendMessage(String(humidity));
	}
}

void DHT22Controller_loop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - _lastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		_lastMillies = currentMillies;
		pollSensors();
	}
}

