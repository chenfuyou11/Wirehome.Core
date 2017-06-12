#include "Arduino.h"
#include <RCSwitch.h>

RCSwitch mySwitch = RCSwitch();

void setup()
{
   //mySwitch.enableTransmit(12);
   mySwitch.enableReceive(digitalPinToInterrupt(3));

   Serial.begin(9600);
}

void loop()
{
      if (mySwitch.available())
      {

        int value = mySwitch.getReceivedValue();

        if (value == 0)
        {
          Serial.print("Unknown encoding");
        }
        else
        {
          Serial.print("Received ");
          Serial.print( mySwitch.getReceivedValue() );
          Serial.print(" / ");
          Serial.print( mySwitch.getReceivedBitlength() );
          Serial.print("bit ");
          Serial.print("Protocol: ");
          Serial.println( mySwitch.getReceivedProtocol() );
        }

        mySwitch.resetAvailable();
    }

    return;

    mySwitch.switchOn("11111", "00010");
    delay(1000);
    mySwitch.switchOff("11111", "00010");
    delay(1000);
}
