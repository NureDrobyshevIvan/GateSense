#include <Arduino.h>
#include <ESP32Servo.h>
#include "config.h"
#include "network/ApiClient.h"

Servo gateServo;
ApiClient apiClient;

unsigned long lastHeartbeat = 0;
unsigned long lastGateStateCheck = 0;
String lastGateState = "";

void sendHeartbeat();
void checkGateState();

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  Serial.println();
  Serial.println("========================================");
  Serial.println("GateSense - Gate Control");
  Serial.println("========================================");
  Serial.println();
  
  gateServo.attach(PIN_SERVO_PWM);
  gateServo.write(SERVO_ANGLE_CLOSED);
  
  Serial.println("Gate controller initialized");
  Serial.println("Gate is CLOSED (angle: 0°)");
  Serial.println();
  
  if (!apiClient.begin()) {
    Serial.println("WARNING: WiFi connection failed. Will retry...");
  }
  
  Serial.println("Starting gate control loop...");
  Serial.println();
}

void loop() {
  unsigned long currentTime = millis();
  
  if (currentTime - lastHeartbeat >= HEARTBEAT_INTERVAL) {
    lastHeartbeat = currentTime;
    sendHeartbeat();
  }
  
  if (currentTime - lastGateStateCheck >= GATE_STATE_CHECK_INTERVAL) {
    lastGateStateCheck = currentTime;
    checkGateState();
  }
  
  if (!apiClient.isWiFiConnected()) {
    Serial.println("WiFi disconnected. Attempting to reconnect...");
    apiClient.connectWiFi();
    delay(WIFI_RECONNECT_DELAY);
  }
  
  delay(100);
}

void sendHeartbeat() {
  Serial.println("Sending heartbeat...");
  if (apiClient.sendHeartbeat()) {
    Serial.println("Heartbeat sent successfully");
  } else {
    Serial.println("Failed to send heartbeat");
  }
}

void checkGateState() {
  GateState gateState = apiClient.getGateState();
  
  if (!gateState.isValid) {
    Serial.println("Failed to get gate state from server");
    return;
  }
  
  if (gateState.state != lastGateState) {
    Serial.print("Gate state changed: ");
    Serial.print(lastGateState);
    Serial.print(" -> ");
    Serial.println(gateState.state);
    
    if (gateState.state == "Open") {
      Serial.println(">>> Opening gate...");
      gateServo.write(SERVO_ANGLE_OPEN);
      Serial.print("Gate angle set to: ");
      Serial.print(SERVO_ANGLE_OPEN);
      Serial.println("° (OPEN)");
    } else if (gateState.state == "Closed") {
      Serial.println(">>> Closing gate...");
      gateServo.write(SERVO_ANGLE_CLOSED);
      Serial.print("Gate angle set to: ");
      Serial.print(SERVO_ANGLE_CLOSED);
      Serial.println("° (CLOSED)");
    }
    
    lastGateState = gateState.state;
  }
}
