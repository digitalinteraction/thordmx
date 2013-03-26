#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include "Arduino.h"
#include "OSCCommon/OSCClient.h"
#include "EthernetBonjour.h"

class DMX{
	private:
		OSCClient client;
	public:
		DMX(void);
		void SetDestination(byte destination[4]);
		void Blackout(void);
		void UpdateChannel(int chan, int val);
		void UpdateAll(int chans[]);
		void SetAutoDestination();
		void run();
};