#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include "Arduino.h"
#include "OSCCommon/OSCcommon.h"
#include "OSCCommon/OSCMessage.h"
#include "OSCCommon/OSCServer.h"
#include "DMX.h"
#include "EthernetBonjour.h"

#define DEST_PORT 12345
#define MY_NAME "<arduino>"

byte destIp[4] = {0};

DMX::DMX(void)
{
	
}

bool isBlankIp(byte ip[4])
{
	for (int i=0;i<4;i++)
	{
		if (ip[i] != 0)
			return false;
	}
	return true;
}

void DMX::SetDestination(byte destination[4])
{
	for (int i=0;i<4;i++)
		destIp[i] = destination[i];
}

void DMX::Blackout(void)
{
if (!isBlankIp(destIp))
	{
  OSCMessage loacal_mes;
  loacal_mes.setAddress(destIp,DEST_PORT);
  loacal_mes.beginMessage("/dmx/blackout");
  loacal_mes.addArgString(MY_NAME);
  client.send(&loacal_mes);
  }
}

bool found = false;
bool discovery = false;

void serviceFound(const char* type, MDNSServiceProtocol proto,const char* name, const byte ipAddr[4], unsigned short port,const char* txtContent)
{
	Serial.println("Service Returned");
  if (NULL == name) {
	
  } else {
	found = true;
    Serial.println("ThorDMX: Server Found on Bonjour");
	Serial.println(name);
	Serial.println(port);
	Serial.println("IP:");
	for (byte thisByte = 0; thisByte < 4; thisByte++) {
		// print the value of each byte of the IP address:
		Serial.print(ipAddr[thisByte], DEC);
		Serial.print("."); 
	}
	for (int i=0;i<4;i++)
		destIp[i] = ipAddr[i];
  }
}

void DMX::SetAutoDestination(void)
{
	Serial.println("ThorDMX: Finding Server");
	EthernetBonjour.begin(MY_NAME);
	EthernetBonjour.setServiceFoundCallback(serviceFound);
	discovery = true;
}

void DMX::run()
{
	if (discovery)
	{
		if (!EthernetBonjour.isDiscoveringService() && !found)
		{
			EthernetBonjour.startDiscoveringService("_tcp",MDNSServiceTCP,5000);
		}
		EthernetBonjour.run();
	}
}

void DMX::UpdateChannel(int chan, int value)
{
	if (!isBlankIp(destIp))
	{
	  OSCMessage loacal_mes;
	  loacal_mes.setAddress(destIp,DEST_PORT);
	  loacal_mes.beginMessage("/dmx/updatechannel");
	  loacal_mes.addArgString(MY_NAME);
	  loacal_mes.addArgInt32(chan);
	  loacal_mes.addArgInt32(value);
	  client.send(&loacal_mes);
  }
}

void DMX::UpdateAll(int chans[])
{
if (!isBlankIp(destIp))
	{
  OSCMessage loacal_mes;
  loacal_mes.setAddress(destIp,DEST_PORT);
  loacal_mes.beginMessage("/dmx/frameupdate");
  loacal_mes.addArgString(MY_NAME);
  loacal_mes.addArgInt32(0);
  loacal_mes.addArgInt32(0);
  for (int i=0;i<512;i++)
	loacal_mes.addArgInt32(chans[i]);
  client.send(&loacal_mes);
  }
}