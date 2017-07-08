#pragma once
#include <Arduino.h>

void CurrentController_handleI2CWrite(uint8_t package[], uint8_t packageLength);
size_t CurrentController_handleI2CRead(uint8_t *response);
void CurrentController_loop();

