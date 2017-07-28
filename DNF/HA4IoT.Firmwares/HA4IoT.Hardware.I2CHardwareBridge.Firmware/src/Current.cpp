
#include "common.h"
#include "SerialEx.h"
#include "Current.h"

#define ACTION_REGISTER_SENSOR 0
#define POLL_INTERVAL 1000UL
#define SAMPLE_TIME 50
#define VALUE_ON_LEVEL 0.15

uint8_t Current::Pins[8];
uint8_t Current::PinsIndex = 0;
uint8_t Current::Cache[8][1];
int Current::Min[8];
int Current::Max[8];
unsigned long Current::LastMillies = millis();

int Current::GetPinIndex(uint8_t pin)
{
	for (int i = 0; i < Current::PinsIndex; i++)
	{
		if (Current::Pins[i] == pin)
		{
			return i;
		}
	}
	return -1;
}

void Current::Register(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 2) return;
	uint8_t action = package[0];

	switch (action)
	{
		case ACTION_REGISTER_SENSOR:
		{
			uint8_t pin = package[1];

			if (Current::GetPinIndex(pin) == -1)
			{
				Current::Pins[Current::PinsIndex] = pin;
				Current::Cache[Current::PinsIndex][0] = 0; 
				Current::PinsIndex++;
			}
			break;
		}
	}
}

void Current::ReadCurrent()
{
	for (int i = 0; i < Current::PinsIndex; i++)
	{
		Current::Min[i] = 1024;
		Current::Max[i] = 0;
	}

	uint32_t start_time = millis();
	while ((millis() - start_time) < SAMPLE_TIME)
	{
		for (int i = 0; i < Current::PinsIndex; i++)
		{
			int readValue = analogRead(Current::Pins[i]);

			if (readValue > Current::Max[i])
			{
				Current::Max[i] = readValue;
			}
			if (readValue < Current::Min[i])
			{
				Current::Min[i] = readValue;
			}
		}
	}

	for (uint8_t i = 0; i < Current::PinsIndex; i++)
	{
		int current = Current::Max[i] - Current::Min[i];
		float voltage = (current * 5.0) / 1024.0;
		uint8_t value = 0;
		int last_value = Current::Cache[i][0];
		
		if(voltage > VALUE_ON_LEVEL)
		{
			value = 1;
		}

		Current::Cache[i][0] = value;
		if(value != last_value)
		{
			Serial.write(sizeof(value) + sizeof(i));
			Serial.write(I2C_ACTION_Current);
			Serial.write(i);
			Serial.write(value);
			Serial.flush();
		}
	}
}

void Current::ProcessLoop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - Current::LastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		Current::LastMillies = currentMillies;
		Current::ReadCurrent();
	}
}