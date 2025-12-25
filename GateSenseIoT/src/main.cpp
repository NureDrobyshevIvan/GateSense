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
    return;
  }
  
  if (gateState.state != lastGateState) {
    if (gateState.state == "Open") {
      gateServo.write(SERVO_ANGLE_OPEN);
    } else if (gateState.state == "Closed") {
      gateServo.write(SERVO_ANGLE_CLOSED);
    }
    
    lastGateState = gateState.state;
  }
}
