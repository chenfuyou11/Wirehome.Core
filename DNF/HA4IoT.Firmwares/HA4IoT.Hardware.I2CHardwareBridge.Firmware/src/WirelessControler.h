#include "Arduino.h"
#include <stdint.h>

#define RCSWITCH_MAX_CHANGES 67

class WirelessControler 
{

  public:
	WirelessControler();
    
	void SendSignal(unsigned long code, unsigned int length, int repeatCount,  int pin);

    bool HaveValue();
    void ResetValue();
	void StopListening();
	void StartListeningOnPin(int interrup);


    unsigned long GetValue();
    unsigned int GetValueLength();
    
  private:
	int nPinSending;
	int nPinReciving;

    void SendSequence(uint8_t high, uint8_t low);

    static void SignalHandler();
    static bool SignalHandlerHelper(unsigned int changeCount);
    
    static unsigned long Value;
    static unsigned int ValueBitlength;
    static unsigned int ValueDelay;

    static unsigned int RawData[RCSWITCH_MAX_CHANGES];

    
};
