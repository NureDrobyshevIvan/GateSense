package com.example.gatesensemobile.data.repository

import com.example.gatesensemobile.data.api.ApiService
import com.example.gatesensemobile.data.api.GateEventDto

class LogRepository(private val api: ApiService) {
    suspend fun gate(garageId: Int, page: Int = 1, pageSize: Int = 50): Result<List<GateEventDto>> = runCatching {
        api.getGateLogs(garageId, page, pageSize).data.items
    }
}
