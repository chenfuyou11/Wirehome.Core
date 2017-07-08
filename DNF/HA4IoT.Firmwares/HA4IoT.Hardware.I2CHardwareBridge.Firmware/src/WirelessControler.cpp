#include "WirelessControler.h"

#define RECEIVE_ATTR
#define ONE_HIGH 3
#define ONE_LOW 1
#define ZERO_HIGH 1
#define ZERO_LOW 3
#define SYNC_HIGH 1
#define SYNC_LOW 31
#define PULSE_LENGTH 350
#define SEPARATION_LIMIT 4600
#define RECIVE_TOLERANCE 60

unsigned int WirelessControler::RawData[RCSWITCH_MAX_CHANGES];
unsigned long WirelessControler::Value = 0;
unsigned int WirelessControler::ValueBitlength = 0;
unsigned int WirelessControler::ValueDelay = 0;

WirelessControler::WirelessControler()
{
	this->nPinSending = -1;
	this->nPinReciving = -1;

	this->ResetValue();
}


void WirelessControler::SendSignal(unsigned long code, unsigned int length, int repeatCount, int pin)
{

	if (pin == -1)
	{
		return;
	}
	if (this->nPinSending != pin)
	{
		this->nPinSending = pin;
		pinMode(pin, OUTPUT);
	}

	// disable reciving if enabled
	int nPinRecivingState = nPinReciving;
	if (nPinRecivingState != -1)
	{
		this->StopListening();
	}

	for (int r = 0; r < repeatCount; r++)
	{
		for (int i = length - 1; i >= 0; i--)
		{
			if (code & (1L << i))
			{
				this->SendSequence(ONE_HIGH, ONE_LOW);
			}
			else
			{
				this->SendSequence(ZERO_HIGH, ZERO_LOW);
			}
		}
		this->SendSequence(SYNC_HIGH, SYNC_LOW);
	}

	// revert reciving state
	if (nPinRecivingState != -1)
	{
		this->StartListeningOnPin(nPinRecivingState);
	}
}

void WirelessControler::SendSequence(uint8_t high, uint8_t low)
{
	digitalWrite(this->nPinSending, HIGH);
	delayMicroseconds(PULSE_LENGTH * high);
	digitalWrite(this->nPinSending, LOW);
	delayMicroseconds(PULSE_LENGTH * low);
}

void WirelessControler::StartListeningOnPin(int pin)
{
	if (pin != -1)
	{
		// Stop listening if current port is diffrent than recived
		if (pin != this->nPinReciving && this->nPinReciving > -1)
		{
			StopListening();
		}
		// If we are already listening on this port
		else if (pin == this->nPinReciving)
		{
			return;
		}

		this->ResetValue();

		this->nPinReciving = pin;

		attachInterrupt(this->nPinReciving, SignalHandler, CHANGE);

	}
}



void WirelessControler::StopListening()
{
	detachInterrupt(this->nPinReciving);
}

bool WirelessControler::HaveValue()
{
	return WirelessControler::Value != 0;
}


unsigned long WirelessControler::GetValue()
{
	return WirelessControler::Value;
}

unsigned int WirelessControler::GetValueLength()
{
	return WirelessControler::ValueBitlength;
}

void WirelessControler::ResetValue()
{
	WirelessControler::Value = 0;
	WirelessControler::ValueBitlength = 0;
	WirelessControler::ValueDelay = 0;
}


static inline unsigned int CalculateDiff(int A, int B)
{
	return abs(A - B);
}

void RECEIVE_ATTR WirelessControler::SignalHandler()
{
	static unsigned int changeCount = 0;
	static unsigned long lastTime = 0;
	static unsigned int repeatCount = 0;

	const long time = micros();
	const unsigned int duration = time - lastTime;

	if (duration > SEPARATION_LIMIT)
	{
		if (CalculateDiff(duration, WirelessControler::RawData[0]) < 200)
		{
			repeatCount++;
			if (repeatCount == 2)
			{
				SignalHandlerHelper(changeCount);

				repeatCount = 0;
			}
		}
		changeCount = 0;
	}

	if (changeCount >= RCSWITCH_MAX_CHANGES)
	{
		changeCount = 0;
		repeatCount = 0;
	}

	WirelessControler::RawData[changeCount++] = duration;
	lastTime = time;
}


bool RECEIVE_ATTR WirelessControler::SignalHandlerHelper(unsigned int changeCount)
{
	unsigned long code = 0;

	const unsigned int syncLengthInPulses = ((SYNC_LOW) > (SYNC_HIGH)) ? (SYNC_LOW) : (SYNC_HIGH);
	const unsigned int delay = WirelessControler::RawData[0] / syncLengthInPulses;
	const unsigned int delayTolerance = delay * RECIVE_TOLERANCE / 100;

	const unsigned int firstDataTiming = (1);

	for (unsigned int i = firstDataTiming; i < changeCount - 1; i += 2)
	{
		code <<= 1;
		if (CalculateDiff(WirelessControler::RawData[i], delay * ZERO_HIGH) < delayTolerance && CalculateDiff(WirelessControler::RawData[i + 1], delay * ZERO_LOW) < delayTolerance)
		{
			// ZERO
		}
		else if (CalculateDiff(WirelessControler::RawData[i], delay * ONE_HIGH) < delayTolerance && CalculateDiff(WirelessControler::RawData[i + 1], delay * ONE_LOW) < delayTolerance)
		{
			// ONE
			code |= 1;
		}
		else
		{
			// ERROR
			return false;
		}
	}

	if (changeCount > 7)
	{    // ignore very short transmissions: no device sends them, so this must be noise
		WirelessControler::Value = code;
		WirelessControler::ValueBitlength = (changeCount - 1) / 2;
		WirelessControler::ValueDelay = delay;
	}

	return true;
}







