#include <SPI.h>
#include <Ethernet.h>
#include "ThorDMX.h"

byte myMac[] = {0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };
byte server[] = {192, 168, 0, 177};

void setup(){ 
  Ethernet.begin(myMac);
  //set server manually
  ThorDMX.SetDestination(server);
  //find server on the network automatically
  ThorDMX.SetAutoDestination();
}

void loop()
{
   ThorDMX.UpdateChannel(1,255);
   ThorDMX.run();
   delay(200);
}

