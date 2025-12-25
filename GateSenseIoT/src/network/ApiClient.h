#ifndef API_CLIENT_H
#define API_CLIENT_H

#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>

struct GateState {
  String state;
  bool isValid;
};

class ApiClient {
public:
  ApiClient();
  bool begin();
  bool connectWiFi();
  bool sendHeartbeat();
  GateState getGateState();
  bool isWiFiConnected();
  
private:
  HTTPClient http;
  WiFiClientSecure client;
  String baseUrl;
  String serialNumber;
  int garageId;
  bool isHttps();
};

#endif

