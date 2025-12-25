#ifndef CONFIG_H
#define CONFIG_H

// WiFi налаштування
#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""

// API налаштування
// Railway production URL
#define API_BASE_URL "https://gatesense-production.up.railway.app"
#define API_HEARTBEAT_ENDPOINT "/iot/heartbeat"
#define API_GATE_STATE_ENDPOINT "/iot/gate-state"
#define API_SENSOR_DATA_ENDPOINT "/iot/sensor-data"

// Пристрій
#define DEVICE_SERIAL_NUMBER "ESP32-001"
#define GARAGE_ID 1

// Пін для Servo PWM сигналу
#define PIN_SERVO_PWM 5
#define PIN_MQ2_AOUT 34

// Кути для Servo (ворота)
#define SERVO_ANGLE_CLOSED 0
#define SERVO_ANGLE_OPEN 90

// Таймери (в мілісекундах)
#define HEARTBEAT_INTERVAL 30000
#define GATE_STATE_CHECK_INTERVAL 10000
#define SENSOR_READ_INTERVAL 5000

// Налаштування WiFi підключення
#define WIFI_CONNECT_TIMEOUT 30000
#define WIFI_RECONNECT_DELAY 5000

#endif

