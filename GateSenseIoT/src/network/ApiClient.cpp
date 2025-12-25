#include "ApiClient.h"
#include "../config.h"

ApiClient::ApiClient() {
  baseUrl = String(API_BASE_URL);
  serialNumber = String(DEVICE_SERIAL_NUMBER);
  garageId = GARAGE_ID;
}

bool ApiClient::begin() {
  return connectWiFi();
}

bool ApiClient::connectWiFi() {
  if (WiFi.status() == WL_CONNECTED) {
    return true;
  }
  
  Serial.print("Connecting to WiFi: ");
  Serial.println(WIFI_SSID);
  
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
  
  unsigned long startTime = millis();
  while (WiFi.status() != WL_CONNECTED && (millis() - startTime) < WIFI_CONNECT_TIMEOUT) {
    delay(500);
    Serial.print(".");
  }
  
  if (WiFi.status() == WL_CONNECTED) {
    Serial.println();
    Serial.print("WiFi connected! IP: ");
    Serial.println(WiFi.localIP());
    return true;
  } else {
    Serial.println();
    Serial.println("WiFi connection failed!");
    return false;
  }
}

bool ApiClient::isWiFiConnected() {
  return WiFi.status() == WL_CONNECTED;
}

bool ApiClient::sendHeartbeat() {
  if (!isWiFiConnected()) {
    if (!connectWiFi()) {
      return false;
    }
  }
  
  String endpoint = String(API_BASE_URL) + String(API_HEARTBEAT_ENDPOINT);
  
  StaticJsonDocument<128> doc;
  doc["serialNumber"] = serialNumber;
  
  String jsonBody;
  serializeJson(doc, jsonBody);
  
  http.setTimeout(5000);
  http.begin(endpoint);
  http.addHeader("Content-Type", "application/json");
  
  int httpCode = http.POST(jsonBody);
  
  if (httpCode == 200 || httpCode == 201) {
    Serial.println("Heartbeat sent successfully");
    http.end();
    return true;
  } else {
    Serial.print("Failed to send heartbeat. HTTP code: ");
    Serial.println(httpCode);
    http.end();
    return false;
  }
}

GateState ApiClient::getGateState() {
  GateState gateState;
  gateState.isValid = false;
  
  if (!isWiFiConnected()) {
    if (!connectWiFi()) {
      return gateState;
    }
  }
  
  String endpoint = String(API_BASE_URL) + String(API_GATE_STATE_ENDPOINT) + "?serialNumber=" + serialNumber;
  
  http.setTimeout(5000);
  http.begin(endpoint);
  int httpCode = http.GET();
  
  if (httpCode == 200) {
    String payload = http.getString();
    http.end();
    
    StaticJsonDocument<256> doc;
    DeserializationError error = deserializeJson(doc, payload);
    
    if (!error) {
      gateState.state = doc["state"] | "Unknown";
      gateState.isValid = true;
    } else {
      Serial.print("JSON parse error: ");
      Serial.println(error.c_str());
    }
  } else if (httpCode > 0) {
    http.end();
    Serial.print("Failed to get gate state. HTTP code: ");
    Serial.println(httpCode);
  } else {
    http.end();
    Serial.print("Connection failed. Error code: ");
    Serial.println(httpCode);
    Serial.println("Note: In Wokwi, 'localhost' doesn't work. Use ngrok or public IP.");
  }
  
  return gateState;
}


