package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.SensorReadingDto

class SensorRepository(private val api: ApiService) {
    suspend fun latest(garageId: Int): Result<List<SensorReadingDto>> = runCatching {
        api.getLatestReadings(garageId).data
    }

    suspend fun alerts(garageId: Int): Result<List<SensorReadingDto>> = runCatching {
        api.getActiveAlerts(garageId).data
    }

    suspend fun history(garageId: Int, page: Int = 1, pageSize: Int = 50): Result<List<SensorReadingDto>> = runCatching {
        api.getSensorHistory(garageId, page, pageSize).data.items
    }
}
