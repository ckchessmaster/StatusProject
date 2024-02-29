/*

*/

#include "definitions.h"
#include <IRremote.hpp>
#include <serial-readline.h>

typedef enum SerialCommand {
  IR_POWER_OFF = 0,
  IR_POWER_ON = 1,
  IR_COLOR_RED = 2,
  IR_COLOR_GREEN = 3,
  IR_COLOR_PURPLE = 4
};

void received(char*);

SerialLineReader reader(Serial, received);

void setup() {
  Serial.begin(9600);
  
  // Start the receiver and if not 3. parameter specified, take LED_BUILTIN pin from the internal boards definition as default feedback LED
  IrReceiver.begin(IR_RECEIVE_PIN, ENABLE_LED_FEEDBACK);
  Serial.println("Ready to receive IR signals at pin " + String(IR_RECEIVE_PIN));

  IrSender.begin(ENABLE_LED_FEEDBACK);
  Serial.println("Ready to send IR signals at pin " + String(IR_SEND_PIN));
}

void loop() {
  // Handle serial input
  reader.poll();

  // Handle IR input
  if (IrReceiver.decode()) {
    Serial.print(F("IR command recieved: "));
    IrReceiver.printIRResultShort(&Serial);

    if (IrReceiver.decodedIRData.protocol != UNKNOWN)
    {
      Serial.println(F("Forwarding..."));
      IrSender.sendNEC(IrReceiver.decodedIRData.address, IrReceiver.decodedIRData.command, 1);
    }
    else
    {
      Serial.println(F("Unknown Protocol, skipping..."));
    }

    IrReceiver.resume(); // Enable receiving of the next value
    Serial.println();
  }
}

void received(char *line) {
  String stringLine = String(line);
  Serial.println("Serial command recieved: " + stringLine);

  switch (stringLine.toInt()) {
    case IR_POWER_OFF: IrSender.sendNEC(IR_DEFAULT_ADDRESS_HEX, IR_POWER_OFF_HEX, 1); break;
    case IR_POWER_ON: IrSender.sendNEC(IR_DEFAULT_ADDRESS_HEX, IR_POWER_ON_HEX, 1); break;
    case IR_COLOR_RED: IrSender.sendNEC(IR_DEFAULT_ADDRESS_HEX, IR_COLOR_RED_HEX, 1); break;
    case IR_COLOR_GREEN: IrSender.sendNEC(IR_DEFAULT_ADDRESS_HEX, IR_COLOR_GREEN_HEX, 1); break;
    case IR_COLOR_PURPLE: IrSender.sendNEC(IR_DEFAULT_ADDRESS_HEX, IR_COLOR_PURPLE_HEX, 1); break;
    default: break;
  }
}