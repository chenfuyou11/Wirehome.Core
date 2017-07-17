#pragma once
#include <Arduino.h>

class SerialEx
{
  public:
    static void Init();
    static void WriteText(const String &message);
    static void WriteLine(const String &message);
    static void WriteError(const String &message);
};