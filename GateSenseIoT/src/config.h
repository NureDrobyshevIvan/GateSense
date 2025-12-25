#ifndef CONFIG_H
#define CONFIG_H

// WiFi налаштування
#define WIFI_SSID "Wokwi-GUEST"
#define WIFI_PASSWORD ""

// API налаштування
// Для Wokwi: використовуйте ngrok або публічну адресу
// Для реального пристрою: використовуйте IP адресу вашого сервера
// Для задеплоєного сервера: використовуйте публічну URL адресу
#define API_BASE_URL "http://localhost:5066"

// Приклади для різних сценаріїв:
// Для ngrok: #define API_BASE_URL "https://abc123.ngrok.io"
// Для локальної мережі: #define API_BASE_URL "http://192.168.1.100:5066"
// Для Azure: #define API_BASE_URL "https://your-app.azurewebsites.net"
// Для AWS: #define API_BASE_URL "https://your-api.execute-api.region.amazonaws.com"
// Для Heroku: #define API_BASE_URL "https://your-app.herokuapp.com"
// Для власного домену: #define API_BASE_URL "https://api.yourdomain.com"

#define API_HEARTBEAT_ENDPOINT "/iot/heartbeat"
#define API_GATE_STATE_ENDPOINT "/iot/gate-state"

// Пристрій
#define DEVICE_SERIAL_NUMBER "ESP32-001"
#define GARAGE_ID 1

// Пін для Servo PWM сигналу
#define PIN_SERVO_PWM 5

// Кути для Servo (ворота)
#define SERVO_ANGLE_CLOSED 0
#define SERVO_ANGLE_OPEN 90

// Таймери (в мілісекундах)
#define HEARTBEAT_INTERVAL 30000
#define GATE_STATE_CHECK_INTERVAL 10000

// Налаштування WiFi підключення
#define WIFI_CONNECT_TIMEOUT 30000
#define WIFI_RECONNECT_DELAY 5000

#endif

