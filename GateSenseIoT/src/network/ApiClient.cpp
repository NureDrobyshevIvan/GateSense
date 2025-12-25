#include "ApiClient.h"
#include "../config.h"

ApiClient::ApiClient() {
  baseUrl = String(API_BASE_URL);
  serialNumber = String(DEVICE_SERIAL_NUMBER);
  garageId = GARAGE_ID;
  
  if (isHttps()) {
    client.setInsecure();
  }
}

bool ApiClient::isHttps() {
  return baseUrl.startsWith("https://");
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
  if (isHttps()) {
    http.begin(client, endpoint);
  } else {
    http.begin(endpoint);
  }
  http.addHeader("Content-Type", "application/json");
  
  int httpCode = http.POST(jsonBody);
  http.end();
  
  if (httpCode == 200 || httpCode == 201) {
    return true;
  } else {
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
  if (isHttps()) {
    http.begin(client, endpoint);
  } else {
    http.begin(endpoint);
  }
  int httpCode = http.GET();
  
  if (httpCode == 200) {
    String payload = http.getString();
    http.end();
    
    StaticJsonDocument<512> doc;
    DeserializationError error = deserializeJson(doc, payload);
    
    if (!error) {
      if (doc.containsKey("data")) {
        gateState.state = doc["data"]["state"] | "Unknown";
      } else {
        gateState.state = doc["state"] | "Unknown";
      }
      gateState.isValid = true;
    }
  } else {
    http.end();
  }
  
  return gateState;
}

bool ApiClient::sendSensorData(String sensorType, float value, String unit) {
  if (!isWiFiConnected()) {
    if (!connectWiFi()) {
      return false;
    }
  }
  
  String endpoint = String(API_BASE_URL) + String(API_SENSOR_DATA_ENDPOINT);
  
  unsigned long now = millis();
  unsigned long seconds = now / 1000;
  
  char timestamp[30];
  snprintf(timestamp, sizeof(timestamp), "2025-01-01T00:00:%02lu.000Z", seconds % 86400);
  
  StaticJsonDocument<256> doc;
  doc["serialNumber"] = serialNumber;
  doc["sensorType"] = sensorType;
  doc["value"] = value;
  doc["unit"] = unit;
  doc["recordedOn"] = timestamp;
  
  String jsonBody;
  serializeJson(doc, jsonBody);
  
  http.setTimeout(5000);
  if (isHttps()) {
    http.begin(client, endpoint);
  } else {
    http.begin(endpoint);
  }
  http.addHeader("Content-Type", "application/json");
  
  int httpCode = http.POST(jsonBody);
  http.end();
  
  return (httpCode == 200 || httpCode == 201);
}

