#include <Arduino.h>
#include <ESP32Servo.h>
#include "config.h"
#include "network/ApiClient.h"

Servo gateServo;
ApiClient apiClient;

unsigned long lastHeartbeat = 0;
unsigned long lastGateStateCheck = 0;
unsigned long lastSensorRead = 0;
String lastGateState = "";

void sendHeartbeat();
void checkGateState();
void readAndSendSensorData();

void setup() {
  Serial.begin(115200);
  delay(1000);
  
  Serial.println();
  Serial.println("========================================");
  Serial.println("GateSense - Gate Control + Sensors");
  Serial.println("========================================");
  Serial.println();
  
  gateServo.attach(PIN_SERVO_PWM);
  gateServo.write(SERVO_ANGLE_CLOSED);
  
  pinMode(PIN_MQ2_AOUT, INPUT);
  
  Serial.println("Gate controller initialized");
  Serial.println("MQ-2 sensor initialized");
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
  
  if (currentTime - lastSensorRead >= SENSOR_READ_INTERVAL) {
    lastSensorRead = currentTime;
    readAndSendSensorData();
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

void readAndSendSensorData() {
  int sensorValue = analogRead(PIN_MQ2_AOUT);
  float voltage = (sensorValue / 4095.0) * 3.3;
  float smokePpm = (voltage - 0.4) * 1000.0 / 1.6;
  if (smokePpm < 0) smokePpm = 0;
  
  Serial.print("MQ-2 Reading: ");
  Serial.print(sensorValue);
  Serial.print(" (");
  Serial.print(voltage);
  Serial.print("V, ~");
  Serial.print(smokePpm);
  Serial.println(" ppm)");
  
  if (apiClient.sendSensorData("Smoke", smokePpm, "ppm")) {
    Serial.println("Sensor data sent successfully");
  } else {
    Serial.println("Failed to send sensor data");
  }
}
