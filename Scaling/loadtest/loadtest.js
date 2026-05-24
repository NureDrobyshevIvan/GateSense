// k6 load test для GateSense API
// Запуск з docker:
//   docker run --rm --network scaling_gsnet -v "C:/path/to/loadtest:/scripts" \
//     -e BASE_URL=http://nginx:80 grafana/k6 run /scripts/loadtest.js
//
// Сценарій: лінійно нарощуємо до 200 VU, тримаємо 30s на піку.
// Жодного sleep у iteration - кожен VU гатить запити максимально швидко,
// щоб вичавити з API максимум RPS. Так видно різницю між 1 і N replicas.

import http from 'k6/http';
import { check } from 'k6';

export const options = {
  stages: [
    { duration: '15s', target: 200 },
    { duration: '30s', target: 200 },
    { duration: '5s',  target: 0   },
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// Дзвонимо на /garages без токена - повернеться 401, але запит повноцінно
// пройшов через nginx + ASP.NET pipeline + auth middleware.
// Для load test нас цікавить throughput nginx + API, не бізнес-логіка.
export default function () {
  const res = http.get(`${BASE_URL}/garages`);
  check(res, {
    'no server error': (r) => r.status < 500,
  });
}
