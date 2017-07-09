#pragma once
#include <Arduino.h>

#include "IRLibDecodeBase.h"
#include "IRLib_P02_Sony.h"
#include "IRLib_P09_GICable.h"
#include "IRLibCombo.h"
#include "IRLibRecvPCI.h"

class Infrared
{
  private:
    static IRdecode myDecoder;   //create decoder
    static IRrecvPCI  myReceiver; //receiver on pin 2
  public:
    static void Init();
    static void ProcessLoop();
    static void Send(uint8_t package[], uint8_t packageLength);
};

